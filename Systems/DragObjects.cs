using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class DragObjects : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family draggable = FamilyManager.getFamily(new AllOfComponents(typeof(Draggable), typeof(Clickable)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		bool mouseOverHUD = gameInfo.First ().GetComponent<GameInfo> ().mouseOverHUD;
		foreach (GameObject go in draggable) {
			PointerOver po = go.GetComponent<PointerOver> ();
			if (po && !mouseOverHUD) {//if mouse over the object and not over ui
				if (Input.GetMouseButtonDown (0)) {//if mouse's left button just got pressed
                    //set the go as "dragged"
					go.GetComponent<Draggable> ().dragged = true;//the go is currently dragged
					gameInfo.First ().GetComponent<GameInfo> ().objectDragged = true;//an object is dragged
					go.GetComponent<Draggable> ().positionBeforeDrag = go.transform.position;//store the current go position as position before drag
					go.GetComponent<Draggable> ().fromMouseToCenter = go.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));//vector from mouse position to screen center before drag (positions in the world)
				}
			}
		}
		if(!Input.GetMouseButton(0) && gameInfo.First ().GetComponent<GameInfo> ().objectDragged){//if the left click is released and an object is dragged
			foreach (GameObject go in draggable) {
				if (go.GetComponent<Draggable> ().dragged) {//find the dragged object
                    //set go to not dragged
					go.GetComponent<Draggable> ().dragged = false;
					gameInfo.First ().GetComponent<GameInfo> ().objectDragged = false;
					go.GetComponent<Draggable> ().canBeMoved = false;
				}
			}
		}
		if (gameInfo.First ().GetComponent<GameInfo> ().objectDragged) {//if an object is dragged
			foreach (GameObject go in draggable) {
				Draggable d = go.GetComponent<Draggable> ();
				if (d.isDraggable && d.dragged) {
					Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,Camera.main.transform.position.y-0.5f)) + d.fromMouseToCenter;//new position of the object
					if (!d.canBeMoved) {
						if ((newPos - d.positionBeforeDrag).magnitude > 1) {
                            /* the object can be moved only if the distance between the position before 
                             * the drag and the current position of the mouse is bigger than a certain value
                             * this way the object won't be dragged with a miss click */
							d.canBeMoved = true;
							go.GetComponent<Clickable> ().isSelected = true;//select the moved object
						}
					}
					if (d.canBeMoved) {
						go.transform.position = newPos;//move the object to the new position
					}
				}
			}
		}
	}
}