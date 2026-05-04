using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AsteroidMover : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private System.Action _onImpact;
    private Rigidbody _rb;
    private bool _handled;

    private float _repelTimer;
    private Vector3 _repelDirection;

    public void Init(Transform target, float speed, System.Action onImpact, System.Action _unused)
    {
        _target = target;
        _speed = speed;
        _onImpact = onImpact;
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        Debug.Log($"[AsteroidMover] Init: target='{target?.name}', speed={speed}");
    }

    public void Repel(Vector3 worldDirection, float force)
    {
        _repelTimer = 2f;
        _repelDirection = worldDirection.normalized;
        _rb.linearVelocity = _repelDirection * force;
        Debug.Log($"[AsteroidMover] Repel! dir={_repelDirection}, force={force}, velocity={_rb.linearVelocity}");
    }

    private void FixedUpdate()
    {
        if (_target == null || _rb == null) return;

        if (_repelTimer > 0f)
        {
            _repelTimer -= Time.fixedDeltaTime;
            _rb.linearVelocity = Vector3.Lerp(
                _rb.linearVelocity,
                (_target.position - transform.position).normalized * _speed,
                0.02f);
        }
        else
        {
            _rb.linearVelocity = (_target.position - transform.position).normalized * _speed;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (_handled) return;
        Debug.Log($"[AsteroidMover] Collision с '{col.gameObject.name}', target='{_target?.name}'");
        if (col.transform == _target || col.transform.IsChildOf(_target))
        {
            _handled = true;
            Debug.Log("[AsteroidMover] -> Попал в корабль! Уничтожаю.");
            _onImpact?.Invoke();
            Destroy(gameObject);
        }
    }
}
