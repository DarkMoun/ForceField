using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class Select : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
    private Family distButton = FamilyManager.getFamily(new AnyOfTags("DistanceButton"));
    private Family movingGO = FamilyManager.getFamily(new AllOfComponents(typeof(Move)));

    public Select()
    {
        distButton.First().GetComponent<Button>().onClick.AddListener(NbSelectableChange);
    }

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameInfo gi = gameInfo.First ().GetComponent<GameInfo> ();

        //count the number of go selected
		gi.selectedGO = 0;
		foreach (GameObject go in clickableGO) {
			if (go.GetComponent<Clickable> ().isSelected) {
				gi.selectedGO++;
			}
		}

        //mouse over an object?
		gi.mouseNotOverObject = true;
        if (movingGO.First().GetComponent<Move>().directionGO.GetComponentInChildren<PointerOver>())
        {
            gi.mouseNotOverObject = false;
        }
        else
        {
            foreach (GameObject go in clickableGO)
            {
                if (go.GetComponent<PointerOver>())
                {
                    gi.mouseNotOverObject = false;
                    break;
                }
            }
        }

		if (gi.selectedGO!=0 && Input.GetMouseButtonDown(0) && !gi.mouseOverHUD && gi.mouseNotOverObject) {//if there are selected objects and the left click is pressed over nothing
			foreach (GameObject g in clickableGO) {
                //unselect all objects
				if (g.GetComponent<Clickable> ().isSelected) {
					g.GetComponent<Clickable> ().isSelected = false;
				}
			}
		}

		foreach (GameObject go in clickableGO) {
			PointerOver po = go.GetComponent<PointerOver> ();
			//select/unselect objects
			if (po && !gi.mouseOverHUD && Input.GetMouseButtonDown(0)) { //if mouse over go and left mouse button clicked
                bool s = go.GetComponent<Clickable>().isSelected;
				if (gi.selectedGO == gi.nbSelectableGO && !go.GetComponent<Clickable> ().isSelected) {//if there is a selected go and the clicked go was not selected before click
					foreach (GameObject g in clickableGO) {
                        //unselect the selected go
						if (g.GetComponent<Clickable> ().isSelected) {
							g.GetComponent<Clickable> ().isSelected = false;
						}
					}
				}
				gi.selectedChanged = true;//true when the selected object just changed
                gi.selectedChangedEM = true;//same as selectedChanged but in editor mode
                go.GetComponent<Clickable> ().isSelected = !s;//select the clicked go or unselect it if it was already selected
			}

			bool selected = go.GetComponent<Clickable> ().isSelected;
			//Enable the good overlay
			if (selected) {
				foreach (Transform child in go.transform) {
					GameObject g = child.gameObject;
					if (g.name != "Direction") {
						if (g.tag == "SelectedOverlay") {//visible when the go is selected
							g.GetComponent<Renderer> ().enabled = true;
						} else {
							g.GetComponent<Renderer> ().enabled = false;
						}
					}
				}
			} else {
				if (po && !gi.mouseOverHUD) { //if mouse over
					foreach (Transform child in go.transform) {
						GameObject g = child.gameObject;
						if (g.name != "Direction") {
							if (g.tag == "MouseOverOverlay") {//visible when the mouse is over the go
								g.GetComponent<Renderer> ().enabled = true;
							} else {
								g.GetComponent<Renderer> ().enabled = false;
							}
						}
					}
				} else {//hide both overlay
					foreach (Transform child in go.transform) {
						GameObject g = child.gameObject;
						if ((g.tag == "MouseOverOverlay" || g.tag == "SelectedOverlay") && g.name != "Direction") {
							g.GetComponent<Renderer> ().enabled = false;
						}
					}
				}
			}

		}

        //count the number of selected go
		gi.selectedGO = 0;
		foreach (GameObject go in clickableGO) {
			if (go.GetComponent<Clickable> ().isSelected) {
				gi.selectedGO++;
			}
		}
	}

    void NbSelectableChange()
    {
        if(gameInfo.First().GetComponent<GameInfo>().nbSelectableGO == 1)
        {
            gameInfo.First().GetComponent<GameInfo>().nbSelectableGO = 2;
        }
        else
        {
            gameInfo.First().GetComponent<GameInfo>().nbSelectableGO = 1;
            foreach (GameObject g in clickableGO)
            {
                //unselect all objects
                if (g.GetComponent<Clickable>().isSelected)
                {
                    g.GetComponent<Clickable>().isSelected = false;
                }
            }
        }
    }
}