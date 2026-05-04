using UnityEngine;

namespace SpaceRepair.Asteroids
{
    [RequireComponent(typeof(Rigidbody))]
    public class AsteroidMover : MonoBehaviour
    {
        private Transform _target;
        private float _speed;
        private System.Action _onImpact;
        private System.Action _onDestroyed;
        private Rigidbody _rb;

        public void Init(Transform target, float speed, System.Action onImpact, System.Action onDestroyed)
        {
            _target = target;
            _speed = speed;
            _onImpact = onImpact;
            _onDestroyed = onDestroyed;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (_target == null) return;
            Vector3 direction = (_target.position - transform.position).normalized;
            _rb.linearVelocity = direction * _speed;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.transform == _target || col.transform.IsChildOf(_target))
            {
                _onImpact?.Invoke();
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            _onDestroyed?.Invoke();
        }
    }
}
