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



        if (item.mesh != null)
        {
            Vector3 scale = transform.localScale;
            MeshFilter mf = GetComponent<MeshFilter>();
            mf.sharedMesh = item.mesh;  // or .mesh if you want a unique copy
            transform.localScale = scale;

            Debug.Log("LootPickup initialized with item: " + item.itemName + " Amount: " + amount);
        }
        else
        {
            Debug.LogWarning("Mesh is null on LootPickup initialization.");
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

    public bool Available()
    {
        return !beingTracked;
    }

}
