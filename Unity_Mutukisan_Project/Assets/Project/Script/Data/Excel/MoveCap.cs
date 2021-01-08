using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR                // ここ
using UnityEditor;              // ここ    
#endif         
public class MoveCap : MonoBehaviour
{

    //衝突判定用コリジョンマスク
    const int CollisionLayerMask = 1;

    /// <summary>
    /// 移動速度
    /// </summary>
    [SerializeField]
    float _speed = 5f;

    /// <summary>
    /// 移動方向
    /// </summary>
    [SerializeField]
    Vector3 _moveDir = Vector3.zero;

    /// <summary>
    /// 移動方向
    /// </summary>
    [SerializeField]
    Vector3 _moveDirInput = Vector3.zero;

    /// <summary>
    /// 判定用コライダー
    /// </summary>
    [SerializeField]
    CapsuleCollider _collider;

    [SerializeField]
    Collider save_collider;

    [SerializeField]
    List<Collider> flip_back_ = new List<Collider>();



    // Use this for initialization
    void Start()
    {
        flip_back_ = new List<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveUpdate();
        //RotationUpdate();
        float moveDistance = _speed * Time.deltaTime;
        _moveDir = _moveDir.normalized;
        MoveSimulation(_moveDir, moveDistance);
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void MoveUpdate()
    {
        Vector3 camera_forward, camera_right;
        camera_forward = Camera.main.transform.forward;
        camera_right = Camera.main.transform.right;


        _moveDir = _moveDirInput.x * camera_right + _moveDirInput.y * camera_forward;
        _moveDir.y = 0.0f;
    }

    private void RotationUpdate()
    {
        if (_moveDir == Vector3.zero) return;
        transform.rotation = Quaternion.LookRotation(_moveDir);
    }

    /// <summary>
    /// InputActioのMoveをバインド
    /// </summary>
    /// <param name="input_value"></param>
    private void OnMove(InputValue input_value)
    {

        _moveDirInput = input_value.Get<Vector2>();
    }

    /// <summary>
    /// 移動時の衝突シミュレーションなるべく移動量分動かすように実装
    /// </summary>
    /// <param name="moveDir">ノーマライズ済み移動方向</param>
    /// <param name="moveDistance">移動距離</param>
    /// <param name="simulationMax">計算回数</param>
    void MoveSimulation(Vector3 moveDir, float moveDistance, int simulationMax = 1)
    {
        //移動量がなくなるかsimulationMax分計算した場合は終了
        for (int i = 0; i < simulationMax; i++)
        {
            Vector3 nextMoveDir;
            float lastMoveDistance = MoveCollisionCalc(moveDir, moveDistance, out nextMoveDir);
        }

    }

    /// <summary>
    /// 移動時の衝突計算
    /// </summary>
    /// <param name="moveDir">ノーマライズ済み移動方向</param>
    /// <param name="moveDistance">移動距離</param>
    /// <param name="nextMoveDir">ノーマライズ済み次回移動方向</param>
    /// <returns></returns>
    float MoveCollisionCalc(Vector3 moveDir, float moveDistance, out Vector3 nextMoveDir)
    {
        float resultMoveDistance = moveDistance;
        nextMoveDir = moveDir;
        Vector3 save_pos = _collider.transform.position;
        Vector3 prevPosition = _collider.transform.position;
        _collider.transform.position += moveDir * moveDistance;


        float scale = Mathf.Max(_collider.transform.localScale.x, _collider.transform.localScale.z);
        float radius = _collider.radius * scale + 0.01f;
        float _height = (((_collider.height * _collider.transform.localScale.y) / 2.0f) - radius);
        Debug.Log(_collider.transform.localScale.y);
        Vector3 start_pos = Vector3.zero;
        start_pos = _collider.center + _collider.transform.position + ((_height * _collider.transform.up));
        Vector3 end_pos = Vector3.zero;

        end_pos = _collider.center + _collider.transform.position + ((_height * (_collider.transform.up * -1.0f)));

        //指定した半径の球に当たるコライダー
        Collider[] colliders = Physics.OverlapCapsule(start_pos, end_pos, radius, CollisionLayerMask);

        foreach(var over_col in colliders)
        {
            var find_col = flip_back_.Find(p => p == over_col);
            if (find_col == null && over_col != _collider) flip_back_.Add(over_col);
        }

        bool isCollisionSphere = false;
        int cnt = 0;
        if (flip_back_.Count > 0)
        {
            Vector3 pushBackVectorAll = Vector3.zero;
            for (int i = 0; i < flip_back_.Count;)
            {
                cnt++;
                if(cnt >= 100)
                {
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
                    return 0.0f;
#else
        Application.Quit ();
#endif
                }
                Collider targetCollider = flip_back_[i];
                if (_collider == targetCollider)
                {
                    i++;
                    continue;
                }


                //押し出す方向と長さを取得
                Vector3 pushBackVector;
                float pushBackDistance;

                bool isCollision =
                    Physics.ComputePenetration(
                        _collider,
                        _collider.transform.position,
                        _collider.transform.rotation,
                        targetCollider,
                        targetCollider.transform.position,
                        targetCollider.transform.rotation,
                        out pushBackVector,
                        out pushBackDistance
                    );


                //めり込んでいた場合
                if (isCollision && pushBackDistance >= 0.001f)
                {
                    resultMoveDistance = pushBackDistance;
                    //if (pushBackDistance >= 0.0001)
                    {
                        Vector3 updatedPostion = pushBackVector * pushBackDistance;
                        moveDir = Vector3.ProjectOnPlane(moveDir, -pushBackVector);
                        transform.position += new Vector3(updatedPostion.x, 0.0f, updatedPostion.z);

                        isCollisionSphere = true;
                        save_collider = targetCollider;
                        i = 0;
                        continue;
                    }

                }
                else
                {

                    i++;
                }


            }

        }


        //if (!isCollisionSphere)
        //{
        //    RaycastHit hitInfo;
        //    Vector3 dir = (_collider.transform.position - save_pos).normalized;
        //    float distance = (_collider.transform.position - save_pos).magnitude;
        //    //押し出し処理のあと同じ半径で衝突判定をすると貫通していたため、少し小さめの半径で判定(0.99f)
        //    if (Physics.SphereCast(prevPosition, radius * 0.99f, dir, out hitInfo, distance, CollisionLayerMask))
        //    {
        //        resultMoveDistance = hitInfo.distance;
        //        //当たった位置まで座標を動かす
        //        Vector3 pos = save_pos;
        //        _collider.transform.position = pos;
        //        //壁を滑らせるような動きをさせるため、次回移動方向を壁に沿った方向に変更
        //        nextMoveDir = Vector3.ProjectOnPlane(moveDir, hitInfo.normal);

        //        //もともとの移動方向と逆向きに進もうとしている場合
        //        if (nextMoveDir.sqrMagnitude > float.Epsilon && Vector3.Dot(nextMoveDir, _moveDir) < 0f)
        //        {
        //            //次回移動方向にもともとの移動方向を設定
        //            nextMoveDir = _moveDir;
        //        }
        //        else
        //        {
        //            nextMoveDir = Vector3.Normalize(nextMoveDir);
        //        }
        //    }
        //}
        return resultMoveDistance;
    }
}
