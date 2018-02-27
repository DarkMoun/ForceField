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
        Color uipColor = gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color;
        Color uipdColor = gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color;
        Color bpColor = gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color;
        Color urColor = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color;
        gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color = new Color(uipColor.r, uipColor.g, uipColor.b, 75f / 255);     //hide uiP
        gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color = new Color(uipdColor.r, uipdColor.g, uipdColor.b, 75f / 255);     //hide uiP delete button
        gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color = new Color(bpColor.r, bpColor.g, bpColor.b, 75f / 255);   //hide bP
        gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color = new Color(urColor.r, urColor.g, urColor.b, 75f / 255);   //hide ur
    }
}