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
                go.GetComponent<Button>().onClick.AddListener(resetLevel);
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
            ff.initialSizeX = ff.sizex;
            ff.initialSizeY = ff.sizey;
            ff.initialValue = ff.value;
            ff.initialDirection = ff.direction;
        }
        Move mv = movingGO.First().GetComponent<Move>();
        mv.initialPostion = mv.gameObject.transform.position;
        mv.initialSpeed = mv.speed;
        mv.playerSpeed = mv.speed;
        mv.initialDirection = mv.direction;
        mv.playerDirection = mv.direction;

        resetLevel();
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
			resetLevel ();
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
		gameInfo.First ().GetComponent<GameInfo> ().gamePaused = true;
	}

	void resetLevel(){
		resetBall ();
		Move mv = movingGO.First().GetComponent<Move> ();
        //set ball's values to initial values in the level
		mv.speed = mv.initialSpeed;
		mv.direction = mv.initialDirection;
		foreach (GameObject ff in forceFields) {//delete the force fields generated by generators
			if (ff.GetComponent<IsEditable> ().isEditable && ff.GetComponent<Draggable>().canBeMovedOutOfEditor) {//if the force field is editable and draggable
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
		/*foreach (GameObject go in draggable) {
			if (go.tag != "Obstacle") {
				go.transform.position = go.GetComponent<Draggable> ().initialPosition;
			}
		}*/
        //set force fields values to their initial values in the level
		foreach (GameObject go in forceFields) {
			ForceField ff = go.GetComponent<ForceField> ();
			ff.sizex = ff.initialSizeX;
			ff.sizey = ff.initialSizeY;
			ff.value = ff.initialValue;
			ff.direction = ff.initialDirection;
		}
	}
}