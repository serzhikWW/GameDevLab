using UnityEngine;
using UnityEngine.UI;

public class HeatUI : MonoBehaviour
{
    [SerializeField] private HeatableObject toolHeat;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject overheatWarning;

    private readonly Color _normalColor   = new Color(1f, 0.6f, 0f);
    private readonly Color _overheatColor = new Color(1f, 0.1f, 0.1f);

    private void Update()
    {
        if (toolHeat == null || fillImage == null) return;

        float t = toolHeat.NormalizedTemperature;

        // Масштабируем Fill по X: 0 = пусто, 1 = полностью заполнено
        // Pivot у Fill должен быть X=0 (левый край), иначе будет сжиматься к центру
        fillImage.transform.localScale = new Vector3(t, 1f, 1f);
        fillImage.color = toolHeat.IsOverheated ? _overheatColor : _normalColor;

        if (overheatWarning != null)
            overheatWarning.SetActive(toolHeat.IsOverheated);
    }
}
