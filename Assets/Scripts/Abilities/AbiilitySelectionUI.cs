using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Shows a card selection panel at level 5 and every 5 levels after.
// Level 5: choose any ability. Level 10+ : upgrade current or swap.
public class AbilitySelectionUI : MonoBehaviour
{

    [Header("Ability Icons")]
    [SerializeField] private Sprite[] abilityIcons = new Sprite[5]; // Sniper, Boomerang, Bomb, Scatter, Blink

    public static AbilitySelectionUI Instance;

    private static readonly string[] Names = { "SNIPER", "BOOMERANG", "BOMB", "SCATTER", "BLINK" };



    private static readonly Color[] CardColors = {
        new Color(0.15f, 0.5f, 0.9f),
        new Color(0.15f, 0.75f, 0.35f),
        new Color(0.9f,  0.4f,  0.1f),
        new Color(0.85f, 0.75f, 0.1f),
        new Color(0.55f, 0.1f,  0.9f),
    };

    // Descriptions per ability per tier (0-indexed)
    private static readonly string[][] Desc = {
        new[] {
            "Piercing bullet\n4x damage\nHits all enemies in line",
            "6x damage\nFaster projectile\nWider hitbox",
            "8x damage\nInstant beam\nClears entire screen"
        },
        new[] {
            "Returns to you\n2x damage both ways",
            "Fires 2 boomerangs\n2x damage",
            "Fires 3 boomerangs\n3x damage"
        },
        new[] {
            "AoE explosion\n3x damage\nRadius 5",
            "4x damage\nRadius 7",
            "5x damage\nRadius 9\nSplits into clusters"
        },
        new[] {
            "10 bullets in cone\n0.6x damage each",
            "15 bullets\n0.7x damage each",
            "20 bullets\n0.8x damage each"
        },
        new[] {
            "Teleport up to 8 units",
            "Teleport up to 12 units\nDamage enemies on arrival",
            "Teleport up to 15 units\n2 charges per cooldown"
        }
    };

    private static readonly Color PanelBg = new Color(0.05f, 0.02f, 0.1f, 0.98f);
    private static readonly Color BtnSelect = new Color(0.25f, 0.5f, 0.25f, 1f);
    private static readonly Color BtnSwap = new Color(0.3f, 0.3f, 0.5f, 1f);
    private static readonly Color BtnMaxed = new Color(0.4f, 0.1f, 0.1f, 1f);

    private GameObject _panel;
    private TMP_Text _title;
    private Button[] _buttons = new Button[5];
    private TMP_Text[] _btnLabels = new TMP_Text[5];
    private TMP_Text[] _descTexts = new TMP_Text[5];
    private TMP_Text[] _tierTexts = new TMP_Text[5];
    private XPSystem _xpSystem;
    private bool _isInitial;

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (_panel == null)
        {
            BuildUI();
            _panel.SetActive(false);
            Subscribe();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from the old XPSystem
        if (_xpSystem != null)
            _xpSystem.OnLevelUp.RemoveListener(OnLevelUp);

        // Destroy the old panel if it still exists
        if (_panel != null)
            Destroy(_panel);

        // Rebuild fresh for the new scene
        BuildUI();
        _panel.SetActive(false);
        Subscribe();
    }

    private void Subscribe()
    {
        _xpSystem = FindAnyObjectByType<XPSystem>();
        if (_xpSystem != null)
            _xpSystem.OnLevelUp.AddListener(OnLevelUp);
    }

    private void OnLevelUp(int level)
    {
        if (level < 5 || level % 5 != 0) return;
        ShowPanel(level == 5);
    }

