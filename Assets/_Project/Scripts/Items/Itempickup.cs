using UnityEngine;

// Un oggetto raccoglibile nel mondo. Ha bisogno di un Collider per essere visto dal raggio.
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData item;
    [SerializeField, Min(1)] private int quantity = 1;

    // Usato quando un oggetto viene lasciato a terra dall'inventario
    public void Setup(ItemData newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    public string GetPrompt()
    {
        if (item == null) return "";
        return quantity > 1
            ? "[E] Raccogli " + item.ItemName + " x" + quantity
            : "[E] Raccogli " + item.ItemName;
    }

    public void Interact(GameObject interactor)
    {
        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory == null || item == null) return;

        int remaining = inventory.AddItem(item, quantity);

        if (remaining <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            // Inventario pieno (o quasi): resta a terra quello che non è entrato
            quantity = remaining;
            Debug.Log("Inventario pieno!");
        }
    }
}
