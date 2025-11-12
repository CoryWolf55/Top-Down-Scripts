using UnityEngine;
using System.Collections;

public class GeneratorController : MonoBehaviour
{
    private Coroutine smoothTransition;
    private GameObject child;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float maxTransparency = 1f; // 1 = fully visible, 0 = invisible
    private float tLevel = 1f;
    private bool isActive = true;

    public void ActivateGenerator(float transparencyLevel)
    {
        if (isActive) return;
        isActive = true;

        tLevel = transparencyLevel;
        foreach (Transform Child in transform)
        {
            if (Child.name == "Glow")
            {
                child = Child.gameObject;

                if (smoothTransition != null)
                    StopCoroutine(smoothTransition);

                smoothTransition = StartCoroutine(SmoothTransition(tLevel, true));
            }
        }
    }

    public void DeactivateGenerator(float transparencyLevel)
    {
        if (!isActive) return;
        isActive = false;

        tLevel = transparencyLevel;
        foreach (Transform Child in transform)
        {
            if (Child.name == "Glow")
            {
                child = Child.gameObject;

                if (smoothTransition != null)
                    StopCoroutine(smoothTransition);

                smoothTransition = StartCoroutine(SmoothTransition(tLevel, false));
            }
        }
    }

    private IEnumerator SmoothTransition(float targetAlpha, bool turningOn)
    {
        Renderer render = child.GetComponent<Renderer>();
        Material mat = render.material;
        ParticleSystem ps = child.GetComponentInChildren<ParticleSystem>();

        Color baseColor = mat.color;
        Color baseEmission = mat.GetColor("_EmissionColor");

        float currentAlpha = baseColor.a;

        // --- Particle setup ---
        float startAlpha = 1f;
        float startRate = 0f;

        if (ps != null)
        {
            var main = ps.main;
            var emission = ps.emission;

            startAlpha = main.startColor.color.a;
            startRate = emission.rateOverTime.constant;

            if (turningOn && !ps.isPlaying)
                ps.Play();
        }

        while (true)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * speed);

            // Material alpha
            Color c = mat.color;
            c.a = currentAlpha;
            mat.color = c;

            // Material emission
            mat.SetColor("_EmissionColor", baseEmission * currentAlpha);

            // --- Fade particle system ---
            if (ps != null)
            {
                var main = ps.main;
                var emission = ps.emission;

                // fade alpha
                Color psColor = main.startColor.color;
                psColor.a = startAlpha * currentAlpha;
                main.startColor = psColor;

                // fade emission rate
                emission.rateOverTime = startRate * currentAlpha;
            }

            if (Mathf.Abs(currentAlpha - targetAlpha) < 0.01f)
                break;

            yield return null;
        }

        // Final cleanup
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, targetAlpha);
        mat.SetColor("_EmissionColor", baseEmission * targetAlpha);

        if (ps != null)
        {
            var main = ps.main;
            var emission = ps.emission;

            Color psColor = main.startColor.color;
            psColor.a = startAlpha * targetAlpha;
            main.startColor = psColor;

            emission.rateOverTime = startRate * targetAlpha;

            // Stop cleanly when fully faded out
            if (!turningOn && targetAlpha <= 0.01f)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
