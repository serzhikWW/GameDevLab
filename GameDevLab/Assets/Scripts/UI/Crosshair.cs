using UnityEngine;
using UnityEngine.UI;

// Повесь на любой GameObject — создаст Canvas с прицелом сам
public class Crosshair : MonoBehaviour
{
    [SerializeField] private float size = 12f;
    [SerializeField] private Color color = Color.white;

    private void Awake()
    {
        // Canvas
        var canvasGO = new GameObject("CrosshairCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Точка прицела
        var dotGO = new GameObject("Dot");
        dotGO.transform.SetParent(canvas.transform, false);

        var img = dotGO.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        var rect = dotGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = Vector2.zero;
    }
}
