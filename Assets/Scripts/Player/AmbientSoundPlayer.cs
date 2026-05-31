using System.Collections;
using UnityEngine;

// Randomly plays one of up to 3 ambient sound clips at a random interval.
[RequireComponent(typeof(AudioSource))]
public class AmbientSoundPlayer : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Interval")]
    [SerializeField] private float minInterval = 45f;
    [SerializeField] private float maxInterval = 75f;

    [Header("Volume")]
    [SerializeField][Range(0f, 1f)] private float volume = 1f;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            if (clips == null || clips.Length == 0) continue;

            // Pick a random clip
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
                _audioSource.PlayOneShot(clip, volume * SettingsMenu.SFXVolume);
        }
    }
}