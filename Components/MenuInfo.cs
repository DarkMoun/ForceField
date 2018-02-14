using UnityEngine;
using UnityEngine.UI;

public class MenuInfo : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public GameObject buttonPrefab;
    public int nbILevel;//number of levels created with the unity editor
    public int nbCLevel;//number of levels created with the game editor

    public MenuInfo()
    {
        nbILevel = 11;
    }
}