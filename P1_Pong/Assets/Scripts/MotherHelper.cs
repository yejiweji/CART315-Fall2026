using UnityEngine;
using UnityEngine.InputSystem;

public class MotherHelper : MonoBehaviour
{
    [Header("Tuning (change these in the Inspector)")]
    public int hitsBeforeOffer = 1;
    public float controlDuration = 5f; // seconds the mother takes over

    private enum Stage
    {
        Waiting,        // normal Pong, counting hits
        Asking,         // "Can I help you?" is on screen
        WaitingForHit,  // answered, waiting for one more hit
        Controlling,    // mother has the paddle
        Released        // control given back to the player
    }

    private Stage stage = Stage.Waiting;
    private int playerHits = 0;
    private float controlTimer = 0f;
    private bool playerSaidYes; // saved for later experiments

    private Paddle _paddle;
    private Ball _ball;

    private void Awake()
    {
        _paddle = GetComponent<Paddle>();
        _ball = FindFirstObjectByType<Ball>();
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite != null) _originalColor = _sprite.color;
    }

    private SpriteRenderer _sprite;
    private Color _originalColor;

    // Paddle.cs calls this every time the ball hits the player's paddle
    public void RegisterPlayerHit()
    {
        if (stage == Stage.Waiting)
        {
            playerHits++;
            Debug.Log("Player hits: " + playerHits);

            if (playerHits >= hitsBeforeOffer)
            {
                stage = Stage.Asking;
                Time.timeScale = 0f; // pause while she asks
            }
        }
        else if (stage == Stage.WaitingForHit)
        {
            // One more hit after answering: she takes over
            stage = Stage.Controlling;
            controlTimer = controlDuration;
            Debug.Log("Mother takes control");
            if (_sprite != null) _sprite.color = Color.red;
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
            controlTimer -= Time.deltaTime;
            if (controlTimer <= 0f)
            {
                stage = Stage.Released;
                Debug.Log("Mother releases control");
                if (_sprite != null) _sprite.color = _originalColor;
            }
        }
    }

    // LateUpdate runs after every Update, so it runs after PlayerController
    // has written the player's input. Her movement overwrites it.
    private void LateUpdate()
    {
        if (stage != Stage.Controlling) return;

        float diffY = _ball.transform.position.y - transform.position.y;
        _paddle.direction = new Vector2(0f, diffY*2f); // move faster than the ball to catch it
    }

    // Big speech bubble, centered on screen
    private void OnGUI()
    {
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
        GUI.Label(new Rect(box.x, box.y + h * 0.08f, w, h * 0.45f), "Can I help you?", labelStyle);

        float btnW = w * 0.35f;
        float btnH = h * 0.25f;
        float btnY = box.y + h * 0.65f;

        if (GUI.Button(new Rect(box.x + w * 0.10f, btnY, btnW, btnH), "YES (Y)", buttonStyle)) Answer(true);
        if (GUI.Button(new Rect(box.x + w * 0.55f, btnY, btnW, btnH), "NO (N)", buttonStyle)) Answer(false);
    }

    private void Answer(bool accepted)
    {
        playerSaidYes = accepted;
        stage = Stage.WaitingForHit; // same result for YES and NO, for now
        Time.timeScale = 1f;         // resume
        Debug.Log("Player answered: " + (accepted ? "YES" : "NO"));
    }
}