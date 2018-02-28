using UnityEngine;
using UnityEngine.UI;
using FYFY;
using System.Collections.Generic;

public class MoveSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family movingGO = FamilyManager.getFamily(new AllOfComponents(typeof(Move)));
    private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));

    public MoveSystem()
    {
        movingGO.First().GetComponent<Move>().lineRendererPositions = new List<Vector3>();
        movingGO.First().GetComponentInChildren<LineRenderer>().positionCount = 0;
    }

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		//movingGO.First ().GetComponent<Mass> ().mass = 5f;
		GameObject go = movingGO.First();
        if (gameInfo.First().GetComponent<GameInfo>().playing)
        {
            Vector3 vertex = new Vector3(go.transform.position.x, 1, go.transform.position.z);
            go.GetComponent<Move>().lineRendererPositions.Add(vertex);
            LineRenderer lr = go.GetComponentInChildren<LineRenderer>();
            lr.positionCount = 0;
            lr.positionCount = go.GetComponent<Move>().lineRendererPositions.Count;
            lr.SetPositions(go.GetComponent<Move>().lineRendererPositions.ToArray());
        }
		go.transform.position += go.GetComponent<Move> ().direction * go.GetComponent<Move> ().speed * Time.deltaTime;//move the ball
	}
}