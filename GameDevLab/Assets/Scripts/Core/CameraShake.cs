using UnityEngine;
using Unity.Cinemachine;

// Повесь на любой объект в сцене вместе с CinemachineImpulseSource.
// AsteroidSpawner вызывает CameraShake.Instance.Shake() при ударе.
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        Instance = this;
        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float force = 1f)
    {
        impulseSource?.GenerateImpulse(force);
    }
}
