using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private Transform shipTarget;
    [SerializeField] private float spawnRadius = 25f;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float asteroidSpeed = 2f;          // было 4 — стало в 2 раза медленнее
    [SerializeField] private int maxAsteroids = 6;

    [Header("Damage")]
    [SerializeField] private ShipDamageSystem shipDamageSystem;
    [SerializeField] private float impactDamage = 20f;

    [Header("Warning Light")]
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.1f);
    [SerializeField] private float warningRange = 10f;
    [SerializeField] private float warningIntensity = 4f;

    [Header("Random Variation")]
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 6f;
    [SerializeField] private int   minMaxAsteroids  = 4;
    [SerializeField] private int   maxMaxAsteroids  = 8;

    private float _timer;
    private float _logTimer;
    private readonly List<GameObject> _active = new();

    private void Start()
    {
        // Каждый рестарт сцены даёт новые параметры волны
        spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        maxAsteroids  = Random.Range(minMaxAsteroids, maxMaxAsteroids + 1);

        Debug.Log($"[AsteroidSpawner] Start: prefabs={asteroidPrefabs?.Length}, target='{shipTarget?.name}', interval={spawnInterval:F1}, max={maxAsteroids}");
    }

    private void Update()
    {
        _active.RemoveAll(a => a == null);

        _logTimer += Time.deltaTime;
        if (_logTimer >= 3f)
        {
            _logTimer = 0f;
            Debug.Log($"[AsteroidSpawner] Активно={_active.Count}/{maxAsteroids} | timer={_timer:F1}/{spawnInterval}");
        }

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval && _active.Count < maxAsteroids)
        {
            _timer = 0f;
            SpawnAsteroid();
        }
    }

    private void SpawnAsteroid()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0)
        {
            Debug.LogWarning("[AsteroidSpawner] Нет префабов астероидов!");
            return;
        }
        if (shipTarget == null)
        {
            Debug.LogWarning("[AsteroidSpawner] shipTarget не назначен!");
            return;
        }

        Vector3 spawnPos = Random.onUnitSphere * spawnRadius + shipTarget.position;
        var prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        var asteroid = Instantiate(prefab, spawnPos, Random.rotation);

        // Подсветка астероида — оранжевый светящийся ореол
        AddWarningLight(asteroid);

        var mover = asteroid.AddComponent<AsteroidMover>();
        mover.Init(shipTarget, asteroidSpeed, OnAsteroidImpact, null);

        _active.Add(asteroid);
        Debug.Log($"[AsteroidSpawner] Заспавнен астероид #{_active.Count} в {spawnPos}");
    }

    private void AddWarningLight(GameObject asteroid)
    {
        var lightGO = new GameObject("WarningLight");
        lightGO.transform.SetParent(asteroid.transform, false);
        lightGO.transform.localPosition = Vector3.zero;

        var light = lightGO.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = warningColor;
        light.range     = warningRange;
        light.intensity = warningIntensity;
    }

    private void OnAsteroidImpact()
    {
        Debug.Log($"[AsteroidSpawner] Астероид попал в корабль! Урон={impactDamage}");
        shipDamageSystem?.TakeDamage(impactDamage);
        CameraShake.Instance?.Shake(1f);
    }
}
