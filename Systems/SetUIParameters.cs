﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using System;

public class SetUIParameters : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
	private Family selectable = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
	private Family movingGO = FamilyManager.getFamily(new AllOfComponents(typeof(Move)));
    private Family undoredo = FamilyManager.getFamily(new AllOfComponents(typeof(UndoRedoValues)));

    private bool changingSize = false;

    public SetUIParameters()
    {
        GameObject bP = gameInfo.First().GetComponent<GameInfo>().ballParameters;//parameters UI for the ball
        GameObject uiP = gameInfo.First().GetComponent<GameInfo>().uiParameters;//parameters UI for others
        GameObject ur = gameInfo.First().GetComponent<GameInfo>().uniformRotator;//UI for uniform force field rotation
        //add listenner on ui for "uiP", "bP" and "ur"
        foreach (Transform child in uiP.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "SizeSlider")
            {
                uiE.GetComponent<Slider>().onValueChanged.AddListener(SizeSliderChanged);
            }
            else if (uiE.name == "SizeInputField")
            {
                uiE.GetComponent<InputField>().onValueChanged.AddListener(SizeInputFieldChanged);
                uiE.GetComponent<InputField>().onEndEdit.AddListener(InputFieldValueChanged);
            }
            else if (uiE.name == "ValueSlider")
            {
                uiE.GetComponent<Slider>().onValueChanged.AddListener(ValueSliderChanged);
            }
            else if (uiE.name == "ValueInputField")
            {
                uiE.GetComponent<InputField>().onValueChanged.AddListener(ValueInputFieldChanged);
                uiE.GetComponent<InputField>().onEndEdit.AddListener(InputFieldValueChanged);
            }
        }
        foreach (Transform child in bP.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "DirectionSlider")
            {
                uiE.GetComponent<Slider>().onValueChanged.AddListener(DirectionSliderChanged);
            }
            else if (uiE.name == "SpeedSlider")
            {
                uiE.GetComponent<Slider>().onValueChanged.AddListener(SpeedSliderChanged);
            }
            else if (uiE.name == "SpeedInputField")
            {
                uiE.GetComponent<InputField>().onValueChanged.AddListener(SpeedInputFieldChanged);
                uiE.GetComponent<InputField>().onEndEdit.AddListener(InputFieldValueChanged);
            }
            else if (uiE.name == "MassSlider")
            {
                uiE.GetComponent<Slider>().onValueChanged.AddListener(MassSliderChanged);
            }
            else if (uiE.name == "MassInputField")
            {
                uiE.GetComponent<InputField>().onValueChanged.AddListener(MassInputFieldChanged);
                uiE.GetComponent<InputField>().onEndEdit.AddListener(InputFieldValueChanged);
            }
            else if (uiE.name == "ChargeSlider")
            {
                uiE.GetComponent<Slider>().onValueChanged.AddListener(ChargeSliderChanged);
            }
            else if (uiE.name == "ChargeInputField")
            {
                uiE.GetComponent<InputField>().onValueChanged.AddListener(ChargeInputFieldChanged);
                uiE.GetComponent<InputField>().onEndEdit.AddListener(InputFieldValueChanged);
            }
        }
        ur.GetComponentInChildren<Slider>().onValueChanged.AddListener(UrSliderChanged);
    }

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject uiP = gameInfo.First ().GetComponent<GameInfo> ().uiParameters;
		GameObject bP = gameInfo.First ().GetComponent<GameInfo> ().ballParameters;
		GameObject ur = gameInfo.First ().GetComponent<GameInfo> ().uniformRotator;
		GameObject editorUI = gameInfo.First ().GetComponent<GameInfo> ().editorUI;

		//set uiP values
		if (gameInfo.First ().GetComponent<GameInfo> ().selectedGO == 1) {//if only one object is selected
			foreach (GameObject go in selectable) {
				if (go.GetComponent<Clickable> ().isSelected) {
					if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedChanged) { //init parameters on selection
						if (go.tag == "ForceField") {
							foreach (Transform child in uiP.transform) {
								GameObject uiE = child.gameObject;
								if (uiE.name == "SizeSlider" && !gameInfo.First().gameObject.GetComponent<GameInfo>().levelEditorMode) {
									uiE.GetComponent<Slider> ().value = (go.transform.localScale.z - 3) / 17;
								} else if (uiE.name == "SizeInputField" && !gameInfo.First().gameObject.GetComponent<GameInfo>().levelEditorMode) {
									uiE.GetComponent<InputField> ().text = "" + go.GetComponent<ForceField> ().transform.localScale.z;
								} else if (uiE.name == "ValueSlider") {
									if (go.GetComponent<ForceField> ().ffType == 0) {
										uiE.GetComponent<Slider>().value = (go.GetComponent<Mass>().value - 1) / 999;
									} else if (go.GetComponent<ForceField> ().ffType == 1)
                                    {
                                        uiE.GetComponent<Slider>().value = (go.GetComponent<Charge>().value - 1) / 999;
                                    }
                                    else if (go.GetComponent<ForceField>().ffType == 2)
                                    {
                                        uiE.GetComponent<Slider>().value = (go.GetComponent<Charge>().value - 1) / 99;
                                    }
                                } else if (uiE.name == "ValueInputField")
                                {
                                    if (go.GetComponent<ForceField>().ffType == 0)
                                    {
                                        uiE.GetComponent<InputField>().text = "" + go.GetComponent<Mass>().value;
                                    }
                                    else if (go.GetComponent<ForceField>().ffType == 1 || go.GetComponent<ForceField>().ffType == 2)
                                    {
                                        uiE.GetComponent<InputField>().text = "" + go.GetComponent<Charge>().value;
                                    }
                                    foreach(Transform c in uiE.transform)
                                    {
                                        GameObject goc = c.gameObject;
                                        if(goc.name == "Placeholder")
                                        {
                                            if (go.GetComponent<ForceField>().ffType == 0)
                                            {
                                                goc.GetComponent<Text>().text = "Mass";
                                            }
                                            else if (go.GetComponent<ForceField>().ffType == 1 || go.GetComponent<ForceField>().ffType == 2)
                                            {
                                                goc.GetComponent<Text>().text = "Charge";
                                            }
                                        }
                                    }
                                }
                                else if (uiE.name == "Value")
                                {
                                    if (go.GetComponent<ForceField>().ffType == 0)
                                    {
                                        uiE.GetComponent<Text>().text = "Mass:";
                                    }
                                    else if (go.GetComponent<ForceField>().ffType == 1 || go.GetComponent<ForceField>().ffType == 2)
                                    {
                                        uiE.GetComponent<Text>().text = "Charge:";
                                    }
                                }
                            }
                            gameInfo.First().GetComponent<GameInfo>().ballParameters.SetActive(false);//hide bP
                            movingGO.First().GetComponent<Move>().directionGO.SetActive(false);//hide ball direction displayer
                            gameInfo.First ().GetComponent<GameInfo> ().uiParameters.SetActive (true);//show uiP
							if (go.GetComponent<ForceField> ().ffType == 2) {
								gameInfo.First ().GetComponent<GameInfo> ().uniformRotator.SetActive (true);//show ur
								ur.GetComponentInChildren<Slider> ().value = (go.GetComponent<ForceField>().direction/360)+0.5f;
                            }
						} else if (go.tag == "Object") {
							Move mv = movingGO.First ().GetComponent<Move> ();
							foreach (Transform child in bP.transform) {
								GameObject uiE = child.gameObject;
								if (uiE.name == "DirectionSlider") {
									if (mv.playerDirection.z > 0) {
										uiE.GetComponent<Slider> ().value = (float)(Math.Acos (mv.playerDirection.x / mv.playerDirection.magnitude) / (2 * Math.PI) + 0.5);
									} else {
										uiE.GetComponent<Slider> ().value = (float)(0.5-Math.Acos (mv.playerDirection.x / mv.playerDirection.magnitude) / (2 * Math.PI));
									}
                                } else if (uiE.name == "SpeedSlider") {
									uiE.GetComponent<Slider> ().value = (mv.speed)/100;
                                } else if (uiE.name == "SpeedInputField") {
									uiE.GetComponent<InputField> ().text = "" + mv.speed;
                                }
                                else if (uiE.name == "MassSlider")
                                {
                                    uiE.GetComponent<Slider>().value = (go.GetComponent<Mass>().value) / 100;
                                }
                                else if (uiE.name == "MassInputField")
                                {
                                    uiE.GetComponent<InputField>().text = "" + go.GetComponent<Mass>().value;
                                }
                                else if (uiE.name == "ChargeSlider")
                                {
                                    uiE.GetComponent<Slider>().value = (go.GetComponent<Charge>().value) / 100;
                                }
                                else if (uiE.name == "ChargeInputField")
                                {
                                    uiE.GetComponent<InputField>().text = "" + go.GetComponent<Charge>().value;
                                }
                            }
                            gameInfo.First().GetComponent<GameInfo>().uiParameters.SetActive(false);//hide uiP
                            gameInfo.First ().GetComponent<GameInfo> ().ballParameters.SetActive (true);//show bP
							mv.directionGO.SetActive(true);
						}
                        else
                        {
                            gameInfo.First().GetComponent<GameInfo>().uiParameters.SetActive(false);     //hide uiP
                            gameInfo.First().GetComponent<GameInfo>().ballParameters.SetActive(false);   //hide bP
                            movingGO.First().GetComponent<Move>().directionGO.SetActive(false);          //hide ball direction displayer
                            gameInfo.First().GetComponent<GameInfo>().uniformRotator.SetActive(false);   //hide ur
                        }
						if (gameInfo.First ().gameObject.GetComponent<GameInfo> ().levelEditorMode) {
							gameInfo.First ().gameObject.GetComponent<GameInfo> ().editorUI.SetActive (true);//show editorUI
							foreach (Transform child in editorUI.transform) {
								GameObject uiE = child.gameObject;
								if (uiE.name == "DeleteStatic") {//delete button for targets and obstacles
									if (go.tag == "Obstacle" || go.tag == "Target") {
										uiE.SetActive (true);//show the button if the selected object is an obstacle or a target
									} else {
										uiE.SetActive (false);
									}
								}
							}
						} else {
							gameInfo.First ().gameObject.GetComponent<GameInfo> ().editorUI.SetActive (false);//if not in editor mode hide editorUI
						}
						gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedChanged = false;
						if (go.GetComponent<IsEditable> ()) {//a go has an "IsEditable" component when it can be edited (for example an obstacle doesn't have this component out of the editor mode)
							foreach (Transform child in uiP.transform) {
								GameObject uiE = child.gameObject;
								if (uiE.name == "Locked") {//the locked ui element is an image that allow to see the ui but not to click on it
									uiE.SetActive (!go.GetComponent<IsEditable> ().isEditable && !gameInfo.First().GetComponent<GameInfo>().levelEditorMode);//uiP locked in play mode when the go is not editable
								} else if (uiE.name == "Delete") {//delete button
                                    if (!gameInfo.First().gameObject.GetComponent<GameInfo>().levelEditorMode)
                                    {
                                        if (go.GetComponent<Draggable>())
                                        {//a go has an "Draggable" component when it can be edited (for example an obstacle doesn't have this component out of the editor mode)
                                            uiE.SetActive(go.GetComponent<IsEditable>().isEditable && go.GetComponent<Draggable>().isDraggable);//the button is visible if the go is editable and draggable
                                        }
                                        else
                                        {
                                            uiE.SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        uiE.SetActive(true);
                                    }
								} else if(uiE.name == "LockedSize")
                                {
                                    uiE.SetActive(gameInfo.First().gameObject.GetComponent<GameInfo>().levelEditorMode);
                                }
							}
							foreach (Transform child in bP.transform) {
								GameObject uiE = child.gameObject;
								if (uiE.name == "Locked") {//the locked ui element is an image that allow to see the ui but not to click on it
									uiE.SetActive (!go.GetComponent<IsEditable> ().isEditable && !gameInfo.First().GetComponent<GameInfo>().levelEditorMode);//bP locked in play mode when the ball isn't editable
								}
							}
							foreach (Transform child in ur.transform) {
								GameObject uiE = child.gameObject;
								if (uiE.name == "Locked") {//the locked ui element is an image that allow to see the ui but not to click on it
									uiE.SetActive (!go.GetComponent<IsEditable> ().isEditable && !gameInfo.First().GetComponent<GameInfo>().levelEditorMode);//ur is locked in play mode when the force field can't be edited
								}
							}
						}
					}
                    if(go.tag == "Object")
                    {//change ball direction with drag
                        if (movingGO.First().GetComponent<Move>().directionGO.GetComponentInChildren<PointerOver>() && Input.GetMouseButtonDown(0) && !gameInfo.First().GetComponent<GameInfo>().playing)
                        {
                            gameInfo.First().GetComponent<GameInfo>().ballDirectionChanging = true;
                        }
                        if (gameInfo.First().GetComponent<GameInfo>().ballDirectionChanging)
                        {
                            Vector3 newDir = Vector3.RotateTowards(movingGO.First().GetComponent<Move>().directionGO.transform.forward, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f))- movingGO.First().GetComponent<Move>().directionGO.transform.position, 360, 0);
                            movingGO.First().GetComponent<Move>().directionGO.transform.rotation = Quaternion.LookRotation(newDir);
                            float value = (movingGO.First().GetComponent<Move>().directionGO.transform.rotation.eulerAngles.y - 90) / (-360) + 0.5f;
                            movingGO.First().GetComponent<Move>().direction = new Vector3((float)Math.Cos((value - 0.5) * 2 * Math.PI), 0, (float)Math.Sin((value - 0.5) * 2 * Math.PI));//set the new direction
                            movingGO.First().GetComponent<Move>().playerDirection = movingGO.First().GetComponent<Move>().direction;//store the direction set by the player
                            foreach (Transform child in bP.transform)
                            {
                                GameObject uiE = child.gameObject;
                                if (uiE.name == "DirectionSlider")
                                {
                                    uiE.GetComponent<Slider>().value = value;
                                }
                            }
                            Color uipColor = gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color;
                            Color uipdColor = gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color;
                            Color bpColor = gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color;
                            Color urColor = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color;
                            gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color = new Color(uipColor.r, uipColor.g, uipColor.b, 75f / 255);     //show uiP
                            gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color = new Color(uipdColor.r, uipdColor.g, uipdColor.b, 75f / 255);     //show uiP delete button
                            gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color = new Color(bpColor.r, bpColor.g, bpColor.b, 75f / 255);   //show bP
                            gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color = new Color(urColor.r, urColor.g, urColor.b, 75f / 255);   //show ur
                            if (Input.GetMouseButtonUp(0))
                            {
                                foreach (Transform child in bP.transform)
                                {
                                    GameObject uiE = child.gameObject;
                                    if (uiE.name == "DirectionSlider")
                                    {
                                        undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallDirection;
                                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                                        undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
                                    }
                                }
                                gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color = new Color(uipColor.r, uipColor.g, uipColor.b, 1);     //hide uiP
                                gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color = new Color(uipdColor.r, uipdColor.g, uipdColor.b, 1);     //hide uiP delete button
                                gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color = new Color(bpColor.r, bpColor.g, bpColor.b, 1);   //hide bP
                                gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color = new Color(urColor.r, urColor.g, urColor.b, 1);   //hide ur
                                gameInfo.First().GetComponent<GameInfo>().ballDirectionChanging = false;
                            }
                        }
                    }
                    if (bP.activeSelf)
                    {
                        Vector3 v = new Vector3(movingGO.First().transform.position.x, movingGO.First().transform.position.y, movingGO.First().transform.position.z - movingGO.First().transform.localScale.z * 2);
                        bP.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(v);
                        bP.GetComponent<RectTransform>().position = new Vector3(bP.GetComponent<RectTransform>().position.x + bP.GetComponent<RectTransform>().rect.width / 2 + 20, bP.GetComponent<RectTransform>().position.y, bP.GetComponent<RectTransform>().position.z);
                    }
                    else if(uiP.activeSelf && !changingSize)
                    {
                        Vector3 v = new Vector3(go.transform.position.x, go.transform.position.y, go.transform.position.z - go.transform.localScale.z / 2);
                        uiP.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(v);
                        uiP.GetComponent<RectTransform>().position = new Vector3(uiP.GetComponent<RectTransform>().position.x + uiP.GetComponent<RectTransform>().rect.width / 2 + 20, uiP.GetComponent<RectTransform>().position.y, uiP.GetComponent<RectTransform>().position.z);
                    }
				}
			}
		} else {//if number of selected objects != 1
			gameInfo.First ().GetComponent<GameInfo> ().uiParameters.SetActive (false);     //hide uiP
			gameInfo.First ().GetComponent<GameInfo> ().ballParameters.SetActive (false);   //hide bP
			movingGO.First().GetComponent<Move>().directionGO.SetActive(false);             //hide ball direction displayer
			gameInfo.First ().GetComponent<GameInfo> ().uniformRotator.SetActive (false);   //hide ur
			gameInfo.First ().GetComponent<GameInfo> ().editorUI.SetActive (false);         //hide editorUI
		}
        if(changingSize && Input.GetMouseButtonUp(0))
        {
            changingSize = false;
        }

        bool playing = gameInfo.First().GetComponent<GameInfo>().playing;
        foreach(Transform child in bP.transform)
        {
            if(child.gameObject.name == "LockedPlaying")
            {
                child.gameObject.SetActive(playing);
            }
        }
        foreach (Transform child in uiP.transform)
        {
            if (child.gameObject.name == "LockedPlaying")
            {
                child.gameObject.SetActive(playing);
            }
        }
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().uipDelete.transform)
        {
            if (child.gameObject.name == "LockedPlaying")
            {
                child.gameObject.SetActive(playing);
            }
        }
        foreach (Transform child in ur.transform)
        {
            if(child.gameObject.name == "LockedPlaying")
            {
                child.gameObject.SetActive(playing);
            }
        }
    }

	void SizeSliderChanged(float value){
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().uiParameters.transform) {
			GameObject uiE = child.gameObject;
			if (uiE.name == "SizeInputField") {
				uiE.GetComponent<InputField> ().text = ""+(value * 17 + 3);//set the input field to the new value
			}
            else if (uiE.name == "SizeSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderFFSize;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
            }
        }
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected && go.GetComponent<IsEditable>().isEditable) {
				go.transform.localScale = new Vector3 (value * 17 + 3, go.transform.localScale.y, value * 17 + 3);//set the new size
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;

            }
        }
        changingSize = true;
    }

	void SizeInputFieldChanged(string value){
		float f;
		float.TryParse (value, out f);
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().uiParameters.transform) {
			GameObject uiE = child.gameObject;
			if (uiE.name == "SizeSlider") {
				uiE.GetComponent<Slider> ().value = (f-3)/17;//set the slider to the new value
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderFFSize;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
            }
		}
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected && go.GetComponent<IsEditable>().isEditable) {
				go.transform.localScale = new Vector3 (f, go.transform.localScale.y, f);//set the new size
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;

            }
        }
        changingSize = true;
    }

	void ValueSliderChanged(float value){
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected) {
                //set the new value
				if (go.GetComponent<ForceField> ().ffType == 0) {
					go.GetComponent<Mass> ().value = value * 999 + 1;
				} else if (go.GetComponent<ForceField> ().ffType == 1)
                {
                    go.GetComponent<Charge>().value = value * 999 + 1;
                }
                else if (go.GetComponent<ForceField>().ffType == 2)
                {
                    go.GetComponent<Charge>().value = value * 99 + 1;
                }
                foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().uiParameters.transform) {
					GameObject uiE = child.gameObject;
					if (uiE.name == "ValueInputField") {
                        //set the input field to the new value
                        if (go.GetComponent<ForceField> ().ffType == 0 || go.GetComponent<ForceField> ().ffType == 1) {
							uiE.GetComponent<InputField> ().text = "" + (value * 999 + 1);
						} else if (go.GetComponent<ForceField> ().ffType == 2) {
							uiE.GetComponent<InputField> ().text = "" + (value * 99 + 1);
						}
                    }
                    else if (uiE.name == "ValueSlider")
                    {
                        undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderFFValue;
                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                        undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
                    }
                }
			}
		}
	}

	void ValueInputFieldChanged(string value){
		float f;
		float.TryParse (value, out f);
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected)
            {
                //set the new value
                if (go.GetComponent<ForceField>().ffType == 0)
                {
                    go.GetComponent<Mass>().value = f;
                }
                else if (go.GetComponent<ForceField>().ffType == 1)
                {
                    go.GetComponent<Charge>().value = f;
                }
                else if (go.GetComponent<ForceField>().ffType == 2)
                {
                    go.GetComponent<Charge>().value = f;
                }
                foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().uiParameters.transform) {
					GameObject uiE = child.gameObject;
					if (uiE.name == "ValueSlider") {
                        //set the slider to the new value
                        if (go.GetComponent<ForceField> ().ffType == 0 || go.GetComponent<ForceField> ().ffType == 1) {
							uiE.GetComponent<Slider> ().value = (f - 1) / 999;
						} else if (go.GetComponent<ForceField> ().ffType == 2) {
							uiE.GetComponent<Slider> ().value = (f - 1) / 99;
                        }
                        undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderFFValue;
                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                        undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
                    }
				}
			}
        }
    }

	void DirectionSliderChanged(float value){//ball direction
		movingGO.First ().GetComponent<Move> ().direction = new Vector3 ((float)Math.Cos ((value-0.5) * 2*Math.PI), 0, (float)Math.Sin ((value-0.5) * 2*Math.PI));//set the new direction
		movingGO.First ().GetComponent<Move> ().playerDirection = movingGO.First ().GetComponent<Move> ().direction;//store the direction set by the player
		movingGO.First ().GetComponent<Move> ().directionGO.transform.rotation = Quaternion.Euler(0,-(value-0.5f) * 360+90,0);//rotate the direction displayer
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "DirectionSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallDirection;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
    }

	void SpeedSliderChanged(float value){
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().ballParameters.transform) {
			GameObject uiE = child.gameObject;
			if (uiE.name == "SpeedInputField") {
				uiE.GetComponent<InputField> ().text = ""+(value * 100);//set the input field to the new value
            }
            else if (uiE.name == "SpeedSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallSpeed;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
		movingGO.First ().GetComponent<Move> ().speed = value *100;//set the new speed
		movingGO.First ().GetComponent<Move> ().playerSpeed = value * 100;//store the speed set by the player
    }

	void SpeedInputFieldChanged(string value){
		float f;
		float.TryParse (value, out f);
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().ballParameters.transform) {
			GameObject uiE = child.gameObject;
			if (uiE.name == "SpeedSlider") {
				uiE.GetComponent<Slider> ().value = (f)/100;//set the slider to the new value
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallSpeed;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
        movingGO.First ().GetComponent<Move> ().speed = f;//set the new speed
        movingGO.First ().GetComponent<Move> ().playerSpeed = f;//store the speed set by the player
    }

    void MassSliderChanged(float value)
    {
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "MassInputField")
            {
                uiE.GetComponent<InputField>().text = "" + (value * 100);//set the input field to the new value
            }
            else if (uiE.name == "MassSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallMass;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
        movingGO.First().GetComponent<Mass>().value = value * 100;//set the new mass
        movingGO.First().GetComponent<Mass>().playerValue = value * 100;//store the mass set by the player
    }

    void MassInputFieldChanged(string value)
    {
        float f;
        float.TryParse(value, out f);
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "MassSlider")
            {
                uiE.GetComponent<Slider>().value = (f) / 100;//set the slider to the new value
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallMass;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
        movingGO.First().GetComponent<Mass>().value = f;//set the new mass
        movingGO.First().GetComponent<Mass>().playerValue = f;//store the mass set by the player
    }

    void ChargeSliderChanged(float value)
    {
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "ChargeInputField")
            {
                uiE.GetComponent<InputField>().text = "" + (value * 100);//set the input field to the new value
            }
            else if (uiE.name == "ChargeSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallCharge;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
        movingGO.First().GetComponent<Charge>().value = value * 100;//set the new charge
        movingGO.First().GetComponent<Charge>().playerValue = value * 100;//store the charge set by the player
    }

    void ChargeInputFieldChanged(string value)
    {
        float f;
        float.TryParse(value, out f);
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "ChargeSlider")
            {
                uiE.GetComponent<Slider>().value = (f) / 100;//set the slider to the new value
                undoredo.First().GetComponent<UndoRedoValues>().slider = uiE.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderBallCharge;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = movingGO.First().GetComponent<IDUndoRedo>().id;
            }
        }
        movingGO.First().GetComponent<Charge>().value = f;//set the new charge
        movingGO.First().GetComponent<Charge>().playerValue = f;//store the charge set by the player
    }

    void UrSliderChanged(float value){//slider for uniform force field direction
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected)
            {
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
                go.GetComponent<ForceField> ().direction = (value-0.5f)*360;//set new direction
                if (go.GetComponent<ForceField>())
                {
                    go.transform.rotation = Quaternion.Euler(0, -(go.GetComponent<ForceField>().direction - 0.5f) + 180, 0);//rotate the force field
                }
            }
        }
        undoredo.First().GetComponent<UndoRedoValues>().slider = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponentInChildren<Slider>();
        undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderFFDirection;
        undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
    }

    void InputFieldValueChanged(string value)
    {
        undoredo.First().GetComponent<UndoRedoValues>().inputfieldChanged = true;
    }
}