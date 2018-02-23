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
    private Family undoredo = FamilyManager.getFamily(new AllOfComponents(typeof(UndoRedoValues)));

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
                UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
                if (gameInfo.First().GetComponent<GameInfo>().levelEditorMode)
                {
                    ur.editorUndoActionTypes.Push(2);
                    if (go.tag == "ForceField")
                    {
                        ur.editorUndoDeletedDirections.Push(go.GetComponent<ForceField>().direction);
                        ur.editorUndoDeletedPositions.Push(go.transform.position);
                        ur.editorUndoDeletedSizes.Push(go.GetComponent<ForceField>().sizex);
                        ur.editorUndoDeletedSizes.Push(go.GetComponent<ForceField>().sizey);
                        ur.editorUndoDeletedTypes.Push(go.GetComponent<ForceField>().ffType);
                        if (go.GetComponent<ForceField>().ffType == 0)
                        {
                            ur.editorUndoDeletedValues.Push(go.GetComponent<Mass>().value);
                        }
                        else if (go.GetComponent<ForceField>().ffType == 1 || go.GetComponent<ForceField>().ffType == 2)
                        {
                            ur.editorUndoDeletedValues.Push(go.GetComponent<Charge>().value);
                        }
                    }
                    else
                    {
                        ur.editorUndoDeletedPositions.Push(go.transform.position);
                        ur.editorUndoDeletedSizes.Push(go.transform.localScale.z);
                        ur.editorUndoDeletedSizes.Push(go.transform.localScale.x);
                        if(go.tag == "Target")
                        {
                            ur.editorUndoDeletedTypes.Push(-1);
                        }
                        else if (go.name.Contains("CircleObstacle"))
                        {

                            ur.editorUndoDeletedTypes.Push(-2);
                        }
                        else if (go.name.Contains("SquareObstacle"))
                        {

                            ur.editorUndoDeletedTypes.Push(-3);
                        }
                    }
                    ur.editorUndoDeletedIDs.Push(go.GetComponent<IDUndoRedo>().id);
                }
                else
                {
                    if (go.tag == "ForceField")
                    {
                        ur.undoActionTypes.Push(2);
                        ur.undoDeletedDirections.Push(go.GetComponent<ForceField>().direction);
                        ur.undoDeletedPositions.Push(go.transform.position);
                        ur.undoDeletedSizes.Push(go.GetComponent<ForceField>().sizex);
                        ur.undoDeletedSizes.Push(go.GetComponent<ForceField>().sizey);
                        ur.undoDeletedTypes.Push(go.GetComponent<ForceField>().ffType);
                        if (go.GetComponent<ForceField>().ffType == 0)
                        {
                            ur.undoDeletedValues.Push(go.GetComponent<Mass>().value);
                        }
                        else if (go.GetComponent<ForceField>().ffType == 1 || go.GetComponent<ForceField>().ffType == 2)
                        {
                            ur.undoDeletedValues.Push(go.GetComponent<Charge>().value);
                        }
                        ur.undoDeletedIDs.Push(go.GetComponent<IDUndoRedo>().id);
                    }
                }
				GameObjectManager.unbind (go);//unbind to FYFY
                GameObject.Destroy(go);
			}
		}
	}
}