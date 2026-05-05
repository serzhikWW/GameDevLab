using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ShipDamageSystem shipDamageSystem;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject gameOverScreen;

    private bool _gameOver;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (shipDamageSystem != null)
        {
            shipDamageSystem.onShipDestroyed.AddListener(OnGameOver);
            shipDamageSystem.onShipFullyRepaired.AddListener(OnVictory);
        }

        SetScreen(victoryScreen, false);
        SetScreen(gameOverScreen, false);
    }

    private void OnVictory()
    {
        if (_gameOver) return;
        _gameOver = true;
        SetScreen(victoryScreen, true);
        EndGame();
    }

    private void OnGameOver()
    {
        if (_gameOver) return;
        _gameOver = true;
        SetScreen(gameOverScreen, true);
        EndGame();
    }

    private static void EndGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log($"[GameManager] EndGame: timeScale={Time.timeScale}, cursorLock={Cursor.lockState}, cursorVisible={Cursor.visible}");
    }

    private void Update()
    {
        // Сторожим состояние курсора во время финального экрана —
        // если кто-то перебивает блокировку, увидим в логе
        if (_gameOver && Cursor.lockState != CursorLockMode.None)
        {
            Debug.LogWarning($"[GameManager] Курсор кем-то залочен после Game Over! Принудительно разблокирую.");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] RestartGame() вызван — перезапуск сцены");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void SetScreen(GameObject screen, bool active)
    {
        if (screen != null) screen.SetActive(active);
    }
}
