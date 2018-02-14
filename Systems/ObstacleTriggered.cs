using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;

public class ObstacleTriggered : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

	public ObstacleTriggered(){
        //listeners on buttons Retry and Menu when level is failed
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().levelLostText.transform) {
			if (child.gameObject.name == "Retry") {
				child.gameObject.GetComponent<Button> ().onClick.AddListener (Retry);
			} else if (child.gameObject.name == "Menu") {
				child.gameObject.GetComponent<Button> ().onClick.AddListener (Menu);
			}
		}
	}

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		gameInfo.First ().GetComponent<GameInfo> ().levelLost = false;
		foreach (GameObject target in GameObject.FindGameObjectsWithTag("Obstacle")) {
			Triggered3D t = target.GetComponent<Triggered3D>();
			if (t) {//if the obstacle is triggered
				foreach (GameObject go in t.Targets) {//for each go that triggered the obstacle
					if (go.tag == "Object") {//if go is the ball
						foreach (FSystem s in FSystemManager.fixedUpdateSystems()) {
							if (s.ToString () == "ApplyForce" || s.ToString() == "MoveSystem") {
								s.Pause = true;//pause the game
							}
						}
						gameInfo.First ().GetComponent<GameInfo> ().gamePaused = true;
						gameInfo.First ().GetComponent<GameInfo> ().levelLost = true;
					}
				}
			}
		}
		gameInfo.First ().GetComponent<GameInfo> ().levelLostText.SetActive (gameInfo.First().GetComponent<GameInfo>().levelLost);//show the text if the level is failed
    }

	void Retry(){
		gameInfo.First ().GetComponent<GameInfo> ().askResetBall = true;//reset the position of the ball
	}

	void Menu(){
		GameObjectManager.loadScene ("Menu");
	}
}