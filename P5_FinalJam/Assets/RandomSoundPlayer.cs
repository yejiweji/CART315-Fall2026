using System.Collections;
using UnityEngine;

// Plays this object's AudioSource at random intervals.
// Pair it with LightFlickerOnSound: the flicker script sees each start and reacts.
[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour
{
    [Tooltip("Shortest wait between sounds, in seconds.")]
    [SerializeField, Min(0f)] private float minDelay = 15f;
    [Tooltip("Longest wait between sounds, in seconds.")]
    [SerializeField, Min(0f)] private float maxDelay = 40f;
    [Tooltip("Wait this long after the game starts before the first sound.")]
    [SerializeField, Min(0f)] private float firstDelay = 8f;
    [Tooltip("Slightly randomise pitch so repeats don't sound identical (0 = off).")]
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.05f;

    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;   // this script decides when to play
        _source.loop = false;          // looping would only start once
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(firstDelay);
        while (true)
        {
            _source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            _source.Play();

            // Wait for the sound to finish, then for a random gap.
            while (_source.isPlaying) yield return null;
            yield return new WaitForSeconds(Random.Range(minDelay, Mathf.Max(minDelay, maxDelay)));
        }
    }
}
