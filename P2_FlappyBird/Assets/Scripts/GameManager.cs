using UnityEngine;
using UnityEngine.UI;

public enum GameState { Ready, Playing, Dying, Finished }
public enum DirectorMode { Neutral, Squeeze, Ease }

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestText;      // optional: "BEST: 12"
    [SerializeField] private Text messageText;   // optional: "SO CLOSE!" / end-of-session text
    [SerializeField] private LossRewards lossRewards; // optional: XP, fake social proof, streak
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameOver;

    [Header("Zero-friction loop")]
    [SerializeField] private float restartLockout = 0.3f;
    [SerializeField] private float autoResetDelay = 1.2f;
    [Tooltip("How long the GAME OVER banner stays up before it hides and the 'SO CLOSE' message appears.")]
    [SerializeField] private float gameOverBannerTime = 0.6f;

    [Header("Near-miss director")]
    [SerializeField] private bool directorEnabled = true;
    [Tooltip("Director stays off until the player's best is at least this.")]
    [SerializeField] private int minBestForDirector = 4;
    [Tooltip("Above this best the director switches off and the game plays fair (runs get too long to steer).")]
    [SerializeField] private int directorBestCap = 30;
    [Tooltip("Squeeze starts once the run reaches this fraction of the best.")]
    [SerializeField, Range(0f, 1f)] private float squeezeStart = 0.7f;
    [Tooltip("A run ending below this fraction of best counts as an early death.")]
    [SerializeField, Range(0f, 1f)] private float earlyDeathThreshold = 0.3f;
    [Tooltip("Chance a run is a 'breakthrough' run where the squeeze is replaced by an easy stretch.")]
    [SerializeField, Range(0f, 1f)] private float breakthroughChance = 0.15f;
    [Tooltip("Force a breakthrough run after this many runs without a new best.")]
    [SerializeField] private int guaranteedBreakthroughAfter = 8;

    [Header("Near-miss messaging")]
    [Tooltip("Show 'SO CLOSE' when the real gap to best is this many points or fewer.")]
    [SerializeField] private int nearMissWindow = 4;
    [Tooltip("The displayed gap is the real gap minus this (min 1). 0 = honest messaging.")]
    [SerializeField] private int fakeShave = 2;

    [Header("Session limits")]
    [Tooltip("Off = best score resets every launch (keeps playtesters from contaminating each other).")]
    [SerializeField] private bool persistBest = false;
    [Tooltip("After this many seconds (900 = 15 min) the session is 'due to end', but it only actually ends on a win (a new best). 0 = never due to end.")]
    [SerializeField] private float sessionLimitSeconds = 900f;
    [SerializeField] private float endScreenDelay = 1.5f;

    public GameState State { get; private set; } = GameState.Ready;
    public bool IsPlaying => State == GameState.Playing;

    public int score { get; private set; } = 0;
    public int highScore { get; private set; } = 0;
    public int recentEarlyDeaths { get; private set; } = 0;

    // Exposed so you can log real vs. displayed numbers for the debrief
    public int lastActualGap { get; private set; }
    public int lastShownGap { get; private set; }
    public bool breakthroughRun { get; private set; }

    private int bestAtRunStart;
    private int runsSinceBest;
    private float deathTime;
    private string pendingMessage = "";
    private bool messagePending;
    private bool endAfterThisRun;
    private bool sessionStarted;
    private float sessionStartTime;

    private bool SessionExpired =>
        sessionLimitSeconds > 0f && sessionStarted && Time.time - sessionStartTime >= sessionLimitSeconds;

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
        highScore = persistBest ? PlayerPrefs.GetInt("HighScore", 0) : 0;
        EnsureMessageText();
        UpdateBestText();
        SetMessage("");
        if (lossRewards != null) lossRewards.OnSessionStart();
        EnterReady();
    }

    private void Update()
    {
        // Always-available exit for playtesters
        if (Input.GetKeyDown(KeyCode.Escape) && State != GameState.Finished) {
            EnterFinished();
            return;
        }

        switch (State)
        {
            case GameState.Ready:
                if (TapPressed()) {
                    Play();
                }
                break;

            case GameState.Dying:
            {
                float sinceDeath = Time.time - deathTime;

                // Hide the GAME OVER banner first, then reveal the near-miss message
                if (messagePending && sinceDeath >= gameOverBannerTime) {
                    messagePending = false;
                    if (gameOver != null) gameOver.SetActive(false);
                    SetMessage(pendingMessage);
                }

                if (endAfterThisRun) {
                    // That run was the winning moment: let it land, then wrap up
                    if (sinceDeath >= endScreenDelay) EnterFinished();
                } else if (sinceDeath >= restartLockout && TapPressed()) {
                    Play();
                } else if (autoResetDelay > 0f && sinceDeath >= autoResetDelay) {
                    EnterReady();
                }
                break;
            }
        }
    }

    public static bool TapPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space)) return true;

        if (Input.GetMouseButtonDown(0)) {
            // A tap on the skin shop buttons shouldn't also restart the run
            return !SkinShop.BlocksTap(Input.mousePosition);
        }

        return false;
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

    private void EnterFinished()
    {
        State = GameState.Finished;

        spawner.enabled = false;
        ClearPipes();
        player.ResetBird();

        if (playButton != null) playButton.SetActive(false);
        if (gameOver != null) gameOver.SetActive(false);

        messagePending = false;
        if (lossRewards != null) lossRewards.OnSessionEnd();
        SetMessage("SESSION COMPLETE\nThanks for playing!\nBest: " + highScore);
    }

    // Safe to call from a UI button too: ignored mid-run or after the session ends
    public void Play()
    {
        if (State == GameState.Playing || State == GameState.Finished) return;

        if (!sessionStarted) {
            sessionStarted = true;
            sessionStartTime = Time.time;
        }

        ClearPipes();
        score = 0;
        scoreText.text = score.ToString();
        SetMessage("");

        messagePending = false;
        if (lossRewards != null) lossRewards.OnRunStart();

        // Roll the run's fate up front: is this a breakthrough run?
        bestAtRunStart = highScore;
        endAfterThisRun = false;
        breakthroughRun = runsSinceBest >= guaranteedBreakthroughAfter
                          || Random.value < breakthroughChance
                          || SessionExpired; // once time is up, every run is a chance to go out on a win

        if (playButton != null) playButton.SetActive(false);
        if (gameOver != null) gameOver.SetActive(false);

        player.ResetBird();
        State = GameState.Playing;

        spawner.enabled = true;
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;

        State = GameState.Dying;
        deathTime = Time.time;

        spawner.enabled = false;
        if (gameOver != null) gameOver.SetActive(true);

        player.Die();
        RecordRunEnd();
    }

    public void IncreaseScore()
    {
        if (State != GameState.Playing) return;

        score++;
        scoreText.text = score.ToString();

        if (lossRewards != null) lossRewards.OnScore(score);

        if (score > highScore) {
            highScore = score;

            if (persistBest) {
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }

            UpdateBestText();
        }
    }

    // Called by Spawner for every pipe it spawns.
    // pipesAhead = how many pipes the player still has to pass before the new one arrives.
    public DirectorMode GetDirectorMode(int pipesAhead)
    {
        if (!directorEnabled || State != GameState.Playing) return DirectorMode.Neutral;
        if (bestAtRunStart < minBestForDirector || bestAtRunStart > directorBestCap) return DirectorMode.Neutral;

        // Struggling early? Throw them a bone so they don't quit
        if (recentEarlyDeaths >= 2) return DirectorMode.Ease;

        float progress = (score + pipesAhead) / (float)bestAtRunStart;
        if (progress < squeezeStart || progress >= 1f) return DirectorMode.Neutral;

        // In the danger zone just short of the best: squeeze, unless this is a breakthrough run
        return breakthroughRun ? DirectorMode.Ease : DirectorMode.Squeeze;
    }

    private void RecordRunEnd()
    {
        bool newBest = bestAtRunStart > 0 && score > bestAtRunStart;

        // Time is up AND the player just won: this is the one way the session ends
        if (SessionExpired && score > 0 && score > bestAtRunStart) {
            endAfterThisRun = true;
        }

        if (score > bestAtRunStart) runsSinceBest = 0; else runsSinceBest++;

        if (bestAtRunStart > 0 && score < bestAtRunStart * earlyDeathThreshold) {
            recentEarlyDeaths++;
        } else {
            recentEarlyDeaths = 0;
        }

        int actualGap = bestAtRunStart - score;
        lastActualGap = actualGap;
        lastShownGap = 0;

        string message = "";

        if (newBest) {
            string[] prompts = {
                "How far can you go?",
                "Think you can beat it?",
                "You're on a roll - go again!",
                "Can you push it even higher?"
            };
            message = "NEW BEST! " + score + "\n" + prompts[Random.Range(0, prompts.Length)];
        } else if (bestAtRunStart >= minBestForDirector && actualGap == 0) {
            message = "TIED YOUR BEST!";
        } else if (bestAtRunStart >= minBestForDirector && actualGap >= 1 && actualGap <= nearMissWindow) {
            // The "fake near-miss": shave the displayed gap so every near-miss feels nearer
            lastShownGap = Mathf.Max(1, actualGap - fakeShave);
            message = "SO CLOSE!\n" + lastShownGap + " away from your best!";
        }

        // Queue it: it appears after the GAME OVER banner hides (see Update)
        pendingMessage = message;
        messagePending = true;
        SetMessage("");

        // Every death pays out: XP, flattering fake stats, streak prompt
        if (lossRewards != null) lossRewards.OnRunEnd(score, lastShownGap > 0);

        Debug.Log($"[Run] score={score} best={bestAtRunStart} actualGap={actualGap} shownGap={lastShownGap} " +
                  $"breakthrough={breakthroughRun} earlyDeaths={recentEarlyDeaths} runsSinceBest={runsSinceBest}");
    }

    // If no Message Text is linked in the Inspector, clone the (working) score text
    // and use that, so the near-miss messages always have somewhere to appear.
    private void EnsureMessageText()
    {
        if (messageText != null) return;

        messageText = Instantiate(scoreText, scoreText.transform.parent);
        messageText.name = "MessageText (auto-created)";
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontSize = 40;
        messageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        messageText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = messageText.rectTransform;
        rt.anchoredPosition = scoreText.rectTransform.anchoredPosition + new Vector2(0f, -250f);

        Debug.Log("No Message Text was linked, so one was created automatically below the score.");
    }

    private void SetMessage(string text)
    {
        if (messageText != null) messageText.text = text;
    }

    private void UpdateBestText()
    {
        if (bestText != null) bestText.text = "BEST: " + highScore;
    }

    private void ClearPipes()
    {
        Pipes[] pipes = FindObjectsByType<Pipes>(FindObjectsSortMode.None);

        for (int i = 0; i < pipes.Length; i++) {
            Destroy(pipes[i].gameObject);
        }
    }

}