using System.Collections;
using UnityEngine;

// Attached to the LightningBoltPrefab parent object.
// Plays a warning sound on spawn, then a strike sound when the bolt fires.
[RequireComponent(typeof(AudioSource))]
public class LightningBoltTimer : MonoBehaviour
{
    [SerializeField] private float warningDelay;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField][Range(0f, 1f)] private float warningSoundVolume = 0.6f;
    [SerializeField] private AudioClip strikeSound;
    [SerializeField][Range(0f, 1f)] private float strikeSoundVolume = 1f;

    private Animator lightningAnimator;
    private AudioSource _audioSource;

    void Start()
    {
        lightningAnimator = GetComponentInChildren<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (warningSound != null)
            _audioSource.PlayOneShot(warningSound,
                warningSoundVolume * SettingsMenu.SFXVolume);

        StartCoroutine(DelayedStrike());
    }

    private IEnumerator DelayedStrike()
    {
        yield return new WaitForSeconds(warningDelay);

        if (strikeSound != null)
            _audioSource.PlayOneShot(strikeSound,
                strikeSoundVolume * SettingsMenu.SFXVolume);

        lightningAnimator.SetTrigger("fireBolt");
    }
}