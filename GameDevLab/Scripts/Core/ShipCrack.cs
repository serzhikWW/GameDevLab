using UnityEngine;
using UnityEngine.Events;

namespace SpaceRepair.Core
{
    public class ShipCrack : MonoBehaviour
    {
        [SerializeField] private float repairRequired = 1f;
        [SerializeField] private GameObject sealedVisual;
        [SerializeField] private GameObject damagedVisual;

        public UnityEvent onSealed;

        private float _repairProgress;
        private bool _isSealed;

        public bool IsSealed => _isSealed;
        public float NormalizedProgress => _repairProgress / repairRequired;

        private void Start() => SetVisuals(false);

        public void ApplyRepair(float amount)
        {
            if (_isSealed) return;

            _repairProgress = Mathf.Min(repairRequired, _repairProgress + amount);
            if (_repairProgress >= repairRequired)
                Seal();
        }

        public void Break()
        {
            _isSealed = false;
            _repairProgress = 0f;
            SetVisuals(false);
        }

        private void Seal()
        {
            _isSealed = true;
            SetVisuals(true);
            onSealed.Invoke();
        }

        private void SetVisuals(bool sealed_)
        {
            if (sealedVisual != null) sealedVisual.SetActive(sealed_);
            if (damagedVisual != null) damagedVisual.SetActive(!sealed_);
        }
    }
}