    private void ShowPanel(bool isInitial)
    {
        _isInitial = isInitial;
        _title.text = isInitial ? "CHOOSE YOUR ABILITY" : "UPGRADE OR SWAP";
        RefreshCards();
        _panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void HidePanel()
    {
        _panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void RefreshCards()
    {
        AbilityType? current = AbilityManager.Instance?.CurrentAbility;
        int tier = AbilityManager.Instance?.AbilityTier ?? 0;

        for (int i = 0; i < 5; i++)
        {
            bool isCurrent = current.HasValue && (int)current.Value == i;

            // Which tier description to show
            int showTier = 0;
            if (!_isInitial && isCurrent)
                showTier = Mathf.Clamp(tier, 0, 2); // show current tier or max

            _descTexts[i].text = Desc[i][showTier];

            if (isCurrent && !_isInitial)
                _tierTexts[i].text = "TIER " + tier + " / 3";
            else
                _tierTexts[i].text = "";

            // Button label and colour
            if (_isInitial)
            {
                _btnLabels[i].text = "SELECT";
                _buttons[i].interactable = true;
                SetBtnColor(_buttons[i], BtnSelect);
            }
            else if (isCurrent)
            {
                if (tier >= 3)
                {
                    _btnLabels[i].text = "MAXED";
                    _buttons[i].interactable = false;
                    SetBtnColor(_buttons[i], BtnMaxed);
                }
                else
                {
                    _btnLabels[i].text = "UPGRADE";
                    _buttons[i].interactable = true;
                    SetBtnColor(_buttons[i], BtnSelect);
                }
            }
            else
            {
                _btnLabels[i].text = current.HasValue ? "SWAP" : "SELECT";
                _buttons[i].interactable = true;
                SetBtnColor(_buttons[i], BtnSwap);
            }
        }
    }

    private void OnCardPicked(int index)
    {
        if (AbilityManager.Instance == null) return;

        bool isCurrent = AbilityManager.Instance.CurrentAbility.HasValue &&
                         (int)AbilityManager.Instance.CurrentAbility.Value == index;

        if (!_isInitial && isCurrent)
            AbilityManager.Instance.UpgradeAbility();
        else
            AbilityManager.Instance.SelectAbility((AbilityType)index);

        HidePanel();
    }

    private void SetBtnColor(Button btn, Color col)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = col;
    }

    // -- UI construction ------------------------------------------------------

    private void BuildUI()
    {
        GameObject cgo = new GameObject("AbilityCanvas");
        Canvas canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        CanvasScaler cs = cgo.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cgo.AddComponent<GraphicRaycaster>();

        _panel = MakeRect("Panel", cgo.transform);
        _panel.AddComponent<Image>().color = PanelBg;
        RectTransform pr = _panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(860, 460);
        pr.anchoredPosition = Vector2.zero;
        _panel.transform.localScale = Vector3.one * 1.15f;

        _title = MakeText("Title", _panel.transform, "", 22, FontStyles.Bold, TextAlignmentOptions.Center);
        _title.color = new Color(0.9f, 0.85f, 1f);
        RectTransform tr = _title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.offsetMin = new Vector2(0, -55); tr.offsetMax = new Vector2(0, -8);

        float cardW = 148f;
        float cardH = 350f;
        float startX = -330f;
        float gap = 165f;

        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            float x = startX + i * gap;
            Color accent = CardColors[i];
            Color dark = new Color(accent.r * 0.25f, accent.g * 0.25f, accent.b * 0.25f, 1f);

            // Card
            GameObject card = MakeRect("Card" + i, _panel.transform);
            card.AddComponent<Image>().color = dark;
            RectTransform cr = card.GetComponent<RectTransform>();
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(cardW, cardH);
            cr.anchoredPosition = new Vector2(x, 5f);

            // Colour stripe header
            GameObject stripe = MakeRect("Stripe", card.transform);
            stripe.AddComponent<Image>().color = accent;
            RectTransform sr = stripe.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0, 1); sr.anchorMax = new Vector2(1, 1);
            sr.offsetMin = new Vector2(0, -68); sr.offsetMax = Vector2.zero;

            TMP_Text nm = MakeText("Name", stripe.transform, Names[i], 12, FontStyles.Bold, TextAlignmentOptions.Center);
            nm.color = Color.white;
            RectTransform nr = nm.GetComponent<RectTransform>();
            nr.anchorMin = Vector2.zero; nr.anchorMax = Vector2.one;
            nr.offsetMin = nr.offsetMax = Vector2.zero;

            // Icon image -- sits below the name stripe
            GameObject iconGo = MakeRect("Icon", card.transform);
            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = abilityIcons != null && abilityIcons.Length > i ? abilityIcons[i] : null;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            RectTransform ir = iconGo.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.1f, 1f);
            ir.anchorMax = new Vector2(0.9f, 1f);
            ir.offsetMin = new Vector2(0, -160f);
            ir.offsetMax = new Vector2(0, -75f);

            // Tier label
            _tierTexts[i] = MakeText("Tier", card.transform, "", 9, FontStyles.Normal, TextAlignmentOptions.Center);
            _tierTexts[i].color = new Color(1f, 1f, 0.5f);
            RectTransform tt = _tierTexts[i].GetComponent<RectTransform>();
            tt.anchorMin = new Vector2(0, 1); 
            tt.anchorMax = new Vector2(1, 1);
            tt.offsetMin = new Vector2(4, -185f);
            tt.offsetMax = new Vector2(-4, -163f);

            // Description
            _descTexts[i] = MakeText("Desc", card.transform, "", 10, FontStyles.Normal, TextAlignmentOptions.Center);
            _descTexts[i].color = new Color(0.85f, 0.85f, 0.85f);
            RectTransform dr = _descTexts[i].GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0, 0); 
            dr.anchorMax = new Vector2(1, 1);
            dr.offsetMin = new Vector2(6, 48);
            dr.offsetMax = new Vector2(-6, -190f);

            // Button
            GameObject bg = MakeRect("Btn", card.transform);
            Image bImg = bg.AddComponent<Image>();
            bImg.color = BtnSelect;
            Button btn = bg.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            _buttons[i] = btn;
            RectTransform br = bg.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0, 0); br.anchorMax = new Vector2(1, 0);
            br.offsetMin = new Vector2(8, 10); br.offsetMax = new Vector2(-8, 44);

            _btnLabels[i] = MakeText("Lbl", bg.transform, "SELECT", 12, FontStyles.Bold, TextAlignmentOptions.Center);
            _btnLabels[i].color = Color.white;
            _btnLabels[i].raycastTarget = false;
            RectTransform bl = _btnLabels[i].GetComponent<RectTransform>();
            bl.anchorMin = Vector2.zero; bl.anchorMax = Vector2.one;
            bl.offsetMin = bl.offsetMax = Vector2.zero;

            btn.onClick.AddListener(() => OnCardPicked(idx));
        }
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
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.alignment = align; t.color = Color.white;
        return t;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_xpSystem != null)
            _xpSystem.OnLevelUp.RemoveListener(OnLevelUp);

        Time.timeScale = 1f;
    }
}