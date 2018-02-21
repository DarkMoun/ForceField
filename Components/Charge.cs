using UnityEngine;

public class Charge : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public float value = 0;
    public float initialValue = 0;
    public float playerValue = 0;
    public static float K = 1;
}