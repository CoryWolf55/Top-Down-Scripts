using UnityEngine;
using System.Collections;

public class GeneratorController : MonoBehaviour
{
    private Coroutine smoothTransition;
    private GameObject child;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float maxTransparency = 1f; // 1 = fully visible, 0 = invisible
    private float tLevel = 1f;

    private bool isActive;
    [SerializeField] private ParticleSystem ps;

    [Header("Power")]
    [SerializeField] private float unitsPerMin = 100;

    public void ToggleGenerator(float transparencyLevel)
    {
        
        
        tLevel = transparencyLevel;
        if(tLevel <= 0f) { tLevel = 0.1f; isActive = false; } //activate generator
        else { isActive = true; }
        // Minimum visibility level

        //Generate Power
        PowerManager.instance.UpdatePowerGeneration(unitsPerMin);


        //Apply effects
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

    

    private IEnumerator SmoothTransition(float targetAlpha, bool turningOn)
    {
        Renderer render = child.GetComponent<Renderer>();
        Material mat = render.material;

        Color baseColor = mat.color;
        Color baseEmission = mat.GetColor("_EmissionColor");

        float currentAlpha = baseColor.a;

      

        if (ps != null)
        {
            Debug.Log("Particle system found.");
            ps.loop = !turningOn;
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

           

            if (Mathf.Abs(currentAlpha - targetAlpha) < 0.01f)
                break;

            yield return null;
        }

        // Final cleanup
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, targetAlpha);
        mat.SetColor("_EmissionColor", baseEmission * targetAlpha);

       
    }
}
