using SpaceRepair.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceRepair.UI
{
    public class ShipIntegrityUI : MonoBehaviour
    {
        [SerializeField] private ShipDamageSystem shipDamageSystem;
        [SerializeField] private Slider integritySlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Gradient integrityGradient;

        private void Start()
        {
            shipDamageSystem.onIntegrityChanged.AddListener(OnIntegrityChanged);
            OnIntegrityChanged(shipDamageSystem.NormalizedIntegrity);
        }

        private void OnIntegrityChanged(float normalized)
        {
            integritySlider.value = normalized;
            if (fillImage != null)
                fillImage.color = integrityGradient.Evaluate(normalized);
        }
    }
}
