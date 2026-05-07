using UnityEngine;
using UnityEngine.Events;

public class ShipCrack : MonoBehaviour
{
    [Header("Repair")]
    [SerializeField] private float repairRequired = 1f;
    [SerializeField] private GameObject sealedVisual;
    [SerializeField] private GameObject damagedVisual;

    [Header("Highlight")]
    [SerializeField] private Color dimColor       = new Color(0.5f, 0.0f, 0.0f);
    [SerializeField] private Color highlightColor = new Color(1.0f, 0.2f, 0.2f);
    [SerializeField] private float pulseSpeed     = 4f;
    [SerializeField] private float beaconRange    = 3f;
    [SerializeField] private float beaconMaxIntensity = 5f;
    [SerializeField] private bool  hideMeshUseLightOnly = true;   // если true — куб не виден, только пульсирующий свет

    public UnityEvent onSealed;

    private float _repairProgress;
    private bool  _isSealed;

    private Renderer[] _damagedRenderers;
    private Collider[]  _damagedColliders;
    private MaterialPropertyBlock _propBlock;
    private Light _beaconLight;
    private static readonly int BaseColor      = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColor  = Shader.PropertyToID("_EmissionColor");

    public bool  IsSealed           => _isSealed;
    public float NormalizedProgress => _repairProgress / repairRequired;

    private void Start()
    {
        if (damagedVisual == null)
        {
            var r = GetComponentInChildren<Renderer>(includeInactive: true);
            if (r != null) damagedVisual = r.gameObject;
        }

        if (damagedVisual != null)
        {
            _damagedRenderers = damagedVisual.GetComponentsInChildren<Renderer>(includeInactive: true);
            _damagedColliders = damagedVisual.GetComponentsInChildren<Collider>(includeInactive: true);
        }

        _propBlock = new MaterialPropertyBlock();

        // Создаём пульсирующий маяк — точечный свет, чтобы трещина была видна на тёмном корпусе
        var beaconGO = new GameObject("CrackBeacon");
        beaconGO.transform.SetParent(transform, false);
        beaconGO.transform.localPosition = Vector3.zero;
        _beaconLight = beaconGO.AddComponent<Light>();
        _beaconLight.type      = LightType.Point;
        _beaconLight.color     = highlightColor;
        _beaconLight.range     = beaconRange;
        _beaconLight.intensity = beaconMaxIntensity;

        Debug.Log($"[ShipCrack] '{name}' ready. renderers={_damagedRenderers?.Length}");

        ShowDamaged(true);
    }

    private void Update()
    {
        if (_isSealed || _damagedRenderers == null) return;

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
        Color color = Color.Lerp(dimColor, highlightColor, pulse);

        foreach (var r in _damagedRenderers)
        {
            if (r == null || !r.enabled) continue;
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColor,     color);
            _propBlock.SetColor(EmissionColor, highlightColor * pulse * 2f);
            r.SetPropertyBlock(_propBlock);
        }

        // Пульсация маяка
        if (_beaconLight != null)
            _beaconLight.intensity = (0.3f + 0.7f * pulse) * beaconMaxIntensity;
    }

    public void ApplyRepair(float amount)
    {
        if (_isSealed) return;
        _repairProgress = Mathf.Min(repairRequired, _repairProgress + amount);

        if (_repairProgress >= repairRequired)
            Seal();
    }

    public void Break()
    {
        _isSealed       = false;
        _repairProgress = 0f;
        ShowDamaged(true);
    }

    private void Seal()
    {
        _isSealed = true;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Маяк гасим — починили, не нужен
        if (_beaconLight != null)
            _beaconLight.enabled = false;

        if (sealedVisual != null)
            sealedVisual.SetActive(true);

        onSealed.Invoke();
        Debug.Log($"[ShipCrack] '{name}' ЗАПАЯНА!");
    }

    private void ShowDamaged(bool show)
    {
        // Рендереры — выключаем если включён режим "только свет"
        bool showRenderers = show && !hideMeshUseLightOnly;
        if (_damagedRenderers != null)
            foreach (var r in _damagedRenderers)
                if (r != null) r.enabled = showRenderers;

        // Коллайдеры оставляем включёнными — нужны для луча сварки
        if (_damagedColliders != null)
            foreach (var c in _damagedColliders)
                if (c != null) c.enabled = show;

        if (TryGetComponent<Collider>(out var ownCol))
            ownCol.enabled = show;
    }
}
