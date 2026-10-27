using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Un singolo riquadro dell'inventario. Si trova sul prefab dello slot.
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;  // opzionale: utile finché non hai le icone
    [SerializeField] private TMP_Text countText;

    private int index;
    private InventoryUI owner;

    public void Init(int slotIndex, InventoryUI inventoryUI)
    {
        index = slotIndex;
        owner = inventoryUI;
    }

    public void Refresh(InventorySlot slot)
    {
        if (slot.IsEmpty)
        {
            if (iconImage != null) iconImage.enabled = false;
            if (nameText != null) nameText.text = "";
            if (countText != null) countText.text = "";
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = slot.item.Icon;
            iconImage.enabled = slot.item.Icon != null;
        }

        if (nameText != null) nameText.text = slot.item.ItemName;
        if (countText != null) countText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        owner.OnSlotClicked(index, eventData.button);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner.OnSlotHovered(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner.OnSlotHovered(-1);
    }

    private void Awake()
    {
        // Solo il riquadro principale riceve i clic: tutti i figli devono essere "trasparenti" al puntatore
        Graphic background = GetComponent<Graphic>();
        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
        {
            if (graphic != background) graphic.raycastTarget = false;
        }
    }
}
