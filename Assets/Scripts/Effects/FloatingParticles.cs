using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FloatingParticles : MonoBehaviour
{
    private ParticleSystem _ps;
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
        _ps = GetComponent<ParticleSystem>();
        Setup();
        _ps.Play();
    }

    private void LateUpdate()
    {
        // Follow the camera so particles always spawn in the visible area
        if (_cam != null)
            transform.position = new Vector3(
                _cam.transform.position.x,
                _cam.transform.position.y,
                0f
            );
    }

    private void Setup()
    {
        var main = _ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.33f);
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.8f, 0.2f, 1f, 0.8f),
            new Color(0.2f, 1f, 0.9f, 0.7f)
        );

        var emission = _ps.emission;
        emission.rateOverTime = 80f;

        var shape = _ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 14f;

        var vel = _ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
        vel.y = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f,   0f),
                new GradientAlphaKey(1f,   0.15f),
                new GradientAlphaKey(1f,   0.85f),
                new GradientAlphaKey(0f,   1f)
            }
        );

        var colorLife = _ps.colorOverLifetime;
        colorLife.enabled = true;
        colorLife.color = new ParticleSystem.MinMaxGradient(g);

        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 10;
    }
}