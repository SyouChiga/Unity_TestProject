using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR                // ここ
using UnityEditor;              // ここ    
#endif
public class MoveBox : MonoBehaviour
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
    BoxCollider _collider;

    [SerializeField]
    Collider save_collider;

    [SerializeField]
    List<Collider> flip_back_ = new List<Collider>();

    public class ColliderSave
    {
        public Collider colider;
        public bool is_hit;
        public void HitTrue() => is_hit = true;
        public void HitFalse() => is_hit = false;
    }

    [SerializeField]
    List<ColliderSave> flip_save_ = new List<ColliderSave>();


    // Use this for initialization
    void Start()
    {
        flip_back_ = new List<Collider>();
        flip_save_ = new List<ColliderSave>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float moveDistance = _speed * Time.deltaTime;
        MoveSimulation(_moveDir, moveDistance);
    }

    private void Update()
    {
        MoveUpdate();
        //RotationUpdate();
        float moveDistance = _speed * Time.deltaTime;
        _moveDir = _moveDir.normalized;
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


        Vector3 size = transform.localScale;
        size.x *= _collider.size.x;
        size.y *= _collider.size.y ;
        size.z *= _collider.size.z ;
        Vector3 pos = transform.position + _collider.center;
        //指定した半径の球に当たるコライダー
        Collider[] colliders = Physics.OverlapBox(pos, size, transform.rotation, CollisionLayerMask);

        foreach (var over_col in colliders)
        {
            if (over_col != _collider)
            {
                var find_col = flip_save_.Find(p => p.colider == over_col);
                if (find_col == null)
                {
                    ColliderSave save = new ColliderSave();
                    save.colider = over_col;
                    flip_save_.Add(save);
                }
            }
        }

        bool isCollisionSphere = false;
        int cnt = 0;
        if (flip_save_.Count > 0)
        {
            Vector3 pushBackVectorAll = Vector3.zero;
            for (int i = 0; i < flip_save_.Count;i++)
            {
                cnt++;
                if (cnt >= 100)
                {
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
                    return 0.0f;
#else
        Application.Quit ();
#endif
                }
                Collider targetCollider = flip_save_[i].colider;
                if (_collider == targetCollider)
                {
                    continue;
                }


                //押し出す方向と長さを取得
                Vector3 pushBackVector;
                float pushBackDistance;
                bool isCollision = false;
                isCollision =
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
                        transform.position += new Vector3(updatedPostion.x, 0.0f, updatedPostion.z);
                        moveDir = Vector3.ProjectOnPlane(moveDir, -pushBackVector);
                        isCollisionSphere = true;
                        save_collider = targetCollider;
                    }

                }

                foreach(var obj_col in flip_save_)
                {

                    isCollision = Physics.ComputePenetration(
                                        _collider,
                                        _collider.transform.position,
                                        _collider.transform.rotation,
                                        obj_col.colider,
                                        obj_col.colider.transform.position,
                                        obj_col.colider.transform.rotation,
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
                            transform.position += new Vector3(updatedPostion.x, 0.0f, updatedPostion.z);
                            if(moveDir.magnitude >= 0.001f)obj_col.HitTrue();
                            isCollisionSphere = true;
                            continue;
                        }

                    }
                }



            }

        }

        bool check = false;
        int chack_count = 0;
        foreach (var save in flip_save_)
        {
            if(save.is_hit == true)
            {
                chack_count++;
            }
        }
        if(chack_count >= flip_save_.Count && flip_save_.Count > 0)
        {
            check = true;
        }
        if(check == true)
        {
            _collider.transform.position = save_pos;
        }
        foreach (var save in flip_save_)
        {
            save.HitFalse();
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
