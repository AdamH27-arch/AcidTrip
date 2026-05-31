using UnityEngine;

public class AnimatedEnemy : Enemy
{
    
    private Vector2 _lastMoveDirection = Vector2.down;
    [SerializeField] private Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            Debug.Log("Child: " + child.name);
        }
        
        


    }

    protected override void MoveTowardPlayer()
    {
        if (Player == null) return;

        Vector2 direction = ((Vector2)Player.position - Rb.position).normalized;
        Rb.linearVelocity = direction * Data.moveSpeed;

        if (direction.sqrMagnitude > 0.01f)
            _lastMoveDirection = direction;
            Debug.Log("MoveX: " + _lastMoveDirection.x + " MoveY: " + _lastMoveDirection.y);

        if (_animator != null)
        {
            Debug.Log("animator updated");
            _animator.SetFloat("MoveX", _lastMoveDirection.x);
            _animator.SetFloat("MoveY", _lastMoveDirection.y);
            _animator.SetFloat("Speed", _lastMoveDirection.magnitude);
        }
    }
}