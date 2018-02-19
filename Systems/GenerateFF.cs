using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class GenerateFF : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family ffGenerator = FamilyManager.getFamily(new AnyOfTags("FFGenerator"));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
    private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));

    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
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
                    if (ff.GetComponent<ForceField>().ffType == 2)
                    {
                        ff.transform.Rotate(0, 180, 0);
                    }
                    ff.GetComponent<Draggable> ().dragged = true;//when created the force field is in "dragged" state
                    ff.GetComponent<Clickable>().isSelected = true;
                    //drag parameters
					gameInfo.First ().GetComponent<GameInfo> ().objectDragged = true;
					ff.GetComponent<Draggable> ().positionBeforeDrag = ff.transform.position;
					ff.GetComponent<Draggable> ().fromMouseToCenter = ff.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));
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
                    if (ff.GetComponent<ForceField>()){
                        if(ff.GetComponent<ForceField>().ffType == 2)
                        {
                            ff.transform.Rotate(0, 180, 0);
                        }
                    }
					ff.GetComponent<Draggable> ().dragged = true;//when created the object is in "dragged" state
                    ff.GetComponent<Clickable>().isSelected = true;
                    //drag parameters
                    gameInfo.First ().GetComponent<GameInfo> ().objectDragged = true;
					ff.GetComponent<Draggable> ().positionBeforeDrag = ff.transform.position;
					ff.GetComponent<Draggable> ().fromMouseToCenter = ff.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));
                }
			}
		}
	}
}