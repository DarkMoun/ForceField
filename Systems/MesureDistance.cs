using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class MesureDistance : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject distanceDisplayer = gameInfo.First ().GetComponent<GameInfo> ().distanceDisplayer;//UI
		//if 2 objects are selected
		if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedGO == 2) {//if 2 go are selected
			GameObject g1 = null;
			GameObject g2 = null;
			foreach (GameObject go in clickableGO) { //get selected objects
				bool selected = go.GetComponent<Clickable> ().isSelected;
				if (selected) {
					if (!g1) {
						g1 = go;
					} else {
						g2 = go;
					}
				}
			}
			distanceDisplayer.SetActive (true);
			distanceDisplayer.GetComponentInChildren<Text> ().text = "Distance: " + (g2.transform.position - g1.transform.position).magnitude;//set the value in the UI
		} else if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedGO == 1) {//if 1 go is selected
			distanceDisplayer.SetActive (true);
			foreach (GameObject go in clickableGO) { //get selected object
				if (go.GetComponent<Clickable> ().isSelected) {
					distanceDisplayer.GetComponentInChildren<Text> ().text = "X: " + (-go.transform.position.z) + "   Y: " + (go.transform.position.x);//set the value with the coordinate according to camera axes and not unity axes
				}
			}
		} else if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedGO == 0) {//if no object is selected
			distanceDisplayer.SetActive (false);
		}
	}
}