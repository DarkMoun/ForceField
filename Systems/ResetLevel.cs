﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class ResetLevel : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
	private Family movingGO = FamilyManager.getFamily(new AllOfComponents(typeof(Move)));
	private Family draggable = FamilyManager.getFamily(new AllOfComponents(typeof(Draggable), typeof(Clickable)));
	private Family forceFields = FamilyManager.getFamily(new AllOfComponents(typeof(ForceField), typeof(IsEditable), typeof(Draggable)));
	private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable)));
	private Family ffGenerator = FamilyManager.getFamily(new AnyOfTags("FFGenerator"));
	private bool editorMode;

	public ResetLevel(){
		editorMode = gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode;//store the current game mode
        //add listener to both reset buttons
        foreach (GameObject go in buttons)
        {
            if (go.name == "ResetButton")
            {
                go.GetComponent<Button>().onClick.AddListener(resetAll);
            }
            else if (go.name == "ResetBallButton")
            {
                go.GetComponent<Button>().onClick.AddListener(resetBall);
            }
        }
        //set initial values of every object in the game
        foreach (GameObject go in draggable)
        {
            go.GetComponent<Draggable>().initialPosition = go.transform.position;
        }
        foreach (GameObject go in forceFields)
        {
            ForceField ff = go.GetComponent<ForceField>();
            ff.initialSizeX = ff.transform.localScale.z;
            ff.initialSizeY = ff.transform.localScale.x;
            if(ff.ffType == 0)
            {
                go.GetComponent<Mass>().initialValue = go.GetComponent<Mass>().value;
            }else if (ff.ffType == 1 || ff.ffType == 2)
            {
                go.GetComponent<Charge>().initialValue = go.GetComponent<Charge>().value;
            }
            ff.initialDirection = ff.direction;
        }
        Move mv = movingGO.First().GetComponent<Move>();
        mv.initialPostion = mv.gameObject.transform.position;
        mv.initialSpeed = mv.speed;
        mv.playerSpeed = mv.speed;
        mv.initialDirection = mv.direction;
        mv.playerDirection = mv.direction;

        resetAll();
    }

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (editorMode != gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode || gameInfo.First ().GetComponent<GameInfo> ().askResetLevel) {//if game mode changed or a system asked to reset the level
			resetAll ();
		}
		if (gameInfo.First ().GetComponent<GameInfo> ().askResetBall) {//if a system asked to reset the ball
			resetBall ();
		}
		editorMode = gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode;//store the current game mode
		gameInfo.First ().GetComponent<GameInfo> ().askResetLevel = false;
		gameInfo.First ().GetComponent<GameInfo> ().askResetBall = false;
	}

	void resetBall(){//reset ball parameters to the parameters set by the player
		
		GameObject obj = movingGO.First ();
		Move mv = obj.GetComponent<Move> ();
        //set ball values to player values
		mv.direction = mv.playerDirection;
		mv.speed = mv.playerSpeed;
		obj.transform.position = mv.initialPostion;
		foreach (FSystem s in FSystemManager.fixedUpdateSystems()) {
			if (s.ToString() == "ApplyForce" || s.ToString() == "MoveSystem") {
				s.Pause = true;//pause game
			}
		}
		foreach (GameObject g in clickableGO) {//unselect all objects
			if (g.GetComponent<Clickable> ().isSelected) {
				g.GetComponent<Clickable> ().isSelected = false;
			}
		}
        //hide end game texts
		gameInfo.First ().GetComponent<GameInfo> ().levelCleared = false;
		gameInfo.First ().GetComponent<GameInfo> ().levelLost = false;
        gameInfo.First().GetComponent<GameInfo>().triggered = false;
        gameInfo.First ().GetComponent<GameInfo> ().gamePaused = true;
        gameInfo.First().GetComponent<GameInfo>().playing = false; ;

        Color uipColor = gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color;
        Color uipdColor = gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color;
        Color bpColor = gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color;
        Color urColor = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color;
        gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color = new Color(uipColor.r, uipColor.g, uipColor.b, 1);     //show uiP
        gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color = new Color(uipdColor.r, uipdColor.g, uipdColor.b, 1);     //show uiP delete button
        gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color = new Color(bpColor.r, bpColor.g, bpColor.b, 1);   //show bP
        gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color = new Color(urColor.r, urColor.g, urColor.b, 1);   //show ur
    }

	void resetAll(){
		resetBall ();
		Move mv = movingGO.First().GetComponent<Move> ();
        //set ball's values to initial values in the level
		mv.speed = mv.initialSpeed;
		mv.direction = mv.initialDirection;
        Vector3 newDir = Vector3.RotateTowards(mv.directionGO.transform.forward, mv.direction, 360, 0);
        mv.directionGO.transform.rotation = Quaternion.LookRotation(newDir);//rotate the direction displayer
        foreach (GameObject ff in forceFields) {//delete the force fields generated by generators
			if (ff.GetComponent<IsEditable> ().isEditable && ff.GetComponent<Draggable>().isDraggable) {//if the force field is editable and draggable
				foreach (GameObject ffg in ffGenerator) {
					if (ffg.GetComponent<FFNbLimit> ().ffType == ff.GetComponent<ForceField> ().ffType) {
						ffg.GetComponent<FFNbLimit> ().available++;//increase the number of available force fields every time one is deleted
					}
				}
                //deleted the force field
				GameObjectManager.unbind (ff);
				GameObject.Destroy (ff);
			}
		}
        //set force fields values to their initial values in the level
		foreach (GameObject go in forceFields) {
			ForceField ff = go.GetComponent<ForceField> ();
            ff.transform.localScale = new Vector3(ff.initialSizeX, ff.transform.localScale.y, ff.initialSizeY);
            if (ff.ffType == 0)
            {
                go.GetComponent<Mass>().value = go.GetComponent<Mass>().initialValue;
            }
            else if (ff.ffType == 1 || ff.ffType == 2)
            {
                go.GetComponent<Charge>().value = go.GetComponent<Charge>().initialValue;
            }
            ff.direction = ff.initialDirection;
		}
        gameInfo.First().GetComponent<GameInfo>().askClearPlayUndo = true;
    }
}