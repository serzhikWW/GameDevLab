using UnityEngine;
using UnityEngine.Events;

namespace SpaceRepair.Core
{
    public class ShipDamageSystem : MonoBehaviour
    {
        [Header("Ship Health")]
        [SerializeField] private float maxHullIntegrity = 100f;
        [SerializeField] private float currentHullIntegrity = 100f;

        [Header("Cracks")]
        [SerializeField] private ShipCrack[] cracks;
        [SerializeField] private float crackDamagePerSecond = 2f;

        [Header("Events")]
        public UnityEvent<float> onIntegrityChanged;   // 0..1 normalized
        public UnityEvent onShipDestroyed;
        public UnityEvent onShipFullyRepaired;

        private bool _destroyed;

        public float NormalizedIntegrity => currentHullIntegrity / maxHullIntegrity;

        private void Update()
        {
            if (_destroyed) return;

            int openCracks = CountOpenCracks();
            if (openCracks > 0)
            {
                TakeDamage(crackDamagePerSecond * openCracks * Time.deltaTime);
            }

            if (AllCracksClosed() && currentHullIntegrity >= maxHullIntegrity)
                onShipFullyRepaired.Invoke();
        }

        public void TakeDamage(float amount)
        {
            if (_destroyed) return;
            currentHullIntegrity = Mathf.Max(0f, currentHullIntegrity - amount);
            onIntegrityChanged.Invoke(NormalizedIntegrity);

            if (currentHullIntegrity <= 0f)
            {
                _destroyed = true;
                onShipDestroyed.Invoke();
            }
        }

        public void Repair(float amount)
        {
            currentHullIntegrity = Mathf.Min(maxHullIntegrity, currentHullIntegrity + amount);
            onIntegrityChanged.Invoke(NormalizedIntegrity);
        }

        private int CountOpenCracks()
        {
            int count = 0;
            foreach (var crack in cracks)
                if (crack != null && !crack.IsSealed) count++;
            return count;
        }

        private bool AllCracksClosed()
        {
            foreach (var crack in cracks)
                if (crack != null && !crack.IsSealed) return false;
            return true;
        }
    }
}
