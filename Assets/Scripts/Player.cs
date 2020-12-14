using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    
    [SerializeField] private float _speed = 1.0f;
    [SerializeField] private float _radius = 5.0f;
    [SerializeField] private float _damagePerSecond = 30.0f;
    [SerializeField] private float _maxHealth = 100.0f;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _receivingDamageSprite;
    [SerializeField] private Sprite _deadSprite;

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public float Radius
    {
        get => _radius;
        set => _radius = value;
    }

    public float DamagePerSecond
    {
        get => _damagePerSecond;
        set => _damagePerSecond = value;
    }

    public float MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    private float _currentHealth;

    private SpriteRenderer _spriteRenderer;
    private Dictionary<Player, LineRenderer> _lineRenderers = new Dictionary<Player, LineRenderer>();
    private float _playerRadius;

    public float PlayerRadius { get => _playerRadius; }

    public Player CurrentTargetToFollow { get; set; }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        var spriteBounds = _spriteRenderer.sprite.bounds.extents;
        _playerRadius = Mathf.Max(spriteBounds.x, spriteBounds.y);
        Restart();
    }

    public void AssignLineRenderer(Player enemy, LineRenderer lineRenderer)
    {
        if (!_lineRenderers.ContainsKey(enemy))
            _lineRenderers[enemy] = lineRenderer;
    }

    public void ReceiveDamage(float damage)
    {
        _currentHealth -= damage;
        _spriteRenderer.sprite = _receivingDamageSprite;
    }

    public void BecomeNormal()
    {
        if (_spriteRenderer.sprite == _receivingDamageSprite)
            _spriteRenderer.sprite = _normalSprite;
    }

    public bool IsAlive()
    {
        return _currentHealth > 0;
    }

    public void Restart()
    {
        _currentHealth = _maxHealth;
        _spriteRenderer.sprite = _normalSprite;
    }

    public void DrawShoot(Player enemy)
    {
        if (!_lineRenderers.ContainsKey(enemy))
        {
            Debug.LogWarning($"No lineRenderer from {gameObject.name} to {enemy.gameObject.name}");
            return;
        }
        var lineRenderer = _lineRenderers[enemy];
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { transform.position, enemy.transform.position });
        lineRenderer.useWorldSpace = true;
    }

    public void UndrawShoot(Player enemy)
    {
        if (_lineRenderers.ContainsKey(enemy) && _lineRenderers[enemy].positionCount > 0)
            _lineRenderers[enemy].positionCount = 0;
    }

    public void Die()
    {
        _spriteRenderer.sprite = _deadSprite;
        foreach (var lineRenderer in _lineRenderers.Values)
            lineRenderer.positionCount = 0;
    }

    public void Update()
    {
        if (IsAlive() && CurrentTargetToFollow != null)
        {
            var dist = ((Vector2)transform.position - (Vector2)CurrentTargetToFollow.transform.position).magnitude;
            var multiplier = (dist - CurrentTargetToFollow.PlayerRadius - PlayerRadius) / dist;
            var targetPosition = transform.position + (CurrentTargetToFollow.transform.position - transform.position) * multiplier;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, _speed * Time.deltaTime);
        }
    }


}
