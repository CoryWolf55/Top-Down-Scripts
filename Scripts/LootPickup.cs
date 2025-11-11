using UnityEngine;

public class LootPickup : MonoBehaviour
{
    public ItemData item;       // The item this pickup represents
    public int amount = 1;
    public bool beingTracked = false;

    public void Initialize(ItemData newItem, int newAmount)
    {
        item = newItem;
        amount = newAmount;

        // Optional: change visual/icon based on item
        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null && item.mesh != null)
        {
            // Could set a material texture or sprite here
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player pickup
        if (other.CompareTag("Player"))
        {
         //   InventoryManager.Instance.AddItem(item, amount);
            Claim(false);
            Destroy(gameObject);
        }

        // Drone pickup (auto-add to generator storage)
        if (other.CompareTag("Drone"))
        {
           // GeneratorStorage.Instance.TryAddItem(item, amount);
            Claim(false);
            Destroy(gameObject);
        }
    }
    public void Claim(bool b)
    {
        beingTracked = b;
    }
    public bool BeingPickedUp()
    {
        return beingTracked;
    }

}
