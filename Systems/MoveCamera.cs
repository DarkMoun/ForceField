using UnityEngine;
using FYFY;

public class MoveCamera : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

	public MoveCamera(){
		Camera.main.transform.position = new Vector3 (0, 62, 0);
	}

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameInfo gi = gameInfo.First ().GetComponent<GameInfo> ();
        //scroll to change the camera height between 2 and 108
        float newY = Camera.main.transform.position.y - Input.GetAxis("Mouse ScrollWheel") * 20;
        if (newY > 2 && newY < 108) {
			Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y - Input.GetAxis ("Mouse ScrollWheel") * 20, Camera.main.transform.position.z);
        }

        //save camera and mouse positions before drag
		if (Input.GetMouseButtonDown(0) && !gi.mouseOverHUD && gi.mouseNotOverObject) {//if left clicked got just pressed and not over UI or an object
			gi.cameraPosBeforeDrag = Camera.main.transform.position;
			gi.mousePosBeforeCameraDrag = Input.mousePosition;
			gi.canMoveCamera = true;
		}

        //move camera according to mouse position
		if (gi.canMoveCamera) {
			if (Input.GetMouseButton (0)) {//as long as the left click if pressed
				Camera.main.transform.position = gi.cameraPosBeforeDrag - (Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y)) - Camera.main.ScreenToWorldPoint (new Vector3 (gi.mousePosBeforeCameraDrag.x, gi.mousePosBeforeCameraDrag.y, Camera.main.transform.position.y)));
			}

			if (Input.GetMouseButtonUp (0)) {
				gi.canMoveCamera = false;
			}
		}
	}
}