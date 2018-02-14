using UnityEngine;
using UnityEngine.UI;
using FYFY;

public class MoveSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family movingGO = FamilyManager.getFamily(new AllOfComponents(typeof(Move)));

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		//movingGO.First ().GetComponent<Mass> ().mass = 5f;
		GameObject go = movingGO.First();
		go.transform.position += go.GetComponent<Move> ().direction * go.GetComponent<Move> ().speed * Time.deltaTime;//move the ball
	}
}