using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class GenerateFF : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family ffGenerator = FamilyManager.getFamily(new AnyOfTags("FFGenerator"));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
    private Family undoredo = FamilyManager.getFamily(new AllOfComponents(typeof(UndoRedoValues)));
    private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));

    public GenerateFF()
    {
        foreach (GameObject ffg in ffGenerator)
        {
            if (ffg.name == "AttractiveCircleFieldGenerator"){
                ffg.GetComponentInChildren<InputField>().onEndEdit.AddListener(MaxA);
            }
            else if (ffg.name == "RepulsiveCircleFieldGenerator")
            {
                ffg.GetComponentInChildren<InputField>().onEndEdit.AddListener(MaxR);
            }
            else if (ffg.name == "UniformFieldGenerator")
            {
                ffg.GetComponentInChildren<InputField>().onEndEdit.AddListener(MaxU);
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
	protected override void onProcess(int familiesUpdateCount)
    {
        foreach (GameObject ffg in ffGenerator)
        {
            if ((ffg.name == "AttractiveCircleFieldGenerator" || ffg.name == "RepulsiveCircleFieldGenerator" || ffg.name == "UniformFieldGenerator") && gameInfo.First().GetComponent<GameInfo>().levelEditorMode)
            {//if the generator is a force field generator and onclick "max" inputfield
                if(ffg.GetComponentInChildren<InputField>().gameObject.GetComponent<PointerOver>() && Input.GetMouseButton(0) && !gameInfo.First().GetComponent<GameInfo>().objectDragged)
                {
                    int.TryParse(ffg.GetComponentInChildren<InputField>().text, out undoredo.First().GetComponent<UndoRedoValues>().maxValue);
                }
            }
        }
        if (!gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode) {
			if (!gameInfo.First ().GetComponent<GameInfo> ().generatorInitialised) {
                //initialize force fields generators
				foreach (GameObject ffg in ffGenerator) {
					foreach (Transform child in ffg.transform) {
						if (child.gameObject.name == "Max") {
							child.gameObject.SetActive (false);//hide the input field modifying the max in editor mode
						}
					}
					if (ffg.GetComponent<FFNbLimit> ().max == 0) {
						ffg.SetActive (false);//hide the generator in the level if max == 0
					} else {
						ffg.GetComponent<FFNbLimit> ().available = ffg.GetComponent<FFNbLimit> ().max;
					}
					
				}
				gameInfo.First ().GetComponent<GameInfo> ().generatorInitialised = true;
			}
			foreach (GameObject ffg in ffGenerator) {
				FFNbLimit limiter = ffg.GetComponent<FFNbLimit> ();
				if (ffg.name == "AttractiveCircleFieldGenerator" || ffg.name == "RepulsiveCircleFieldGenerator" || ffg.name == "UniformFieldGenerator") {//if the generator is a force field generator
					ffg.GetComponentInChildren<Text> ().text = limiter.available + "/" + limiter.max;
				}
				if (ffg.GetComponentInChildren<Image>().gameObject.GetComponent<PointerOver> () && Input.GetMouseButton (0) && !gameInfo.First ().GetComponent<GameInfo> ().objectDragged && limiter.available != 0) {//on click on the generator, if available !=0
					GameObject ff = Object.Instantiate (ffg.GetComponent<FFNbLimit> ().ff);//create a new force field corresponding to the generator
					ff.transform.SetParent (gameInfo.First ().transform);
					GameObjectManager.bind (ff);//bind to FYFY
					ff.transform.position = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));//set position to mouse position
                    ff.GetComponent<Draggable> ().dragged = true;//when created the force field is in "dragged" state
                    if (gameInfo.First().GetComponent<GameInfo>().selectedGO == gameInfo.First().GetComponent<GameInfo>().nbSelectableGO)
                    {
                        foreach (GameObject g in clickableGO)
                        {
                            //unselect all objects
                            if (g.GetComponent<Clickable>().isSelected)
                            {
                                g.GetComponent<Clickable>().isSelected = false;
                            }
                        }
                    }
                    ff.GetComponent<Clickable>().isSelected = true;
                    gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                    //drag parameters
                    gameInfo.First ().GetComponent<GameInfo> ().objectDragged = true;
					ff.GetComponent<Draggable> ().positionBeforeDrag = ff.transform.position;
					ff.GetComponent<Draggable> ().fromMouseToCenter = ff.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));
                    ff.GetComponent<IDUndoRedo>().id = undoredo.First().GetComponent<UndoRedoValues>().idCount;
                    undoredo.First().GetComponent<UndoRedoValues>().idCount++;
                    undoredo.First().GetComponent<UndoRedoValues>().undoActionTypes.Push(3);
                    undoredo.First().GetComponent<UndoRedoValues>().undoCreatedGO.Push(ff.GetComponent<IDUndoRedo>().id);
                    undoredo.First().GetComponent<UndoRedoValues>().goCreated = true;
                    undoredo.First().GetComponent<UndoRedoValues>().draggedAtCreation = true;
                    limiter.available--;
				}
			}
		} else {//in editor mode
			foreach (GameObject generator in ffGenerator) {
				foreach (Transform child in generator.transform) {
					if (child.gameObject.name == "Max") {
						child.gameObject.SetActive (true);//show the input field modifying the max in editor mode
                    }
				}
                //initialize generators
				if (!gameInfo.First ().GetComponent<GameInfo> ().generatorInitialised) {
					generator.SetActive (true);//all generators are visible (target and obstacle generators included)
					if (generator.GetComponentInChildren<Text> ()) {
						generator.GetComponentInChildren<Text> ().text = "";
					}
					gameInfo.First ().GetComponent<GameInfo> ().generatorInitialised = true;
				}
				if (generator.GetComponentInChildren<Image>().gameObject.GetComponent<PointerOver> () && Input.GetMouseButton (0) && !gameInfo.First ().GetComponent<GameInfo> ().objectDragged) {//on click on the generator
					GameObject ff = Object.Instantiate (generator.GetComponent<FFNbLimit> ().ff);//create a new object corresponding to the generator
					ff.transform.SetParent (gameInfo.First ().transform);
					GameObjectManager.bind (ff);//bind to FYFY
					ff.transform.position = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));//set position to mouse position
					ff.GetComponent<Draggable> ().dragged = true;//when created the object is in "dragged" state
                    if (gameInfo.First().GetComponent<GameInfo>().selectedGO == gameInfo.First().GetComponent<GameInfo>().nbSelectableGO)
                    {
                        foreach (GameObject g in clickableGO)
                        {
                            //unselect all objects
                            if (g.GetComponent<Clickable>().isSelected)
                            {
                                g.GetComponent<Clickable>().isSelected = false;
                            }
                        }
                    }
                    ff.GetComponent<Clickable>().isSelected = true;
                    gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                    gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                    //drag parameters
                    gameInfo.First ().GetComponent<GameInfo> ().objectDragged = true;
					ff.GetComponent<Draggable> ().positionBeforeDrag = ff.transform.position;
					ff.GetComponent<Draggable> ().fromMouseToCenter = ff.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));
                    ff.GetComponent<IDUndoRedo>().id = undoredo.First().GetComponent<UndoRedoValues>().idCount;
                    undoredo.First().GetComponent<UndoRedoValues>().idCount++;
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(3);
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoCreatedGO.Push(ff.GetComponent<IDUndoRedo>().id);
                    undoredo.First().GetComponent<UndoRedoValues>().goCreated = true;
                    undoredo.First().GetComponent<UndoRedoValues>().draggedAtCreation = true;
                }
			}
		}
	}

    public void MaxA(string value)
    {
        int v;
        int.TryParse(value, out v);
        if(v != undoredo.First().GetComponent<UndoRedoValues>().maxValue)
        {
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(4);
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoGeneratorTypes.Push(0);
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoGeneratorMax.Push(undoredo.First().GetComponent<UndoRedoValues>().maxValue);
        }
    }

    public void MaxR(string value)
    {
        int v;
        int.TryParse(value, out v);
        if (v != undoredo.First().GetComponent<UndoRedoValues>().maxValue)
        {
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(4);
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoGeneratorTypes.Push(1);
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoGeneratorMax.Push(undoredo.First().GetComponent<UndoRedoValues>().maxValue);
        }
    }

    public void MaxU(string value)
    {
        int v;
        int.TryParse(value, out v);
        if (v != undoredo.First().GetComponent<UndoRedoValues>().maxValue)
        {
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(4);
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoGeneratorTypes.Push(2);
            undoredo.First().GetComponent<UndoRedoValues>().editorUndoGeneratorMax.Push(undoredo.First().GetComponent<UndoRedoValues>().maxValue);
        }
    }
}