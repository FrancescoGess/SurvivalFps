using UnityEngine;

// Un oggetto del gioco (mela, bottiglia, legno...) definito come "dato" e non come codice.
// Si crea da Unity: tasto destro nel Project > Create > Survival > Item.
// Attenzione: i ScriptableObject contengono solo la definizione. Le quantità
// possedute dal giocatore stanno nell'inventario, mai qui dentro.
[CreateAssetMenu(fileName = "NewItem", menuName = "Survival/Item")]
public class ItemData : ScriptableObject
{
    [Header("Generale")]
    [SerializeField] private string itemName = "Nuovo oggetto";
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField, Min(1)] private int maxStack = 10;
    [SerializeField] private GameObject worldPrefab; // come appare quando viene lasciato a terra

    [Header("Consumabile")]
    [SerializeField] private bool isConsumable;
    [SerializeField] private float healthRestore;
    [SerializeField] private float hungerRestore;
    [SerializeField] private float thirstRestore;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => maxStack;
    public GameObject WorldPrefab => worldPrefab;
    public bool IsConsumable => isConsumable;
    public float HealthRestore => healthRestore;
    public float HungerRestore => hungerRestore;
    public float ThirstRestore => thirstRestore;
}
