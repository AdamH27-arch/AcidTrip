using System.Collections.Generic;
using UnityEngine;

public enum PopupAnchor { Above, Below, Left, Right, Center }

[System.Serializable]
public class TutorialStep
{
    public string id;
    [TextArea(1, 3)]
    public string title;
    [TextArea(2, 6)]
    public string description;
    public RectTransform target;
    public PopupAnchor anchor;
    public bool pauseGame = true;
}

// Drives a step-by-step guided tutorial.
// Fill the Steps list in the Inspector, subscribe to OnStepActivated in your UI,
// call Next() from your button, and TriggerStep(id) for event-driven steps.
public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    [SerializeField] public GameObject welcomePanel;

    [Header("Steps")]
    [SerializeField] private List<TutorialStep> _steps = new List<TutorialStep>();

    [Header("How many steps are in the initial tour before external triggers fire")]
    [SerializeField] private int _initialTourCount = 4;

    [Header("Gap between popup and target in pixels")]
    [SerializeField] private float _popupOffset = 24f;

    // Persists through scene reloads -- true after the player has seen the tutorial once
    private static bool _hasPlayedBefore = false;


    public System.Action<TutorialStep> OnStepActivated;
    public System.Action OnTutorialComplete;

    private int _index = -1;
    private bool _tourDone;
    private static readonly HashSet<string> _triggered = new HashSet<string>();

    public bool TourDone => _tourDone;
    private void Awake()
    {
        Instance = this;

        if (_hasPlayedBefore)
        {
            // Player has already seen the tutorial -- skip straight to gameplay
            _tourDone = true;
            Time.timeScale = 1f;

            if (welcomePanel != null)
                welcomePanel.SetActive(false);
        }
        else
        {
            // First time playing -- pause and show the welcome panel
            Time.timeScale = 0f;
        }
    }


    public void BeginTour()
    {
        _hasPlayedBefore = true;

   
        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        if (_steps.Count > 0)
            Activate(0);
    }


    public void Next()
    {
        int next = _index + 1;

        if (next < _steps.Count && next < _initialTourCount)
        {
            Activate(next);
        }
        else
        {
            _tourDone = true;
            Time.timeScale = 1f;
            OnTutorialComplete?.Invoke();
        }
    }

    // -- Called from game scripts to trigger event-driven tips ----------------

    // Only fires after the initial tour is complete and only once per id
    public void TriggerStep(string id)
    {
        if (!_tourDone) return;
        if (_triggered.Contains(id)) return;
        _triggered.Add(id);

        TutorialStep step = _steps.Find(s => s.id == id);
        if (step != null) Activate(_steps.IndexOf(step));
    }

    // -- Positioning helpers -- call from your UI after receiving OnStepActivated

    // Returns the screen-space position the center of your popup should sit at
    public Vector2 GetPopupPosition(TutorialStep step, RectTransform popupRect)
    {
        if (step.target == null || step.anchor == PopupAnchor.Center)
            return Vector2.zero;

        Vector3[] corners = new Vector3[4];
        step.target.GetWorldCorners(corners);

        // Force z to zero -- avoids view frustum errors on perspective cameras
        for (int i = 0; i < 4; i++)
            corners[i].z = 0f;

        Vector2 centre = ((Vector2)corners[0] + (Vector2)corners[2]) * 0.5f;
        float tw = corners[2].x - corners[0].x;
        float th = corners[2].y - corners[0].y;
        float pw = popupRect.rect.width;
        float ph = popupRect.rect.height;

        switch (step.anchor)
        {
            case PopupAnchor.Above: return centre + new Vector2(0, th * 0.5f + ph * 0.5f + _popupOffset);
            case PopupAnchor.Below: return centre + new Vector2(0, -th * 0.5f - ph * 0.5f - _popupOffset);
            case PopupAnchor.Left: return centre + new Vector2(-tw * 0.5f - pw * 0.5f - _popupOffset, 0);
            case PopupAnchor.Right: return centre + new Vector2(tw * 0.5f + pw * 0.5f + _popupOffset, 0);
            default: return centre;
        }
    }

    // Returns rotation angle for your arrow sprite so it points back at the target
    // 0 = right   90 = up   180 = left   270 = down
    public float GetArrowAngle(TutorialStep step)
    {
        switch (step.anchor)
        {
            case PopupAnchor.Above: return 270f;
            case PopupAnchor.Below: return 90f;
            case PopupAnchor.Left: return 0f;
            case PopupAnchor.Right: return 180f;
            default: return 0f;
        }
    }

    // -- Internal -------------------------------------------------------------

    private void Activate(int index)
    {
        _index = index;
        TutorialStep step = _steps[index];

        if (step.pauseGame) Time.timeScale = 0f;

        OnStepActivated?.Invoke(step);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}