using System;
using UnityEngine;

// Una statistica generica con valore attuale e massimo (fame, sete, stamina...).
// [Serializable] la rende visibile e modificabile nell'Inspector.
[Serializable]
public class Stat
{
    [SerializeField] private float max = 100f;

    public float Current { get; private set; }
    public float Max => max;
    public float Normalized => max > 0f ? Current / max : 0f;
    public bool IsEmpty => Current <= 0f;

    // Scatta ogni volta che il valore cambia: (valore attuale, valore massimo)
    public event Action<float, float> Changed;

    // Riporta la statistica al massimo
    public void Init()
    {
        Current = max;
        Changed?.Invoke(Current, max);
    }

    // Aggiunge (valore positivo) o toglie (valore negativo), restando tra 0 e max
    public void Change(float delta)
    {
        float previous = Current;
        Current = Mathf.Clamp(Current + delta, 0f, max);

        if (!Mathf.Approximately(previous, Current))
        {
            Changed?.Invoke(Current, max);
        }
    }
}
