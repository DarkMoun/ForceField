using UnityEngine;

public class ForceField : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public float sizex;
	public float sizey;
    //initial values in the level, used to reset the level
	public float initialSizeX;
	public float initialSizeY;
	public float initialDirection;
	public int ffType; //0-attractive, 1-repulsive, 2-uniform
	public float direction;
}