using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private int _hp;
    [SerializeField] private int _power;
    [SerializeField] private float _speed;
    
    private Rigidbody2D _rigidbody;
    private InputActions _inputActions;
    private Vector2 _moveDirection;
    
    public override void Spawned()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        // 上下左右移動のInputActionを取得
        _inputActions = new InputActions();
        _inputActions.Enable();
        _inputActions.Player.Move.started += OnMove;
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public override void FixedUpdateNetwork()
    {
        _rigidbody.linearVelocity = _moveDirection * _speed;
    }

    private void OnDestroy()
    {
        _inputActions?.Disable();
    }
}
