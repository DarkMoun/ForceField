using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class PauseGame : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

    public PauseGame()
    {
        foreach (FSystem s in FSystemManager.fixedUpdateSystems())
        {
            if (s.ToString() == "ApplyForce" || s.ToString() == "MoveSystem")
            {
                s.Pause = true;//pause the game when level starts
            }
        }
        foreach (GameObject go in buttons)
        {
            if (go.name == "PauseButton")
            {
                go.GetComponent<Button>().onClick.AddListener(pauseGame);//add listener on pause button
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
	}

	//pause or resume the game
	void pauseGame(){
		bool gamePaused = gameInfo.First ().GetComponent<GameInfo> ().gamePaused;
		foreach (FSystem s in FSystemManager.fixedUpdateSystems()) {
			if (s.ToString() == "ApplyForce" || s.ToString() == "MoveSystem") {
				s.Pause = !gamePaused;
			}
		}
		gameInfo.First ().GetComponent<GameInfo> ().gamePaused = !gamePaused;
	}
}