using SpaceRepair.Mechanics;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceRepair.UI
{
    public class HeatUI : MonoBehaviour
    {
        [SerializeField] private HeatableObject toolHeat;
        [SerializeField] private Slider heatSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Gradient sliderGradient;
        [SerializeField] private GameObject overheatWarning;

        private void Update()
        {
            if (toolHeat == null) return;

            float t = toolHeat.NormalizedTemperature;
            heatSlider.value = t;

            if (fillImage != null)
                fillImage.color = sliderGradient.Evaluate(t);

            if (overheatWarning != null)
                overheatWarning.SetActive(toolHeat.IsOverheated);
        }
    }
}
