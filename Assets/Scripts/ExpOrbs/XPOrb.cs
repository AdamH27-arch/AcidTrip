using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class XPOrb : MonoBehaviour
{
    [Header("Value")]
    [SerializeField] private int xpValue = 10;

    [Header("Visuals")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField][Range(0f, 1f)] private float collectSoundVolume = 0.8f;

    private Vector3 _startPos;

    // All active orbs register themselves here
    private static readonly List<XPOrb> _allOrbs = new List<XPOrb>();

    private void OnEnable() => _allOrbs.Add(this);
    private void OnDisable() => _allOrbs.Remove(this);

    // Read only access for external systems
    public static IReadOnlyList<XPOrb> AllOrbs => _allOrbs;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Bobbing is driven by _startPos -- moving _startPos moves the orb
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed + _startPos.x) * bobHeight;
        transform.position = new Vector3(_startPos.x, newY, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Collect();
    }

    public void SetValue(int value)
    {
        xpValue = value;
    }

    // Move _startPos toward the player -- Update carries the orb there naturally
    public void Vacuum(Transform target)
    {
        Debug.Log("Vacuum called on: " + gameObject.name);
        StartCoroutine(VacuumRoutine(target));
    }

    private IEnumerator VacuumRoutine(Transform target)
    {
        float elapsed = 0f;
        float timeLimit = 5f; // orbs give up after this many seconds

        while (target != null
               && Vector2.Distance(_startPos, target.position) > 0.3f
               && elapsed < timeLimit)
        {
            _startPos = Vector2.MoveTowards(_startPos, target.position, 4f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Only collect if the orb actually reached the player
        if (Vector2.Distance(_startPos, target.position) <= 0.3f)
            Collect();

        // Otherwise the orb just stops where it is and stays collectible normally
    }

    private void Collect()
    {
        // PlayClipAtPoint so the sound survives the Destroy call below
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position,
                collectSoundVolume * SettingsMenu.SFXVolume);

        XPSystem xpSystem = GameObject.FindWithTag("Player").GetComponent<XPSystem>();
        if (xpSystem != null)
            xpSystem.AddXP(xpValue);
        else
            Debug.LogWarning("XPOrb: could not find XPSystem on Player.");

        Destroy(gameObject);
    }
}