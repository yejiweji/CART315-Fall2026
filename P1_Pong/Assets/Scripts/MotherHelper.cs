using UnityEngine;
using UnityEngine.InputSystem;

public class MotherHelper : MonoBehaviour
{
    [Header("Offer")]
    public int hitsBeforeOffer = 2;

    [Header("Mother's control (seconds)")]
    public float controlDuration = 2f;     // first takeover
    public float controlGrowth = 1.6f;     // each takeover lasts this many times longer
    public float maxControlDuration = 30f;

    [Header("Player's free time (seconds)")]
    public float freeDuration = 12f;       // free time after the first takeover
    public float freeShrink = 0.75f;       // each free period is this fraction of the last
    public float minFreeDuration = 2f;

    [Header("How hard she follows the ball")]
    public float followStrength = 2f;      // move faster than the ball to catch it
    public float followStrengthPerTakeover = 0.5f;

    [Header("Chaos (0 = predictable, 0.5 = very random)")]
    [Range(0f, 0.9f)] public float chaos = 0.3f;

        [Header("Ending")]
    public int hitsBeforeFullControl = 6;   // player hits (while free) before she takes over for good
    public float fullControlStrength = 4f;

    private enum Stage {
            Waiting,
            Asking,
            WaitingForHit,
            Controlling,
            Free,
            FullControl     // permanent, never released
    }

    private Stage stage = Stage.Waiting;
    private int playerHits = 0;
    private int takeoverCount = 0;
    private bool askedAgain = false;
    private float timer = 0f;
    private bool playerSaidYes; // saved for later experiments
    private Paddle _paddle;
    private Ball _ball;
    private SpriteRenderer _sprite;
    private Color _originalColor;
    private int freeHits = 0;

    private void Awake()
    {
        _paddle = GetComponent<Paddle>();
        _ball = FindFirstObjectByType<Ball>();
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite != null) _originalColor = _sprite.color;
    }

    // Paddle.cs calls this every time the ball hits the player's paddle
    public void RegisterPlayerHit()
    {
        if (stage == Stage.Waiting)
        {
            playerHits++;
            Debug.Log("Player hits: " + playerHits);

            if (playerHits >= hitsBeforeOffer)
                StartAsking();
        }
        else if (stage == Stage.WaitingForHit)
        {
            StartControl();
        }
        else if (stage == Stage.Free)
        {
            freeHits++;
            Debug.Log("Free hits: " + freeHits);
            if (freeHits >= hitsBeforeFullControl) StartFullControl();
        }
    }

    private void Update()
    {
        if (stage == Stage.Asking)
        {
            if (Keyboard.current.yKey.wasPressedThisFrame) Answer(true);
            if (Keyboard.current.nKey.wasPressedThisFrame) Answer(false);
        }
        else if (stage == Stage.Controlling)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) EndControl();
        }
        else if (stage == Stage.Free)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (!askedAgain)
                {
                    askedAgain = true; // she only asks one more time
                    StartAsking();
                }
                else
                {
                    stage = Stage.WaitingForHit; // she just takes over on the next hit
                }
            }
        }
    }

    // LateUpdate runs after every Update, so it runs after PlayerController
    // has written the player's input. Her movement overwrites it.
    private void LateUpdate()
    {
        if (stage != Stage.Controlling && stage != Stage.FullControl) return;

        float strength = (stage == Stage.FullControl)
            ? fullControlStrength
            : followStrength + followStrengthPerTakeover * takeoverCount;

        float diffY = _ball.transform.position.y - transform.position.y;
        _paddle.direction = new Vector2(0f, diffY * strength);
    }
    private void StartAsking()
    {
        stage = Stage.Asking;
        Time.timeScale = 0f; // pause while she asks
    }

    private void Answer(bool accepted)
    {
        playerSaidYes = accepted;
        stage = Stage.WaitingForHit; // same result for YES and NO
        Time.timeScale = 1f;         // resume
        Debug.Log("Player answered: " + (accepted ? "YES" : "NO"));
    }

    private void StartControl()
    {
        stage = Stage.Controlling;
        timer = Randomize(Mathf.Min(controlDuration * Mathf.Pow(controlGrowth, takeoverCount), maxControlDuration));
        if (_sprite != null) _sprite.color = Color.red;
        Debug.Log("Mother takes control for " + timer.ToString("F1") + "s (takeover #" + (takeoverCount + 1) + ")");
    }

        private void StartFullControl()
    {
        stage = Stage.FullControl;
        if (_sprite != null) _sprite.color = Color.red;
        Debug.Log("Mother has full control. She never gives it back.");
    }

    private void EndControl()
    {
        takeoverCount++;
        stage = Stage.Free;
        timer = Randomize(Mathf.Max(freeDuration * Mathf.Pow(freeShrink, takeoverCount - 1), minFreeDuration));
        if (_sprite != null) _sprite.color = _originalColor;
        Debug.Log("Mother releases control. Player free for " + timer.ToString("F1") + "s");
    }

    // Adds randomness so the timing feels unpredictable
    private float Randomize(float value)
    {
        return value * Random.Range(1f - chaos, 1f + chaos);
    }

    // Big speech bubble, centered on screen
    private void OnGUI()
    {
                if (stage == Stage.FullControl)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = Mathf.RoundToInt(Screen.height * 0.06f);
            s.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, Screen.height * 0.1f),
                "I'm only trying to help.", s);
            return;
        }
        
        if (stage != Stage.Asking) return;

        float w = Screen.width * 0.5f;
        float h = Screen.height * 0.4f;
        Rect box = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = Mathf.RoundToInt(h * 0.2f);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.wordWrap = true;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = Mathf.RoundToInt(h * 0.11f);

        GUI.Box(box, "");
        GUI.Label(new Rect(box.x, box.y + h * 0.08f, w, h * 0.45f), "Can Mother help you?", labelStyle);

        float btnW = w * 0.35f;
        float btnH = h * 0.25f;
        float btnY = box.y + h * 0.65f;

        if (GUI.Button(new Rect(box.x + w * 0.10f, btnY, btnW, btnH), "YES (Y)", buttonStyle)) Answer(true);
        if (GUI.Button(new Rect(box.x + w * 0.55f, btnY, btnW, btnH), "NO (N)", buttonStyle)) Answer(false);
    }
}