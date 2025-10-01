using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, ICharacter
{
    [SerializeField] private float _speed;
    private GameManager _gameManager;
    private PlayerStatusEffectManager _playerStatusEffectManager;
    private float _hp = 100;
    private Rigidbody2D _rigidbody;
    private InputActions _inputActions;
    private Vector2 _moveDirection;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private CancellationTokenSource _cts;
    private CancellationToken _token;
    
    public override async void Spawned()
    {
        _cts = new CancellationTokenSource();
        _token = _cts.Token;
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _playerStatusEffectManager = this.GetComponent<PlayerStatusEffectManager>();
        _token = this.GetCancellationTokenOnDestroy();
        
        // 上下左右移動のInputActionを取得
        _inputActions = new InputActions();
        _inputActions.Enable();
        _inputActions.Player.Move.started += OnMove;
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        
        _gameManager = GameManager.Instance;
        await UniTask.WaitUntil(() => _gameManager.IsSpawned, cancellationToken: _token);
        _gameManager.AddPlayerHp(_hp);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!_gameManager.IsAllPlayerJoined || _playerStatusEffectManager.PlayerStatusEffects == StatusEffect.Paralysis)
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
        if (HasStateAuthority && _gameManager.CurrentGameState == GameState.Playing)
        {
            _gameManager.RpcDecreasePlayerHpGauge(damage);
        }
    }

    /// <summary>
    /// ダメージを喰らった時に，赤色にする
    /// </summary>
    public async UniTaskVoid ChangeDamageColor()
    {
        /*
        Debug.Log("red");
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        _token = _cts.Token;
        */
        
        _spriteRenderer.color = Color.red;
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: _token);
        _spriteRenderer.color = Color.white;
    }
}
