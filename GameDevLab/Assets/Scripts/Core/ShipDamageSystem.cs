using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipDamageSystem : MonoBehaviour
{
    [Header("Ship Health")]
    [SerializeField] private float maxHullIntegrity = 100f;
    [SerializeField] private float currentHullIntegrity = 100f;

    [Header("Cracks")]
    [SerializeField] private ShipCrack[] cracks;

    [Header("Random Setup")]
    [SerializeField] private int minActiveCracks = 2;
    [SerializeField] private int maxActiveCracks = 5;

    [Header("Events")]
    public UnityEvent<float> onIntegrityChanged;
    public UnityEvent onShipDestroyed;
    public UnityEvent onShipFullyRepaired;

    private bool _destroyed;
    private bool _repaired;
    private bool _hadCracksAtStart;
    private int _lastReportedSealed = -1;

    public float NormalizedIntegrity => currentHullIntegrity / maxHullIntegrity;

    private void Start()
    {
        RandomizeActiveCracks();

        _hadCracksAtStart = cracks != null && cracks.Length > 0;
        Debug.Log($"[ShipDamageSystem] Старт: трещин на корабле = {cracks?.Length ?? 0}, HP = {currentHullIntegrity}/{maxHullIntegrity}");
        if (!_hadCracksAtStart)
            Debug.LogWarning("[ShipDamageSystem] Внимание: ни одной трещины не назначено в массиве 'cracks'! Победа не сработает.");
    }

    private void RandomizeActiveCracks()
    {
        if (cracks == null || cracks.Length == 0) return;

        int maxAvailable = cracks.Length;
        int target = Random.Range(
            Mathf.Min(minActiveCracks, maxAvailable),
            Mathf.Min(maxActiveCracks, maxAvailable) + 1);

        // Перемешиваем (Fisher-Yates)
        var shuffled = new List<ShipCrack>(cracks);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        // Лишние — выключаем GameObject полностью
        for (int i = target; i < shuffled.Count; i++)
            if (shuffled[i] != null)
                shuffled[i].gameObject.SetActive(false);

        // В массиве оставляем только активные
        var active = new List<ShipCrack>(target);
        for (int i = 0; i < target; i++)
            active.Add(shuffled[i]);
        cracks = active.ToArray();

        Debug.Log($"[ShipDamageSystem] Случайно выбрано {target} трещин для починки");
    }

    private void Update()
    {
        if (_destroyed || _repaired) return;
        if (!_hadCracksAtStart) return;

        // Логируем прогресс при изменении количества запаянных трещин
        int sealed_ = CountSealed();
        if (sealed_ != _lastReportedSealed)
        {
            _lastReportedSealed = sealed_;
            Debug.Log($"[ShipDamageSystem] Прогресс: {sealed_}/{cracks.Length} трещин запаяно");
        }

        // Победа: все трещины запаяны
        if (sealed_ == cracks.Length)
        {
            _repaired = true;
            Debug.Log("[ShipDamageSystem] ВСЕ ТРЕЩИНЫ ЗАПАЯНЫ — ПОБЕДА!");
            onShipFullyRepaired.Invoke();
        }
    }

    public void TakeDamage(float amount)
    {
        if (_destroyed) return;
        currentHullIntegrity = Mathf.Max(0f, currentHullIntegrity - amount);
        onIntegrityChanged.Invoke(NormalizedIntegrity);

        if (currentHullIntegrity <= 0f)
        {
            _destroyed = true;
            Debug.Log("[ShipDamageSystem] КОРАБЛЬ УНИЧТОЖЕН — GAME OVER");
            onShipDestroyed.Invoke();
        }
    }

    public void Repair(float amount)
    {
        currentHullIntegrity = Mathf.Min(maxHullIntegrity, currentHullIntegrity + amount);
        onIntegrityChanged.Invoke(NormalizedIntegrity);
    }

    private int CountSealed()
    {
        int n = 0;
        foreach (var crack in cracks)
            if (crack != null && crack.IsSealed) n++;
        return n;
    }
}
