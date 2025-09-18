using System;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, ICharacter
{
    [SerializeField] private float _speed;
    private PlayerStatusEffectManager _playerStatusEffectManager;
    private float _hp = 100;
    private Rigidbody2D _rigidbody;
    private InputActions _inputActions;
    private Vector2 _moveDirection;
    
    public override async void Spawned()
    {
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _playerStatusEffectManager = this.GetComponent<PlayerStatusEffectManager>();
        
        // 上下左右移動のInputActionを取得
        _inputActions = new InputActions();
        _inputActions.Enable();
        _inputActions.Player.Move.started += OnMove;
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;

        await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.IsSpawned);
        GameManager.Instance.AddPlayerHP(_hp);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GameManager.Instance.CurrentGameState != GameState.Playing || _playerStatusEffectManager.PlayerStatusEffects.Contains(StatusEffect.Paralysis))
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }
    
        
        _rigidbody.linearVelocity = _moveDirection * _speed;
    }
    
    
    private void OnDestroy()
    {
        _inputActions?.Disable();
    }

    public void Damage(float damage)
    {
        if (HasStateAuthority)
        {
            GameManager.Instance.RpcDecreasePlayerHpGauge(damage);
        }
    }
}
