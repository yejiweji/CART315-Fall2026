using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Pipes prefab;
    public float spawnRate = 1f;
    public float minHeight = -1f;
    public float maxHeight = 2f;
    public float verticalGap = 3f;

    [Header("Near-miss director")]
    [Tooltip("Pipes the player still has to pass before a newly spawned pipe reaches them. Tune to your spawn distance.")]
    public int spawnLookahead = 2;
    public float squeezeGapMultiplier = 0.85f;
    public float easeGapMultiplier = 1.2f;
    [Tooltip("Floor for a squeezed gap so the run stays beatable.")]
    public float minGap = 2.4f;
    [Tooltip("Ceiling for an eased gap.")]
    public float maxGap = 3.6f;

    private float lastHeight;

    private void OnEnable()
    {
        lastHeight = (minHeight + maxHeight) / 2f;
        InvokeRepeating(nameof(Spawn), spawnRate, spawnRate);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    private void Spawn()
    {
        Pipes pipes = Instantiate(prefab, transform.position, Quaternion.identity);

        float height = Random.Range(minHeight, maxHeight);
        float gap = verticalGap;

        GameManager gm = GameManager.Instance;

        if (gm != null) {
            switch (gm.GetDirectorMode(spawnLookahead))
            {
                case DirectorMode.Squeeze:
                    // Tighter gap, and put it at the opposite extreme from the last pipe
                    // so the player has to make a big vertical move
                    gap = Mathf.Max(gap * squeezeGapMultiplier, minGap);
                    float mid = (minHeight + maxHeight) / 2f;
                    height = lastHeight >= mid
                        ? Random.Range(minHeight, minHeight + 0.4f)
                        : Random.Range(maxHeight - 0.4f, maxHeight);
                    break;

                case DirectorMode.Ease:
                    gap = Mathf.Min(gap * easeGapMultiplier, maxGap);
                    break;
            }
        }

        lastHeight = height;

        pipes.transform.position += Vector3.up * height;
        pipes.gap = gap;
    }

}