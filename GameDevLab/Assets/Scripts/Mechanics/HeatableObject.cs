using UnityEngine;
using UnityEngine.Events;

public class HeatableObject : MonoBehaviour
{
    [Header("Heat Settings")]
    [SerializeField] private float maxTemperature = 100f;
    [SerializeField] private float coolingRate = 5f;
    [SerializeField] private float overheatThreshold = 90f;

    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Gradient heatColorGradient;
    [SerializeField] private float maxEmissionIntensity = 3f;

    [Header("Events")]
    public UnityEvent onOverheat;
    public UnityEvent onCooledDown;

    private float _temperature;
    private bool _isOverheated;
    private MaterialPropertyBlock _propBlock;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public float Temperature => _temperature;
    public float NormalizedTemperature => _temperature / maxTemperature;
    public bool IsOverheated => _isOverheated;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (_temperature > 0f)
        {
            _temperature = Mathf.Max(0f, _temperature - coolingRate * Time.deltaTime);
            UpdateVisuals();
        }

        if (_isOverheated && _temperature < overheatThreshold * 0.5f)
        {
            _isOverheated = false;
            onCooledDown.Invoke();
        }
    }

    public void ApplyHeat(float amount)
    {
        if (_isOverheated) return;

        _temperature = Mathf.Min(maxTemperature, _temperature + amount * Time.deltaTime);
        UpdateVisuals();

        if (_temperature >= overheatThreshold && !_isOverheated)
        {
            _isOverheated = true;
            onOverheat.Invoke();
        }
    }

    private void UpdateVisuals()
    {
        if (targetRenderer == null) return;

        float t = NormalizedTemperature;

        Color baseColor;
        if (heatColorGradient != null)
        {
            baseColor = heatColorGradient.Evaluate(t);
        }
        else
        {
            // Fallback: оранжевый → красный
            baseColor = Color.Lerp(new Color(1f, 0.5f, 0f), new Color(1f, 0.05f, 0.05f), t);
        }

        Color heatColor = baseColor * (maxEmissionIntensity * t);

        targetRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(EmissionColor, heatColor);
        targetRenderer.SetPropertyBlock(_propBlock);
    }

    public void ResetHeat()
    {
        _temperature = 0f;
        _isOverheated = false;
        UpdateVisuals();
    }
}
