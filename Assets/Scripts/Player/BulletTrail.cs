using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [SerializeField] private Color trailColor = new Color(0.5f, 0.9f, 1f, 1f);

    private void Awake()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0f;
        trail.sortingOrder = 3;
        trail.generateLightingData = false;
        trail.material = new Material(Shader.Find("Sprites/Default"));

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white,  0f),
                new GradientColorKey(trailColor,   0.4f),
                new GradientColorKey(trailColor,   1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = g;
    }
}