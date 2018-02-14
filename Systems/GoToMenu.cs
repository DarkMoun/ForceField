using UnityEngine;
using FYFY;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoToMenu : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

    public GoToMenu()
    {
        foreach (GameObject go in buttons)
        {
            if (go.name == "MenuButton")
            {
                //Add a listener to "Menu" button in the UI
                go.GetComponent<Button>().onClick.AddListener(loadMenu);
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

	void loadMenu(){
		GameObjectManager.loadScene ("Menu");
	}
}