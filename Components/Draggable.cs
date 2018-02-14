using UnityEngine;

public class Draggable : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public bool isDraggable = true;
	public bool dragged = false;//true if the object is currently dragged

    /* the object can be moved only if the distance between the position before 
     * the drag and the current position of the mouse is bigger than a certain value
     * this way the object won't be dragged with a miss click */
    public bool canBeMoved = false;
	public bool canBeMovedOutOfEditor = false;//every object is draggable in editor mode, the value of isDraggable in play mode is stored in this bool
	public Vector3 initialPosition;//initial position in the level
	public Vector3 positionBeforeDrag;
	public Vector3 fromMouseToCenter;//vector from mouse position to screen center before drag (positions in the world)
}