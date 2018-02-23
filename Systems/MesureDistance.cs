using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class MesureDistance : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject distanceDisplayer = gameInfo.First ().GetComponent<GameInfo> ().distanceDisplayer;//UI
        GameObject dig = gameInfo.First().GetComponent<GameInfo>().distanceInGame;
        //if 2 objects are selected
        if (gameInfo.First().gameObject.GetComponent<GameInfo>().selectedGO == 2 && gameInfo.First ().gameObject.GetComponent<GameInfo> ().nbSelectableGO == 2) {//if 2 go are selected
			GameObject g1 = null;
			GameObject g2 = null;
			foreach (GameObject go in clickableGO) { //get selected objects
				bool selected = go.GetComponent<Clickable> ().isSelected;
				if (selected) {
					if (!g1) {
						g1 = go;
					} else {
						g2 = go;
					}
				}
			}
			distanceDisplayer.SetActive (true);
            dig.SetActive(true);
            distanceDisplayer.GetComponent<LineRenderer>().positionCount = 0;
            distanceDisplayer.GetComponent<LineRenderer>().positionCount = 2;
            distanceDisplayer.GetComponent<LineRenderer>().SetPosition(0, g1.transform.position);
            distanceDisplayer.GetComponent<LineRenderer>().SetPosition(1, g2.transform.position);
            Vector3 screenPosG1 = Camera.main.WorldToScreenPoint(g1.transform.position);
            Vector3 screenPosG2 = Camera.main.WorldToScreenPoint(g2.transform.position);
            if ((screenPosG2 - screenPosG1).x > 0)
            {
                dig.GetComponent<RectTransform>().position = (screenPosG1 + screenPosG2) / 2 - Vector3.Cross(screenPosG2 - screenPosG1, Vector3.forward).normalized * 30;
                dig.GetComponent<RectTransform>().rotation = Quaternion.Euler(dig.GetComponent<RectTransform>().rotation.eulerAngles.x, dig.GetComponent<RectTransform>().rotation.eulerAngles.y, -Vector3.Angle(screenPosG2 - screenPosG1, Vector3.up) + 90);
            }
            else
            {
                dig.GetComponent<RectTransform>().position = (screenPosG1 + screenPosG2) / 2 + Vector3.Cross(screenPosG2 - screenPosG1, Vector3.forward).normalized * 30;
                dig.GetComponent<RectTransform>().rotation = Quaternion.Euler(dig.GetComponent<RectTransform>().rotation.eulerAngles.x, dig.GetComponent<RectTransform>().rotation.eulerAngles.y, Vector3.Angle(screenPosG2 - screenPosG1, Vector3.up) - 90);
            }
            dig.GetComponent<Text>().text = "" + (g2.transform.position - g1.transform.position).magnitude;
            distanceDisplayer.GetComponentInChildren<Text> ().text = "Distance: " + (g2.transform.position - g1.transform.position).magnitude;//set the value in the UI
		} else if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedGO == 1)
        {//if 1 go is selected
            distanceDisplayer.GetComponent<LineRenderer>().positionCount = 0;
            distanceDisplayer.SetActive (true);
            dig.SetActive(false);
            foreach (GameObject go in clickableGO) { //get selected object
				if (go.GetComponent<Clickable> ().isSelected) {
					distanceDisplayer.GetComponentInChildren<Text> ().text = "X: " + (-go.transform.position.z) + "   Y: " + (go.transform.position.x);//set the value with the coordinate according to camera axes and not unity axes
				}
			}
		} else if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedGO == 0)
        {//if no object is selected
            distanceDisplayer.GetComponent<LineRenderer>().positionCount = 0;
            dig.SetActive(false);
            distanceDisplayer.SetActive (false);
		}
	}
}