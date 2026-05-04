using SpaceRepair.Core;
using UnityEngine;

namespace SpaceRepair.Asteroids
{
    public class AsteroidSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] asteroidPrefabs;
        [SerializeField] private Transform shipTarget;
        [SerializeField] private float spawnRadius = 30f;
        [SerializeField] private float spawnInterval = 4f;
        [SerializeField] private float asteroidSpeed = 3f;
        [SerializeField] private int maxAsteroids = 6;

        [Header("Damage")]
        [SerializeField] private ShipDamageSystem shipDamageSystem;
        [SerializeField] private float impactDamage = 15f;

        private float _timer;
        private int _activeCount;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= spawnInterval && _activeCount < maxAsteroids)
            {
                _timer = 0f;
                SpawnAsteroid();
            }
        }

        private void SpawnAsteroid()
        {
            if (asteroidPrefabs == null || asteroidPrefabs.Length == 0) return;

            Vector3 spawnPos = Random.onUnitSphere * spawnRadius + shipTarget.position;
            var prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            var asteroid = Instantiate(prefab, spawnPos, Random.rotation);

            var mover = asteroid.AddComponent<AsteroidMover>();
            mover.Init(shipTarget, asteroidSpeed, OnAsteroidImpact, OnAsteroidDestroyed);
            _activeCount++;
        }

        private void OnAsteroidImpact()
        {
            _activeCount--;
            shipDamageSystem?.TakeDamage(impactDamage);
        }

        private void OnAsteroidDestroyed()
        {
            _activeCount--;
        }
    }
}
