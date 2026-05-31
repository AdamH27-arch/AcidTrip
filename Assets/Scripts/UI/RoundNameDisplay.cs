using System.Collections;
using UnityEngine;
using TMPro;

public class RoundNameDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text roundNameText;
    [SerializeField] private Animator animator;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Match these to your animation clip lengths")]
    [SerializeField] private float showDuration = 1.5f;
    [SerializeField] private float dissolveDuration = 1f;

    private WaveSpawner _waveSpawner;

    private void Start()
    {
        _waveSpawner = FindAnyObjectByType<WaveSpawner>();

        if (_waveSpawner != null)
        {
            _waveSpawner.OnRoundStarted.AddListener(OnRoundStarted);
            _waveSpawner.OnRoundCompleted.AddListener(OnRoundCompleted);
        }

        animator.SetTrigger("gone");
    }

    private void OnRoundStarted(int round)
    {
        roundNameText.text = _waveSpawner.CurrentRoundName;
        canvasGroup.alpha = 0f;
        StartCoroutine(ShowRoutine());
    }

    private void OnRoundCompleted(int round)
    {
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        // Wait for welcome panel and tutorial tips to finish
        yield return new WaitUntil(() =>
            TutorialController.Instance == null ||
            TutorialController.Instance.TourDone);

        animator.SetTrigger("Show");
        yield return new WaitForSeconds(showDuration);
        animator.SetTrigger("anchored");
    }

    private IEnumerator DissolveRoutine()
    {
        // Play dissolve animation -- text fades out
        animator.SetTrigger("dissolve");
        yield return new WaitForSeconds(dissolveDuration);

        // Dissolve finished -- go fully invisible ready for next round
        animator.SetTrigger("gone");
    }

    private void OnDestroy()
    {
        if (_waveSpawner != null)
        {
            _waveSpawner.OnRoundStarted.RemoveListener(OnRoundStarted);
            _waveSpawner.OnRoundCompleted.RemoveListener(OnRoundCompleted);
        }
    }
}