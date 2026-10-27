using UnityEngine;

// Tutto ciò con cui il giocatore può interagire premendo E (oggetti da raccogliere,
// porte, casse, alberi da tagliare...) implementa questa interfaccia.
public interface IInteractable
{
    string GetPrompt();                 // testo mostrato quando lo si guarda
    void Interact(GameObject interactor);
}