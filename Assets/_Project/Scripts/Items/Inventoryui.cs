using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Mostra l'inventario. Va messo su un oggetto SEMPRE ATTIVO (ad esempio il Canvas),
// non sul pannello: altrimenti, una volta nascosto, non potrebbe più riaprirsi.
public class InventoryUI : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private FirstPersonController movement;

    [Header("Interfaccia")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private TMP_Text detailsText;

    private readonly List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private int hoveredIndex = -1;
    private bool isOpen;

    private void OnEnable()
    {
        if (inventory != null) inventory.Changed += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Refresh;
    }

    private void Start()
    {
        // Crea tanti riquadri quanti sono gli slot dell'inventario
        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            InventorySlotUI slotUI = Instantiate(slotPrefab, slotContainer);
            slotUI.Init(i, this);
            slotUIs.Add(slotUI);
        }

        Refresh();
        SetOpen(false);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
        {
            SetOpen(!isOpen);
        }
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        panel.SetActive(open);

        // Blocca visuale e movimento del giocatore mentre l'inventario è aperto
        if (movement != null) movement.IsUiOpen = open;

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        if (!open)
        {
            hoveredIndex = -1;
            ShowDetails(-1);
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].Refresh(inventory.Slots[i]);
        }

        ShowDetails(hoveredIndex);
    }

    // Chiamato dagli slot
    public void OnSlotClicked(int index, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left) inventory.UseItem(index);
        else if (button == PointerEventData.InputButton.Right) inventory.DropItem(index);
    }

    public void OnSlotHovered(int index)
    {
        hoveredIndex = index;
        ShowDetails(index);
    }

    private void ShowDetails(int index)
    {
        if (detailsText == null) return;

        if (index < 0 || index >= inventory.Slots.Count || inventory.Slots[index].IsEmpty)
        {
            detailsText.text = "";
            return;
        }

        ItemData item = inventory.Slots[index].item;
        detailsText.text = item.ItemName + "\n" + item.Description;
    }
}
