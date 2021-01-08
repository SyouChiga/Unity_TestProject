using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Move : MonoBehaviour
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
        SphereCollider _collider;



        // Use this for initialization
        void Start()
        {
    
        }

        // Update is called once per frame
        void FixedUpdate()
        {
        MoveUpdate();
        RotationUpdate();
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
            _collider.transform.position += moveDir * moveDistance;
            Vector3 prevPosition = _collider.transform.position + moveDir * moveDistance;
            //オブジェクトのスケールを掛けて衝突計算で使用するする半径を_colliderのサイズとあわせる
            float simulationRadius = _collider.radius * _collider.transform.localScale.x;
            //指定した半径の球に当たるコライダー
            Collider[] colliders = Physics.OverlapSphere(_collider.transform.position, simulationRadius, CollisionLayerMask);
            bool isCollisionSphere = false;
        if (colliders.Length > 0)
        {
            Vector3 pushBackVectorAll = Vector3.zero;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider targetCollider = colliders[i];
                if (_collider == targetCollider)
                    continue;

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
                if (isCollision)
                {
                    resultMoveDistance = pushBackDistance;
                    if (pushBackDistance >= 0.0001)
                    {
                        Vector3 updatedPostion = pushBackVector * pushBackDistance;
                        moveDir = Vector3.ProjectOnPlane(moveDir, -pushBackVector);
                        transform.position += new Vector3(updatedPostion.x , 0.0f, updatedPostion.z);
                        
                        isCollisionSphere = true;

                    }
                    else
                    {
                        isCollisionSphere = false; 
                        //_collider.transform.position = prevPosition + moveDir * moveDistance;
                    }

                }

  
            }

        }

        ////押し出し処理をしていない場合はSphereCastで球の移動分の衝突判定
        //if (!isCollisionSphere)
        //{
        //    RaycastHit hitInfo;
        //    //押し出し処理のあと同じ半径で衝突判定をすると貫通していたため、少し小さめの半径で判定(0.99f)
        //    if (Physics.SphereCast(prevPosition, simulationRadius * 0.99f, moveDir, out hitInfo, moveDistance, CollisionLayerMask))
        //    {
        //        resultMoveDistance = hitInfo.distance;
        //        //当たった位置まで座標を動かす
        //        Vector3 moveVector = moveDir * resultMoveDistance;
        //        Vector3 pos = prevPosition + moveVector;
        //        //_collider.transform.position = pos;
        //        //壁を滑らせるような動きをさせるため、次回移動方向を壁に沿った方向に変更
        //        nextMoveDir = Vector3.ProjectOnPlane(moveDir, hitInfo.normal);
        //        _collider.transform.position = prevPosition + nextMoveDir * moveDistance;
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
        //    else
        //    {
        //        //衝突していない場合は通常の移動計算
        //        _collider.transform.position = prevPosition + moveDir * moveDistance;
        //    }
        //}
        return resultMoveDistance;
        }
    
}
