using UnityEngine;
using UnityEngine.UI;

public class ShipIntegrityUI : MonoBehaviour
{
    [SerializeField] private ShipDamageSystem shipDamageSystem;
    [SerializeField] private Image fillImage;

    private readonly Color _highColor = new Color(0.2f, 0.9f, 0.2f);
    private readonly Color _midColor  = new Color(1f,   0.8f, 0f);
    private readonly Color _lowColor  = new Color(0.9f, 0.1f, 0.1f);

    private void Start()
    {
        if (shipDamageSystem == null) return;
        shipDamageSystem.onIntegrityChanged.AddListener(OnIntegrityChanged);
        OnIntegrityChanged(shipDamageSystem.NormalizedIntegrity);
    }

    private void OnIntegrityChanged(float t)
    {
        if (fillImage == null) return;

        // Масштабируем Fill по X: 1 = 100% здоровья, 0 = корабль разрушен
        // Pivot у Fill должен быть X=0 (левый край)
        fillImage.transform.localScale = new Vector3(t, 1f, 1f);
        fillImage.color = t > 0.5f ? _highColor
                        : t > 0.25f ? _midColor
                        : _lowColor;
    }
}
