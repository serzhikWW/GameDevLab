using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class WeldingTool : MonoBehaviour
{
    [Header("Tool Settings")]
    [SerializeField] private float range = 8f;
    [SerializeField] private float heatPerSecond = 20f;
    [SerializeField] private float repelForce = 15f;
    [SerializeField] private float weldProgressPerSecond = 1.5f;
    [SerializeField] private float deformForcePerSecond = 0.5f;
    [SerializeField] private float crackSearchRadius = 1.5f;

    [Header("References")]
    [SerializeField] private HeatableObject toolHeat;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource weldAudio;

    [Header("VFX")]
    [SerializeField] private VisualEffect sparkVFX;   // VFX Graph — искры сварки

    private bool _isFiring;
    private int _hitMask;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (toolHeat == null)    toolHeat = GetComponent<HeatableObject>();
        _hitMask = ~(1 << 2);

        // Искры выключены до начала сварки
        if (sparkVFX != null) sparkVFX.gameObject.SetActive(false);
    }

    private void Update()
    {
        // На паузе (GameOver/Victory) инструмент не работает
        if (Time.timeScale <= 0f) { StopFiring(); return; }

        bool lmb = Mouse.current.leftButton.isPressed;
        bool rmb = Mouse.current.rightButton.isPressed;

        if      (lmb && !toolHeat.IsOverheated) UseWeld();
        else if (rmb && !toolHeat.IsOverheated) UseRepel();
        else                                    StopFiring();
    }

    private void UseWeld()
    {
        SetFiring(true);
        toolHeat.ApplyHeat(heatPerSecond);

        if (!CenterRaycast(out RaycastHit hit))
        {
            SetSparks(false, Vector3.zero, Vector3.up);
            return;
        }

        // Искры в точке попадания, направлены по нормали поверхности
        SetSparks(true, hit.point, hit.normal);

        if (hit.collider.TryGetComponent<DeformableMesh>(out var dm))
        {
            dm.Deform(hit.point, deformForcePerSecond);
            return;
        }

        var crack = hit.collider.GetComponent<ShipCrack>()
                    ?? hit.collider.GetComponentInParent<ShipCrack>()
                    ?? FindNearestCrack(hit.point);

        if (crack != null)
            crack.ApplyRepair(weldProgressPerSecond * Time.deltaTime);
    }

    private void UseRepel()
    {
        SetFiring(true);
        toolHeat.ApplyHeat(heatPerSecond * 0.5f);

        if (!CenterRaycast(out RaycastHit hit)) return;

        var mover = hit.collider.GetComponent<AsteroidMover>()
                    ?? hit.collider.GetComponentInParent<AsteroidMover>();

        if (mover != null) { mover.Repel(-hit.normal, repelForce); return; }

        if (hit.rigidbody != null)
            hit.rigidbody.AddForceAtPosition(hit.normal * repelForce, hit.point, ForceMode.Impulse);
    }

    // ── VFX ──────────────────────────────────────────────────────────────────

    private void SetSparks(bool active, Vector3 position, Vector3 normal)
    {
        if (sparkVFX == null) return;

        if (active)
        {
            sparkVFX.transform.position = position;
            // Разворачиваем VFX по нормали поверхности — искры летят "из стены"
            sparkVFX.transform.rotation = Quaternion.LookRotation(normal);

            if (!sparkVFX.gameObject.activeSelf)
                sparkVFX.gameObject.SetActive(true);
        }
        else
        {
            if (sparkVFX.gameObject.activeSelf)
                sparkVFX.gameObject.SetActive(false);
        }
    }

    // ── Утилиты ──────────────────────────────────────────────────────────────

    private bool CenterRaycast(out RaycastHit hit)
        => Physics.Raycast(
            playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)),
            out hit, range, _hitMask);

    private ShipCrack FindNearestCrack(Vector3 point)
    {
        ShipCrack nearest = null;
        float minDist = float.MaxValue;
        foreach (var col in Physics.OverlapSphere(point, crackSearchRadius))
        {
            var c = col.GetComponent<ShipCrack>();
            if (c == null || c.IsSealed) continue;
            float d = Vector3.Distance(point, col.transform.position);
            if (d < minDist) { minDist = d; nearest = c; }
        }
        return nearest;
    }

    private void StopFiring()
    {
        SetFiring(false);
        SetSparks(false, Vector3.zero, Vector3.up);
    }

    private void SetFiring(bool firing)
    {
        if (_isFiring == firing) return;
        _isFiring = firing;
        if (weldAudio != null) { if (firing) weldAudio.Play(); else weldAudio.Stop(); }
    }
}
