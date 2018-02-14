using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class DetectMouseOverHUD : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		PointerOver po = GameObject.FindGameObjectWithTag ("HUD").GetComponent<PointerOver> ();
		if (po) {
			gameInfo.First ().GetComponent<GameInfo> ().mouseOverHUD = true;
		} else {
			gameInfo.First ().GetComponent<GameInfo> ().mouseOverHUD = false;
		}
	}
}