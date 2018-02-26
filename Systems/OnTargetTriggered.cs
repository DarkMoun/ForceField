using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;

public class OnTargetTriggered : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
    private Family targets = FamilyManager.getFamily(new AnyOfTags("Target"));

	public OnTargetTriggered(){
        //listeners on buttons Retry and Menu when level is cleared
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().levelClearedText.transform) {
			if (child.gameObject.name == "Retry") {
				child.gameObject.GetComponent<Button> ().onClick.AddListener (Retry);
			} else if (child.gameObject.name == "Menu") {
                if (GameInfo.loadedFromEditor)
                {
                    child.gameObject.GetComponentInChildren<Text>().text = "Edit";
                    child.gameObject.GetComponent<Button>().onClick.AddListener(Edit);
                }
                else
                {
                    child.gameObject.GetComponentInChildren<Text>().text = "Menu";
                    child.gameObject.GetComponent<Button>().onClick.AddListener(Menu);
                }
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
        if (!gameInfo.First().GetComponent<GameInfo>().triggered && !gameInfo.First().GetComponent<GameInfo>().levelEditorMode)
        {
            gameInfo.First().GetComponent<GameInfo>().levelCleared = false;
            foreach (GameObject target in targets)
            {
                Triggered3D t = target.GetComponent<Triggered3D>();
                if (t)
                {//if the target is triggered
                    foreach (GameObject go in t.Targets)
                    {//for each go that triggered the target
                        if (go.tag == "Object")
                        {//if go is the ball
                            foreach (FSystem s in FSystemManager.fixedUpdateSystems())
                            {
                                if (s.ToString() == "ApplyForce" || s.ToString() == "MoveSystem")
                                {
                                    s.Pause = true;//pause the game
                                }
                            }
                            gameInfo.First().GetComponent<GameInfo>().gamePaused = true;
                            gameInfo.First().GetComponent<GameInfo>().levelCleared = true;
                            gameInfo.First().GetComponent<GameInfo>().triggered = true;
                        }
                    }
                }
            }
        }
		gameInfo.First ().GetComponent<GameInfo> ().levelClearedText.SetActive (gameInfo.First().GetComponent<GameInfo>().levelCleared);//show the text if the level is cleared
	}

	void Retry()
    {
        gameInfo.First ().GetComponent<GameInfo> ().askResetLevel = true;//reset the level
    }

    void Edit()
    {
        gameInfo.First().GetComponent<GameInfo>().levelEditorMode = true;//enter editor mode
    }

    void Menu()
    {
        GameObjectManager.loadScene ("Menu");
	}
}