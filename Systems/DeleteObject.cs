using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class DeleteObject : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
	private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
	private Family ffGenerator = FamilyManager.getFamily(new AnyOfTags("FFGenerator"));
	private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));

    public DeleteObject()
    {
        foreach (GameObject go in buttons)
        {
            if (go.name == "Delete" || go.name == "DeleteStatic")
            {
                go.GetComponent<Button>().onClick.AddListener(deleteGO);//add listener on delete buttons
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

	void deleteGO(){
		foreach (GameObject go in clickableGO) {
			if (go.GetComponent<Clickable> ().isSelected) {//find the selected object
				if (!gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode) {//if in play mode
					foreach (GameObject ffg in ffGenerator) {
						if (ffg.GetComponent<FFNbLimit> ().ffType == go.GetComponent<ForceField> ().ffType) {
							ffg.GetComponent<FFNbLimit> ().available++;//increase the number of force fields available in the corresponding generator
						}
					}
				}
				go.GetComponent<Clickable> ().isSelected = false;//unselect the object
				gameInfo.First ().GetComponent<GameInfo> ().selectedGO--;
				GameObjectManager.unbind (go);//unbind to FYFY
                GameObject.Destroy(go);
			}
		}
	}
}