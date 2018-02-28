using UnityEngine;
using System.Collections.Generic;

public class Move : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Vector3 direction;
	public float speed;
	public Vector3 initialPostion;      //initial values are used when the level is reset
	public Vector3 initialDirection;
	public float initialSpeed;
    public Vector3 playerDirection;     //player values are used when the player reset only the ball
    public float playerSpeed;
	public GameObject directionGO;      //gameobject display the ball direction
    public List<Vector3> lineRendererPositions;
}