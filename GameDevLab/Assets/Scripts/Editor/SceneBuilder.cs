#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SceneBuilder
{
    [MenuItem("SpaceRepair/Build Scene")]
    public static void BuildScene()
    {
        // Directional Light
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // Ship
        var ship = new GameObject("Ship");
        var dmg = ship.AddComponent<ShipDamageSystem>();

        var shipBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shipBody.name = "ShipBody";
        shipBody.transform.SetParent(ship.transform);
        shipBody.transform.localScale = new Vector3(4f, 1f, 6f);
        shipBody.transform.localPosition = Vector3.zero;
        SetLayer(shipBody, "Ship");

        var crack01 = new GameObject("Crack_01");
        crack01.transform.SetParent(ship.transform);
        crack01.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        var crackBox = crack01.AddComponent<BoxCollider>();
        crackBox.size = new Vector3(1f, 0.2f, 1f);
        var crackComp = crack01.AddComponent<ShipCrack>();
        SetLayer(crack01, "Ship");

        // Wire cracks array
        var so = new SerializedObject(dmg);
        var cracksArr = so.FindProperty("cracks");
        cracksArr.arraySize = 1;
        cracksArr.GetArrayElementAtIndex(0).objectReferenceValue = crackComp;
        so.ApplyModifiedProperties();

        // Player
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 2f, -10f);
        player.AddComponent<HeatableObject>();
        player.AddComponent<WeldingTool>();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(player.transform);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }

        // GameManager
        var gm = new GameObject("GameManager");
        var gmComp = gm.AddComponent<GameManager>();
        var gmSo = new SerializedObject(gmComp);
        gmSo.FindProperty("shipDamageSystem").objectReferenceValue = dmg;
        gmSo.ApplyModifiedProperties();

        // AsteroidSpawner
        var spawnerGO = new GameObject("AsteroidSpawner");
        var spawnerComp = spawnerGO.AddComponent<AsteroidSpawner>();
        var spawnerSo = new SerializedObject(spawnerComp);
        spawnerSo.FindProperty("shipTarget").objectReferenceValue = ship.transform;
        spawnerSo.FindProperty("shipDamageSystem").objectReferenceValue = dmg;
        spawnerSo.FindProperty("spawnRadius").floatValue = 25f;
        spawnerSo.FindProperty("spawnInterval").floatValue = 4f;
        spawnerSo.FindProperty("asteroidSpeed").floatValue = 4f;
        spawnerSo.FindProperty("maxAsteroids").intValue = 5;
        spawnerSo.FindProperty("impactDamage").floatValue = 20f;
        spawnerSo.ApplyModifiedProperties();

        Debug.Log("[SceneBuilder] Готово! Назначь префаб астероида в AsteroidSpawner вручную.");
        Selection.activeGameObject = spawnerGO;
    }

    private static void SetLayer(GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
            Debug.LogWarning($"[SceneBuilder] Слой '{layerName}' не найден. Создай его в Tags and Layers.");
        else
            go.layer = layer;
    }
}
#endif
