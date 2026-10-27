using System;
using System.Collections.Generic;
using UnityEngine;

// La logica dell'inventario (nessuna UI qui dentro: se ne occupa InventoryUI).
public class Inventory : MonoBehaviour
{
    [SerializeField] private int slotCount = 20;
    [SerializeField] private Transform cameraTransform; // da dove escono gli oggetti lasciati a terra
    [SerializeField] private float dropDistance = 1.5f;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();
    private Health health;
    private SurvivalStats survivalStats;

    public IReadOnlyList<InventorySlot> Slots => slots;

    // Scatta a ogni modifica: la UI si aggiorna di conseguenza
    public event Action Changed;

    private void Awake()
    {
        health = GetComponent<Health>();
        survivalStats = GetComponent<SurvivalStats>();

        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    // Aggiunge l'oggetto: prima riempie le pile esistenti, poi usa gli slot vuoti.
    // Restituisce quanti pezzi NON sono entrati (0 = tutto aggiunto).
    public int AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        bool changed = false;

        // 1) Completa le pile già presenti
        foreach (InventorySlot slot in slots)
        {
            if (amount <= 0) break;
            if (slot.IsEmpty || slot.item != item || slot.quantity >= item.MaxStack) continue;

            int toAdd = Mathf.Min(item.MaxStack - slot.quantity, amount);
            slot.quantity += toAdd;
            amount -= toAdd;
            changed = true;
        }

        // 2) Usa gli slot vuoti
        foreach (InventorySlot slot in slots)
        {
            if (amount <= 0) break;
            if (!slot.IsEmpty) continue;

            int toAdd = Mathf.Min(item.MaxStack, amount);
            slot.item = item;
            slot.quantity = toAdd;
            amount -= toAdd;
            changed = true;
        }

        if (changed) Changed?.Invoke();
        return amount;
    }

    // Quanti pezzi di un oggetto possiedi in totale (servirà per il crafting)
    public int GetCount(ItemData item)
    {
        int total = 0;
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.item == item) total += slot.quantity;
        }
        return total;
    }

    // Toglie una certa quantità di un oggetto, se ce n'è abbastanza (servirà per il crafting)
    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0 || GetCount(item) < amount) return false;

        for (int i = slots.Count - 1; i >= 0 && amount > 0; i--)
        {
            InventorySlot slot = slots[i];
            if (slot.IsEmpty || slot.item != item) continue;

            int toRemove = Mathf.Min(slot.quantity, amount);
            slot.quantity -= toRemove;
            amount -= toRemove;
            if (slot.quantity <= 0) slot.Clear();
        }

        Changed?.Invoke();
        return true;
    }

    // Usa un oggetto consumabile: applica gli effetti e ne consuma uno
    public void UseItem(int index)
    {
        if (!IsValidIndex(index)) return;

        InventorySlot slot = slots[index];
        if (slot.IsEmpty) return;

        ItemData item = slot.item;
        if (!item.IsConsumable)
        {
            Debug.Log(item.ItemName + " non si può usare.");
            return;
        }

        if (health != null && item.HealthRestore > 0f) health.Heal(item.HealthRestore);

        if (survivalStats != null)
        {
            if (item.HungerRestore > 0f) survivalStats.Eat(item.HungerRestore);
            if (item.ThirstRestore > 0f) survivalStats.Drink(item.ThirstRestore);
        }

        slot.quantity--;
        if (slot.quantity <= 0) slot.Clear();
        Changed?.Invoke();
    }

    // Lascia a terra l'intera pila davanti al giocatore
    public void DropItem(int index)
    {
        if (!IsValidIndex(index)) return;

        InventorySlot slot = slots[index];
        if (slot.IsEmpty) return;

        if (slot.item.WorldPrefab == null)
        {
            Debug.LogWarning(slot.item.ItemName + " non ha un World Prefab: non può essere lasciato a terra.");
            return;
        }

        Vector3 position = cameraTransform != null
            ? cameraTransform.position + cameraTransform.forward * dropDistance
            : transform.position + transform.forward * dropDistance + Vector3.up;

        GameObject dropped = Instantiate(slot.item.WorldPrefab, position, Quaternion.identity);

        ItemPickup pickup = dropped.GetComponent<ItemPickup>();
        if (pickup != null) pickup.Setup(slot.item, slot.quantity);

        slot.Clear();
        Changed?.Invoke();
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < slots.Count;
    }
}
