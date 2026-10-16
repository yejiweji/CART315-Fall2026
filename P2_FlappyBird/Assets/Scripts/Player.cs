using UnityEngine;

public class Player : MonoBehaviour
{
    public Sprite[] sprites;
    public float strength = 5f;
    public float gravity = -9.81f;
    public float tilt = 5f;

    [Tooltip("Small upward pop when the bird dies, before it falls.")]
    public float deathHop = 3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;
    private Vector3 direction;
    private int spriteIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);
    }

    // Called by GameManager when entering Ready or starting a run
    public void ResetBird()
    {
        Vector3 position = transform.position;
        position.y = 0f;
        transform.position = position;
        transform.rotation = Quaternion.identity;

        direction = Vector3.zero;

        if (bodyCollider != null) bodyCollider.enabled = true;
    }

    // Called by GameManager on collision: bird hops, then falls
    public void Die()
    {
        if (bodyCollider != null) bodyCollider.enabled = false; // no more scoring/obstacle triggers
        direction = Vector3.up * deathHop;
    }

    private void Update()
    {
        GameManager gm = GameManager.Instance;

        // Hover at the start line until the first tap
        if (gm == null || gm.State == GameState.Ready || gm.State == GameState.Finished) return;

        // Input only counts while the run is live
        if (gm.State == GameState.Playing && GameManager.TapPressed()) {
            direction = Vector3.up * strength;
        }

        // Gravity applies while playing AND while dying, so death is a fall, not a freeze
        direction.y += gravity * Time.deltaTime;
        transform.position += direction * Time.deltaTime;

        // Tilt the bird based on the direction
        Vector3 rotation = transform.eulerAngles;
        rotation.z = Mathf.Clamp(direction.y * tilt, -90f, 45f);
        transform.eulerAngles = rotation;
    }

    private void AnimateSprite()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.State != GameState.Playing) return; // wings stop when not flying

        spriteIndex++;

        if (spriteIndex >= sprites.Length) {
            spriteIndex = 0;
        }

        if (spriteIndex < sprites.Length && spriteIndex >= 0) {
            spriteRenderer.sprite = sprites[spriteIndex];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;

        if (other.gameObject.CompareTag("Obstacle")) {
            gm.GameOver();
        } else if (other.gameObject.CompareTag("Scoring")) {
            gm.IncreaseScore();
        }
    }

}