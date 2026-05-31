using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    private GameObject _panel;
    private TMP_Text _pointsLabel;
    private TMP_Text[] _levelLabels = new TMP_Text[6];
    private TMP_Text[] _valueLabels = new TMP_Text[6];
    private Button[] _buttons = new Button[6];
    private Button[] _minusButtons = new Button[6];
    private bool _open;
    public static bool IsOpen { get; private set; }

    private static readonly string[] Names = { "Power", "Fire Rate", "Move Speed", "Dash Distance", "Dash Cooldown", "Proj Range" };
    private static readonly string[] Units = { " dmg", "s", " spd", "s dur", "s cd", " units" };

    private static readonly Color PanelBg = new Color(0.06f, 0.03f, 0.12f, 0.97f);
    private static readonly Color RowBg = new Color(0f, 0f, 0f, 0.45f);
    private static readonly Color AccentOn = new Color(0.56f, 0.51f, 0.93f, 1f);
    private static readonly Color AccentOff = new Color(0.25f, 0.22f, 0.40f, 1f);

    private void Start()
    {
        BuildUI();

        _panel.SetActive(false);

        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnPointsChanged += _ => Refresh();
            UpgradeSystem.Instance.OnStatsChanged += Refresh;
        }
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            SetOpen(!_open);
    }

    private void SetOpen(bool open)
    {
        _open = open;
        IsOpen = open;
        _panel.SetActive(open);
        Time.timeScale = open ? 0f : 1f;

        if (open)
        {
            UISound.PlayClick();
            Refresh();
        }
    }

    private void Refresh()
    {
        if (UpgradeSystem.Instance == null) return;

        _pointsLabel.text = "POINTS AVAILABLE  " + UpgradeSystem.Instance.AvailablePoints;

        for (int i = 0; i < 6; i++)
        {
            var stat = (UpgradeStat)i;
            int level = UpgradeSystem.Instance.GetLevel(stat);
            float val = UpgradeSystem.Instance.GetCurrentValue(stat);

            _levelLabels[i].text = "LVL " + level + " / 10";
            _valueLabels[i].text = val.ToString("0.##") + Units[i];

            bool canUp = UpgradeSystem.Instance.CanUpgrade(stat);
            _buttons[i].interactable = canUp;
            ColorBlock cb = _buttons[i].colors;
            cb.normalColor = canUp ? AccentOn : AccentOff;
            cb.highlightedColor = canUp ? new Color(0.72f, 0.68f, 1f) : AccentOff;
            cb.disabledColor = AccentOff;
            _buttons[i].colors = cb;

            bool canDown = UpgradeSystem.Instance.CanDowngrade(stat);
            _minusButtons[i].interactable = canDown;
            ColorBlock mcb = _minusButtons[i].colors;
            mcb.normalColor = canDown ? AccentOn : AccentOff;
            mcb.highlightedColor = canDown ? new Color(0.72f, 0.68f, 1f) : AccentOff;
            mcb.disabledColor = AccentOff;
            _minusButtons[i].colors = mcb;
        }
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("UpgradeCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        _panel = MakeRect("Panel", canvasGo.transform);
        Image bg = _panel.AddComponent<Image>();
        bg.color = PanelBg;

        RectTransform pr = _panel.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.5f, 0.5f);
        pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(520, 460);
        pr.anchoredPosition = Vector2.zero;

        _panel.transform.localScale = Vector3.one * 1.4f;

        TMP_Text title = MakeText("Title", _panel.transform, "UPGRADES", 22, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0, 1);
        tr.anchorMax = new Vector2(1, 1);
        tr.offsetMin = new Vector2(0, -60);
        tr.offsetMax = new Vector2(0, -10);

        _pointsLabel = MakeText("Points", _panel.transform, "POINTS AVAILABLE  0", 13, FontStyles.Normal, TextAlignmentOptions.Center);
        _pointsLabel.color = AccentOn;
        RectTransform plr = _pointsLabel.GetComponent<RectTransform>();
        plr.anchorMin = new Vector2(0, 1);
        plr.anchorMax = new Vector2(1, 1);
        plr.offsetMin = new Vector2(0, -90);
        plr.offsetMax = new Vector2(0, -62);

        float rowH = 48f;
        float startY = -100f;

        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            float y = startY - i * (rowH + 6);

            GameObject row = MakeRect("Row" + i, _panel.transform);
            Image rowImg = row.AddComponent<Image>();
            rowImg.color = RowBg;
            RectTransform rr = row.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(0, 1);
            rr.anchorMax = new Vector2(1, 1);
            rr.offsetMin = new Vector2(16, y - rowH);
            rr.offsetMax = new Vector2(-16, y);

            TMP_Text nameLabel = MakeText("Name", row.transform, Names[i], 14, FontStyles.Bold, TextAlignmentOptions.Left);
            SetAnchors(nameLabel.GetComponent<RectTransform>(), 0, 0, 1, 0, 10, 0, 152, rowH);

            _levelLabels[i] = MakeText("Level", row.transform, "LVL 0 / 10", 11, FontStyles.Normal, TextAlignmentOptions.Left);
            _levelLabels[i].color = new Color(0.7f, 0.7f, 0.7f);
            SetAnchors(_levelLabels[i].GetComponent<RectTransform>(), 0, 0, 1, 0, 156, 0, 262, rowH);

            _valueLabels[i] = MakeText("Value", row.transform, "", 12, FontStyles.Normal, TextAlignmentOptions.Right);
            _valueLabels[i].color = AccentOn;
            SetAnchors(_valueLabels[i].GetComponent<RectTransform>(), 0, 0, 1, 0, 266, 0, 362, rowH);

            // Minus button
            GameObject minGo = MakeRect("BtnMinus", row.transform);
            Image minImg = minGo.AddComponent<Image>();
            minImg.color = AccentOff;

            Button minBtn = minGo.AddComponent<Button>();
            minBtn.transition = Selectable.Transition.ColorTint;
            _minusButtons[i] = minBtn;
            SetAnchors(minGo.GetComponent<RectTransform>(), 0, 0, 1, 0, 366, 6, 424, rowH - 6);

            TMP_Text minLabel = MakeText("MinusLabel", minGo.transform, "-", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            minLabel.color = Color.white;
            minLabel.raycastTarget = false;
            RectTransform ml = minLabel.GetComponent<RectTransform>();
            ml.anchorMin = Vector2.zero;
            ml.anchorMax = Vector2.one;
            ml.offsetMin = Vector2.zero;
            ml.offsetMax = Vector2.zero;

            int minIdx = i;
            minBtn.onClick.AddListener(() =>
            {
                if (UpgradeSystem.Instance.CanDowngrade((UpgradeStat)minIdx))
                {
                    UpgradeSystem.Instance.Downgrade((UpgradeStat)minIdx);
                    UISound.PlayUpgrade();
                }
            });

            // Plus button
            GameObject btnGo = MakeRect("BtnPlus", row.transform);
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = AccentOn;

            Button btn = btnGo.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            _buttons[i] = btn;
            SetAnchors(btnGo.GetComponent<RectTransform>(), 0, 0, 1, 0, 428, 6, 486, rowH - 6);

            TMP_Text btnLabel = MakeText("BtnLabel", btnGo.transform, "+", 18, FontStyles.Bold, TextAlignmentOptions.Center);
            btnLabel.color = Color.white;
            btnLabel.raycastTarget = false;
            RectTransform bl = btnLabel.GetComponent<RectTransform>();
            bl.anchorMin = Vector2.zero;
            bl.anchorMax = Vector2.one;
            bl.offsetMin = Vector2.zero;
            bl.offsetMax = Vector2.zero;

            btn.onClick.AddListener(() =>
            {
                if (UpgradeSystem.Instance.CanUpgrade((UpgradeStat)idx))
                {
                    UpgradeSystem.Instance.Upgrade((UpgradeStat)idx);
                    UISound.PlayUpgrade();
                }
            });
        }

        // Reset All button
        GameObject resetGo = MakeRect("ResetBtn", _panel.transform);
        Image resetImg = resetGo.AddComponent<Image>();
        resetImg.color = new Color(0.65f, 0.08f, 0.08f, 0.9f);
        Button resetBtn = resetGo.AddComponent<Button>();
        resetBtn.transition = Selectable.Transition.ColorTint;
        resetBtn.onClick.AddListener(() =>
        {
            UpgradeSystem.Instance?.ResetAll();
            UISound.PlayUpgrade();
        });

        RectTransform resetRT = resetGo.GetComponent<RectTransform>();
        resetRT.anchorMin = new Vector2(0f, 0f);
        resetRT.anchorMax = new Vector2(0f, 0f);
        resetRT.sizeDelta = new Vector2(130, 34);
        resetRT.anchoredPosition = new Vector2(74f, 20f);

        TMP_Text resetLabel = MakeText("ResetLabel", resetGo.transform, "RESET ALL", 11, FontStyles.Bold, TextAlignmentOptions.Center);
        resetLabel.color = Color.white;
        resetLabel.raycastTarget = false;
        RectTransform rl = resetLabel.GetComponent<RectTransform>();
        rl.anchorMin = Vector2.zero;
        rl.anchorMax = Vector2.one;
        rl.offsetMin = Vector2.zero;
        rl.offsetMax = Vector2.zero;

        TMP_Text hint = MakeText("Hint", _panel.transform, "TAB to close", 10, FontStyles.Normal, TextAlignmentOptions.Center);
        hint.color = new Color(1f, 1f, 1f, 0.3f);
        RectTransform hr = hint.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0, 0);
        hr.anchorMax = new Vector2(1, 0);
        hr.offsetMin = new Vector2(0, 6);
        hr.offsetMax = new Vector2(0, 26);
    }

    private GameObject MakeRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private TMP_Text MakeText(string name, Transform parent, string text, float size, FontStyles style, TextAlignmentOptions align)
    {
        GameObject go = MakeRect(name, parent);
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = Color.white;
        return t;
    }

    private void SetAnchors(RectTransform rt, float amin, float bmin, float amax, float bmax, float l, float b, float r, float t)
    {
        rt.anchorMin = new Vector2(amin, bmin);
        rt.anchorMax = new Vector2(amax, bmax);
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(r - 504, t);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}