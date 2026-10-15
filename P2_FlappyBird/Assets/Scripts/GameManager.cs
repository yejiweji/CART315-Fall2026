using UnityEngine;
using UnityEngine.UI;

public enum GameState { Ready, Playing, Dying }

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject playButton;   // now just a "TAP TO START" prompt (optional)
    [SerializeField] private GameObject gameOver;     // brief banner while the bird falls (optional)

    [Header("Zero-friction loop")]
    [Tooltip("Seconds after death before a tap is accepted (prevents accidental restarts).")]
    [SerializeField] private float restartLockout = 0.3f;
    [Tooltip("If the player doesn't tap, the bird resets to the start line after this many seconds. 0 = never.")]
    [SerializeField] private float autoResetDelay = 1.2f;

    public GameState State { get; private set; } = GameState.Ready;
    public bool IsPlaying => State == GameState.Playing;
    public int score { get; private set; } = 0;

    private float deathTime;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        EnterReady();
    }

    private void Update()
    {
        switch (State)
        {
            case GameState.Ready:
                if (TapPressed()) {
                    Play();
                }
                break;

            case GameState.Dying:
                float sinceDeath = Time.time - deathTime;

                if (sinceDeath >= restartLockout && TapPressed()) {
                    Play(); // straight back into a run, no menus
                } else if (autoResetDelay > 0f && sinceDeath >= autoResetDelay) {
                    EnterReady(); // bird waits at the start line, one tap from playing
                }
                break;
        }
    }

    // Single place that defines "the player pressed something"
    public static bool TapPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }

    private void EnterReady()
    {
        State = GameState.Ready;
        Time.timeScale = 1f;

        ClearPipes();
        score = 0;
        scoreText.text = score.ToString();

        spawner.enabled = false;
        player.ResetBird();

        if (playButton != null) playButton.SetActive(true);
        if (gameOver != null) gameOver.SetActive(false);
    }

    // Safe to call from a UI button too: ignored if a run is already active
    public void Play()
    {
        if (State == GameState.Playing) return;

        ClearPipes();
        score = 0;
        scoreText.text = score.ToString();

        if (playButton != null) playButton.SetActive(false);
        if (gameOver != null) gameOver.SetActive(false);

        player.ResetBird();
        State = GameState.Playing;

        spawner.enabled = true; // OnEnable restarts the spawn timer
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;

        State = GameState.Dying;
        deathTime = Time.time;

        spawner.enabled = false;
        if (gameOver != null) gameOver.SetActive(true);

        player.Die();
    }

    public void IncreaseScore()
    {
        if (State != GameState.Playing) return;

        score++;
        scoreText.text = score.ToString();
    }

    private void ClearPipes()
    {
        Pipes[] pipes = FindObjectsByType<Pipes>(FindObjectsSortMode.None);

        for (int i = 0; i < pipes.Length; i++) {
            Destroy(pipes[i].gameObject);
        }
    }

}