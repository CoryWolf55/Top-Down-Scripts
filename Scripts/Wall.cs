using UnityEngine;

public class Wall : MonoBehaviour
{
    public float maxHealth = 100f;
    public bool isBreakable = true;
   
    public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (!isBreakable) return;

        currentHealth -= damage * Time.deltaTime; // scale per second

        Debug.Log($"{gameObject.name} took {damage} damage, remaining health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Vector3 l = this.transform.position;
            Destroy(gameObject);
            FlowfieldGenerator.instance.UpdateFlowfieldArea(l,0);

        }
    }
}
