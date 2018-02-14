using UnityEngine;

public class FFNbLimit : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public int max;//number max of force field available in the level
	public int available;//number of force field currently available in the level
	public GameObject ff;//prefab of the object generated (not only force fields)
	public int ffType;//type of the object (used only for force fields)
}