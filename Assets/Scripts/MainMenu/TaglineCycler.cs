using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaglineCycler : MonoBehaviour
{
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private float fadeTime = 0.5f;

    private TMP_Text _tmp;

    private List<string> _taglines = new List<string>
    {
        "touch grass simulator",
        "your eyes will thank you",
        "epilepsy warning: just kidding... maybe",
        "not responsible for existential crises",
        "side effects may include fun",
        "wave 20 is a myth",
        "the enemies are also confused",
        "no refunds",
        "rated E for everyone... probably",
        "made with love and poor decisions"
    };

    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        StartCoroutine(Cycle());
    }

    private IEnumerator Cycle()
    {
        int index = Random.Range(0, _taglines.Count);

        while (true)
        {
            _tmp.text = _taglines[index];
            yield return StartCoroutine(FadeTo(1f));
            yield return new WaitForSeconds(displayTime);
            yield return StartCoroutine(FadeTo(0f));
            index = (index + 1) % _taglines.Count;
        }
    }

    private IEnumerator FadeTo(float target)
    {
        float start = _tmp.color.a;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            Color c = _tmp.color;
            c.a = Mathf.Lerp(start, target, elapsed / fadeTime);
            _tmp.color = c;
            yield return null;
        }

        Color final = _tmp.color;
        final.a = target;
        _tmp.color = final;
    }
}