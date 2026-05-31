using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("Popup Panel")]
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;

    [Header("Arrow")]
    [SerializeField] private Image arrowImage;

    private void Start()
    {
        gameObject.SetActive(false);

        TutorialController.Instance.OnStepActivated += ShowStep;
        TutorialController.Instance.OnTutorialComplete += () => gameObject.SetActive(false);

        nextButton.onClick.AddListener(OnNextClicked);
    }

    private void ShowStep(TutorialStep step)
    {
        gameObject.SetActive(true);

        titleText.text = step.title;
        descText.text = step.description;

        if (step.anchor == PopupAnchor.Center)
        {
            popupRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            Vector2 pos = TutorialController.Instance.GetPopupPosition(step, popupRect);

            float halfW = popupRect.rect.width * 0.5f;
            float halfH = popupRect.rect.height * 0.5f;

            pos.x = Mathf.Clamp(pos.x, halfW, Screen.width - halfW);
            pos.y = Mathf.Clamp(pos.y, halfH, Screen.height - halfH);

            popupRect.position = new Vector3(pos.x, pos.y, 0f);
        }

        bool hasTarget = step.anchor != PopupAnchor.Center && step.target != null;
        arrowImage.gameObject.SetActive(hasTarget);

        if (hasTarget)
        {
            Vector3[] corners = new Vector3[4];
            step.target.GetWorldCorners(corners);
            Vector2 targetCenter = ((Vector2)corners[0] + (Vector2)corners[2]) * 0.5f;

            Vector2 dir = (targetCenter - (Vector2)popupRect.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrowImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

            float halfW = popupRect.rect.width * 0.5f;
            float halfH = popupRect.rect.height * 0.5f;

            float tx = Mathf.Abs(dir.x) > 0.001f ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
            float ty = Mathf.Abs(dir.y) > 0.001f ? halfH / Mathf.Abs(dir.y) : float.MaxValue;

            arrowImage.rectTransform.anchoredPosition = dir * Mathf.Min(tx, ty);
        }
    }

    private void OnNextClicked()
    {
        UISound.PlayClick();
        TutorialController.Instance.Next();
    }

    private void OnDestroy()
    {
        if (TutorialController.Instance != null)
        {
            TutorialController.Instance.OnStepActivated -= ShowStep;
            TutorialController.Instance.OnTutorialComplete -= () => gameObject.SetActive(false);
        }
    }
}