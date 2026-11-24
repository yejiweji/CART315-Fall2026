using System.Collections;
using UnityEngine;

// AudioSource starts playing, or whenever Flicker() is called from another script.
public class LightFlickerOnSound : MonoBehaviour
{
    [Header("What triggers it")]
    [Tooltip("Flicker each time this source starts playing. Leave empty if you call Flicker() yourself.")]
    [SerializeField] private AudioSource source;

    [Header("What flickers")]
    [SerializeField] private Light[] lights;
    [Tooltip("Optional: bulb renderers whose material has Emission ticked.")]
    [SerializeField] private Renderer[] glowRenderers;

    [Header("How it flickers")]
    [SerializeField, Min(0.05f)] private float flickerDuration = 0.4f;
    [Tooltip("Flips between dim and full this many times per second.")]
    [SerializeField, Min(1f)] private float flickerRate = 25f;
    [Tooltip("Brightness while dimmed: 0 = off, 1 = no change.")]
    [SerializeField, Range(0f, 1f)] private float dimLevel = 0.1f;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private float[] _baseIntensity;
    private Color[] _baseEmission;
    private bool _wasPlaying;
    private Coroutine _routine;

    private void Awake()
    {
        _baseIntensity = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            if (lights[i] != null) _baseIntensity[i] = lights[i].intensity;

        _baseEmission = new Color[glowRenderers.Length];
        for (int i = 0; i < glowRenderers.Length; i++)
        {
            if (glowRenderers[i] == null) continue;
            // .material makes a private copy so other objects sharing the material are untouched.
            Material m = glowRenderers[i].material;
            if (m.HasProperty(EmissionColor)) _baseEmission[i] = m.GetColor(EmissionColor);
        }
    }

    private void Update()
    {
        if (source == null) return;
        bool playing = source.isPlaying;
        if (playing && !_wasPlaying) Flicker();
        _wasPlaying = playing;
    }

    // Call this from other scripts, e.g. after you play a sound.
    public void Flicker()
    {
        if (!isActiveAndEnabled) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        float end = Time.time + flickerDuration;
        float interval = 1f / flickerRate;
        bool dim = true;
        while (Time.time < end)
        {
            // Random brightness inside the dim/bright band keeps it from looking mechanical.
            float level = dim ? Random.Range(0f, dimLevel) : Random.Range(0.7f, 1f);
            Apply(level);
            dim = !dim;
            yield return new WaitForSeconds(interval);
        }
        Apply(1f);
        _routine = null;
    }

    private void Apply(float level)
    {
        for (int i = 0; i < lights.Length; i++)
            if (lights[i] != null) lights[i].intensity = _baseIntensity[i] * level;

        for (int i = 0; i < glowRenderers.Length; i++)
        {
            if (glowRenderers[i] == null) continue;
            Material m = glowRenderers[i].material;
            if (m.HasProperty(EmissionColor)) m.SetColor(EmissionColor, _baseEmission[i] * level);
        }
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        if (_baseIntensity != null) Apply(1f);
    }
}
