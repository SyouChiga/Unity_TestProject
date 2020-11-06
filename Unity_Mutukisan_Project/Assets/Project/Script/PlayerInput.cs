//==========================================-
//作成者 : 千賀翔
//
//==========================================-
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerのInputクラス
/// </summary>
public class PlayerInput : MonoBehaviour
{
    /// <summary>
    /// 入力値
    /// </summary>
    [SerializeField]
    private Vector2 move_input_ = Vector2.zero;

    /// <summary>
    /// 移動量
    /// </summary>
    [SerializeField]
    private Vector3 move_velocity_ = Vector3.zero;

    /// <summary>
    /// 移動スピード
    /// </summary>
    [SerializeField]
    private float move_speed_ = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveUpdate();
        RotationUpdate();

        move_velocity_ = Vector3.zero;
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void MoveUpdate()
    {
        Vector3 camera_forward, camera_right;
        camera_forward = Camera.main.transform.forward;
        camera_right = Camera.main.transform.right;


        move_velocity_ = move_input_.x * camera_right + move_input_.y * camera_forward;
        move_velocity_.y = 0.0f;
        //今回はTransformでの移動
        transform.position += move_velocity_ * move_speed_;
    }

    private void RotationUpdate()
    {
        if (move_velocity_ == Vector3.zero) return;
        transform.rotation = Quaternion.LookRotation(move_velocity_);
    }

    /// <summary>
    /// InputActioのMoveをバインド
    /// </summary>
    /// <param name="input_value"></param>
    private void OnMove(InputValue input_value)
    {
        move_input_ = input_value.Get<Vector2>();
    }
}
