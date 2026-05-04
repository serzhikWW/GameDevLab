using SpaceRepair.Asteroids;
using SpaceRepair.Mechanics;
using UnityEngine;
using UnityEngine.VFX;

namespace SpaceRepair.Tools
{
    public class WeldingTool : MonoBehaviour
    {
        [Header("Tool Settings")]
        [SerializeField] private float range = 3f;
        [SerializeField] private float heatPerSecond = 25f;
        [SerializeField] private float repelForce = 8f;
        [SerializeField] private float weldProgressPerSecond = 0.3f;
        [SerializeField] private float deformForcePerSecond = 0.5f;

        [Header("References")]
        [SerializeField] private HeatableObject toolHeat;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private VisualEffect weldVFX;
        [SerializeField] private VisualEffect repelVFX;
        [SerializeField] private AudioSource weldAudio;

        [Header("Layers")]
        [SerializeField] private LayerMask targetLayers;

        private bool _isFiring;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
            if (toolHeat == null)
                toolHeat = GetComponent<HeatableObject>();
        }

        private void Update()
        {
            bool primaryFire = Input.GetButton("Fire1");
            bool secondaryFire = Input.GetButton("Fire2");

            if (primaryFire && !toolHeat.IsOverheated)
                UseWeld();
            else if (secondaryFire && !toolHeat.IsOverheated)
                UseRepel();
            else
                StopFiring();
        }

        private void UseWeld()
        {
            SetFiring(true);
            toolHeat.ApplyHeat(heatPerSecond);

            if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                    out RaycastHit hit, range, targetLayers))
                return;

            // Weld ship crack
            if (hit.collider.TryGetComponent<ShipCrack>(out var crack))
            {
                crack.ApplyRepair(weldProgressPerSecond * Time.deltaTime);
                PositionVFX(weldVFX, hit.point);
                return;
            }

            // Deform asteroid
            if (hit.collider.TryGetComponent<DeformableMesh>(out var asteroid))
            {
                asteroid.Deform(hit.point, deformForcePerSecond);
                PositionVFX(weldVFX, hit.point);
            }
        }

        private void UseRepel()
        {
            SetFiring(true);
            toolHeat.ApplyHeat(heatPerSecond * 0.5f);

            if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                    out RaycastHit hit, range, targetLayers))
                return;

            if (hit.rigidbody != null)
            {
                Vector3 repelDirection = hit.normal;
                hit.rigidbody.AddForceAtPosition(repelDirection * repelForce, hit.point, ForceMode.Impulse);
                PositionVFX(repelVFX, hit.point);
            }
        }

        private void StopFiring()
        {
            SetFiring(false);
        }

        private void SetFiring(bool firing)
        {
            if (_isFiring == firing) return;
            _isFiring = firing;

            if (weldVFX != null) { if (firing) weldVFX.Play(); else weldVFX.Stop(); }
            if (weldAudio != null) { if (firing) weldAudio.Play(); else weldAudio.Stop(); }
        }

        private void PositionVFX(VisualEffect vfx, Vector3 worldPos)
        {
            if (vfx == null) return;
            vfx.transform.position = worldPos;
        }
    }
}
