﻿using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class SetEditorMode : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
	private Family staticGO = FamilyManager.getFamily(new AnyOfTags("Obstacle", "Target", "Object"));
	private Family draggable = FamilyManager.getFamily(new AllOfComponents(typeof(Draggable), typeof(Clickable)));
	private Family ffGenerator = FamilyManager.getFamily(new AnyOfTags("FFGenerator"));
	private Family selectable = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
	private Family buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)));
	private Family forceFields = FamilyManager.getFamily(new AnyOfComponents(typeof(ForceField)));
	private Family movingGO = FamilyManager.getFamily(new AllOfComponents(typeof(Move)));
    private Family undoredo = FamilyManager.getFamily(new AllOfComponents(typeof(UndoRedoValues)));

    private bool erase = false;
    private bool edit = true;
    private bool canSave = false;

    public SetEditorMode() {
		draggable.addEntryCallback (setCanBeDragged);
		foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>()) {
			if (s.gameObject.name == "SizeXSlider") {
				s.onValueChanged.AddListener (XSlider);
			} else if (s.gameObject.name == "SizeYSlider") {
				s.onValueChanged.AddListener (YSlider);
			} else if (s.gameObject.name == "SizeBSlider") {
				s.onValueChanged.AddListener (BSlider);
			}
		}
		foreach (InputField i in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<InputField>()) {
			if (i.gameObject.name == "SizeXInputField") {
				i.onValueChanged.AddListener (XInputField);
                i.onEndEdit.AddListener(InputFieldValueChanged);
            } else if (i.gameObject.name == "SizeYInputField") {
				i.onValueChanged.AddListener (YInputField);
                i.onEndEdit.AddListener(InputFieldValueChanged);
            } else if (i.gameObject.name == "SizeBInputField") {
				i.onValueChanged.AddListener (BInputField);
                i.onEndEdit.AddListener(InputFieldValueChanged);
            }
		}
		foreach (Toggle t in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Toggle>()) {
			if (t.gameObject.name == "Editable") {
				t.onValueChanged.AddListener(EditableToggle);
			} else if (t.gameObject.name == "Draggable") {
				t.onValueChanged.AddListener(DraggableToggle);

			}
		}
		foreach (GameObject go in buttons) {
			if (go.name == "EditButton") {
				go.GetComponent<Button> ().onClick.AddListener (ActivateEditorMode);
			} else if (go.name == "TryButton") {
				go.GetComponent<Button> ().onClick.AddListener (CheckAlertTry);
			} else if (go.name == "SaveButton") {
				go.GetComponent<Button> ().onClick.AddListener (CheckAlertSave);
			} else if (go.name == "ValidateButton") {
				go.GetComponent<Button> ().onClick.AddListener (CheckAlertValidate);
			}else if(go.name == "Continue")
            {
                go.GetComponent<Button>().onClick.AddListener(SaveOrTryOrValidate);
            }
            else if (go.name == "Cancel")
            {
                go.GetComponent<Button>().onClick.AddListener(CancelAlert);
            }
            else if (go.name == "ValidateSaved")
            {
                go.GetComponent<Button>().onClick.AddListener(HideLevelSaved);
                HideLevelSaved();
            }
            else if (go.name == "ValidateNoTarget")
            {
                go.GetComponent<Button>().onClick.AddListener(HideNoTargetAlert);
                HideNoTargetAlert();
            }
            else if (go.name == "New")
            {
                go.GetComponent<Button>().onClick.AddListener(NewSave);
            }
            else if (go.name == "Erase")
            {
                go.GetComponent<Button>().onClick.AddListener(Erase);
            }
        }

        GameInfo gi = gameInfo.First().GetComponent<GameInfo>();
        if (!GameInfo.loadedFromEditor)
        {
            gi.levelEditorMode = false;
            LoadLevel("Level_" + GameInfo.loadedLevelID, false);
        }
        else
        {
            if (!GameInfo.newLevel)
            {
                LoadLevel("Editor_" + GameInfo.loadedLevelID, true);
            }
            gi.levelEditorMode = true;
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
		bool editorMode = gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode;
        if (gameInfo.First().GetComponent<GameInfo>().uiInitialisation)
        {
            gameInfo.First().GetComponent<GameInfo>().uiInitialisation = !gameInfo.First().GetComponent<GameInfo>().uiInitialisation;
        }
		foreach (Transform child in gameInfo.First ().GetComponent<GameInfo> ().gameButtons.transform) {
			GameObject go = child.gameObject;
			if (go.name == "Game") {
				go.SetActive (!editorMode);
			} else if (go.name == "Editor") {
				go.SetActive (editorMode);
			}
        }
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().gameButtons.transform)
        {
            if (child.gameObject.name == "Game")
            {
                foreach (Transform c in child.gameObject.transform)
                {
                    if (c.gameObject.name == "EditButton")
                    {
                        c.gameObject.SetActive(GameInfo.loadedFromEditor);//show the edit button if the level is loaded from Editor mode (from menu)
                    }
                }
            }
        }
        if (gameInfo.First().GetComponent<GameInfo>().validated)
        {
            canSave = true;
            gameInfo.First().GetComponent<GameInfo>().validated = false;
        }
        else
        {
            if (canSave)
            {
                erase = false;
                canSave = false;
                SaveLevel(false);
            }
        }
        if (editorMode) {
			RectTransform rt = gameInfo.First ().GetComponent<GameInfo> ().gameButtons.GetComponent<RectTransform> ();
			rt.sizeDelta = new Vector2 (171, rt.sizeDelta.y);
			if (gameInfo.First ().GetComponent<GameInfo> ().justEnteredEditorMode) {
                gameInfo.First().GetComponent<GameInfo>().validating = false;
                foreach (GameObject generator in ffGenerator) {
					generator.SetActive (true);
					if (generator.name == "AttractiveCircleFieldGenerator" || generator.name == "RepulsiveCircleFieldGenerator" || generator.name == "UniformFieldGenerator") {
						foreach (Transform child in generator.transform) {
							if (child.gameObject.name == "Max") {
								child.gameObject.SetActive (true);
								child.gameObject.GetComponent<InputField>().text = "" + generator.GetComponent<FFNbLimit> ().max;
							}
						}
					}
					generator.GetComponent<FFNbLimit> ().max = 0;
				}
				foreach (GameObject go in staticGO) {
					if (!go.GetComponent<Draggable> ()) {
						GameObjectManager.addComponent<Draggable> (go);
					}
				}
				foreach (GameObject go in draggable) {
					go.GetComponent<Draggable> ().canBeMovedOutOfEditor = go.GetComponent<Draggable> ().isDraggable;
					go.GetComponent<Draggable> ().isDraggable = true;
				}
				gameInfo.First ().GetComponent<GameInfo> ().justEnteredEditorMode = false;
				gameInfo.First ().GetComponent<GameInfo> ().generatorInitialised = false;
			}
			if (gameInfo.First ().GetComponent<GameInfo> ().selectedGO == 1 && gameInfo.First ().GetComponent<GameInfo> ().selectedChangedEM) {//if only one object is selected
                gameInfo.First().GetComponent<GameInfo>().uiInitialisation = true;
				foreach (GameObject go in selectable) {
					if (go.GetComponent<Clickable> ().isSelected) {
						bool isCircle = go.GetComponent<CapsuleCollider> ();
						bool isFF = go.tag == "ForceField";
						bool editable = isFF || go.tag == "Object";
						bool isBall = go.tag == "Object";
						foreach (Transform t in gameInfo.First().GetComponent<GameInfo>().editorUI.transform) {
							if (t.gameObject.name == "Size") {
								foreach (Transform child in t) {
									if (child.gameObject.name == "Locked") {
										if (isCircle || isBall) {
											child.gameObject.SetActive (true);
										} else {
											child.gameObject.SetActive (false);
										}
									} else if (child.gameObject.name == "LockedB") {
										if (isBall) {
											child.gameObject.SetActive (true);
										} else {
											child.gameObject.SetActive (false);
										}
									}
								}
							} else if (t.gameObject.name == "Editable") {
								foreach (Transform child in t) {
									if (child.gameObject.name == "Locked") {
										if (!editable) {
											child.gameObject.SetActive(true);
										} else {
											child.gameObject.SetActive(false);
										}
									}
								}
							} else if (t.gameObject.name == "Draggable") {
								foreach (Transform child in t) {
									if (child.gameObject.name == "Locked") {
										if (!isFF) {
											child.gameObject.SetActive(true);
										} else {
											child.gameObject.SetActive(false);
										}
									}
								}
							}
						}
						foreach (InputField i in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<InputField>()) {
							if (i.gameObject.name == "SizeXInputField") {
								i.text = "" + go.transform.localScale.z;
							} else if (i.gameObject.name == "SizeYInputField") {
								i.text = "" + go.transform.localScale.x;
							} else if (i.gameObject.name == "SizeBInputField") {
								i.text = "";
							}
						}
						foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>()) {
							if (s.gameObject.name == "SizeXSlider") {
								s.value = (go.transform.localScale.z-1)/29;
							} else if (s.gameObject.name == "SizeYSlider") {
								s.value = (go.transform.localScale.x-1)/29;
							}
						}
						foreach (Toggle t in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Toggle>()) {
							if (t.gameObject.name == "Editable" && go.GetComponent<IsEditable>()) {
								t.isOn = go.GetComponent<IsEditable>().isEditable;
							} else if (t.gameObject.name == "Draggable" && go.GetComponent<Draggable>()) {
								t.isOn = go.GetComponent<Draggable>().canBeMovedOutOfEditor;
							}
						}
						gameInfo.First ().gameObject.GetComponent<GameInfo> ().selectedChangedEM = false;
					}
				}
			}
		} else {
			RectTransform rt = gameInfo.First ().GetComponent<GameInfo> ().gameButtons.GetComponent<RectTransform> ();
			rt.sizeDelta = new Vector2 (365, rt.sizeDelta.y);
			if (!gameInfo.First ().GetComponent<GameInfo> ().justEnteredEditorMode) {
				foreach (GameObject go in staticGO) {
					GameObjectManager.removeComponent<Draggable>(go);
				}
				foreach (GameObject generator in ffGenerator) {
					if (generator.name == "TargetGenerator" || generator.name == "CircleObstacleGenerator" || generator.name == "SquareObstacleGenerator") {
						generator.SetActive (false);
					} else {
						int max = 0;
						int.TryParse (generator.GetComponentInChildren<InputField> ().text, out max);
						if (max < 0) {
							generator.GetComponent<FFNbLimit> ().max = 0;
						} else {
							generator.GetComponent<FFNbLimit> ().max = max;
						}
					}
					if (generator.GetComponentInChildren<InputField> ()) {
						generator.GetComponentInChildren<InputField> ().gameObject.SetActive (false);
					}
				}
				gameInfo.First ().GetComponent<GameInfo> ().justEnteredEditorMode = true;
				gameInfo.First ().GetComponent<GameInfo> ().generatorInitialised = false;
			}
		}
	}

	void setCanBeDragged(GameObject go){
        if (!undoredo.First().GetComponent<UndoRedoValues>().undoing)
        {
            go.GetComponent<Draggable>().canBeMovedOutOfEditor = false;
            go.GetComponent<Draggable>().isDraggable = true;
        }
	}

	void XSlider(float value){
		foreach (InputField i in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<InputField>()) {
			if (i.gameObject.name == "SizeXInputField") {
				i.text = "" + (value * 29+1);
            }
        }
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected) {
				go.transform.localScale = new Vector3 (go.transform.localScale.x, go.transform.localScale.y, value* 29+1);
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;

            }
        }
        foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>())
        {
            if (s.gameObject.name == "SizeXSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = s.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeX;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
            }
        }
    }

	void YSlider(float value){
		foreach (InputField i in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<InputField>()) {
			if (i.gameObject.name == "SizeYInputField") {
				i.text = "" + (value * 29+1);
			}
		}
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected) {
				go.transform.localScale = new Vector3 (value* 29+1, go.transform.localScale.y, go.transform.localScale.z);
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
            }
        }
        foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>())
        {
            if (s.gameObject.name == "SizeYSlider")
            {
                undoredo.First().GetComponent<UndoRedoValues>().slider = s.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeY;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
            }
        }
    }

	void BSlider(float value)
    {
        if (!gameInfo.First().GetComponent<GameInfo>().uiInitialisation)
        {
            foreach (InputField i in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<InputField>())
            {
                if (i.gameObject.name == "SizeBInputField")
                {
                    i.text = "" + (value * 29 + 1);
                }
            }
            foreach (GameObject go in selectable)
            {
                if (go.GetComponent<Clickable>().isSelected)
                {
                    go.transform.localScale = new Vector3(value * 29 + 1, go.transform.localScale.y, value * 29 + 1);
                    undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
                }
            }
            foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>())
            {
                if (s.gameObject.name == "SizeBSlider")
                {
                    undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeB;
                    undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeB2;
                }
                else if (s.gameObject.name == "SizeXSlider")
                {
                    undoredo.First().GetComponent<UndoRedoValues>().slider = s.GetComponent<Slider>();
                }
                else if (s.gameObject.name == "SizeYSlider")
                {
                    undoredo.First().GetComponent<UndoRedoValues>().slider2 = s.GetComponent<Slider>();
                }
            }
        }
	}

	void XInputField(string value){
		foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>()) {
			if (s.gameObject.name == "SizeXSlider") {
				float v;
				float.TryParse (value, out v);
				s.value = (v -1)/ 29;
                undoredo.First().GetComponent<UndoRedoValues>().slider = s.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeX;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
            }
		}
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected) {
				go.transform.localScale = new Vector3 (go.transform.localScale.x, go.transform.localScale.y, float.Parse(value));
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
            }
        }
    }

	void YInputField(string value){
		foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>()) {
			if (s.gameObject.name == "SizeYSlider") {
				float v;
				float.TryParse (value, out v);
				s.value = (v -1)/ 29;
                undoredo.First().GetComponent<UndoRedoValues>().slider = s.GetComponent<Slider>();
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeY;
                undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = -1;
            }
		}
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Clickable> ().isSelected) {
				go.transform.localScale = new Vector3 (float.Parse(value), go.transform.localScale.y, go.transform.localScale.z);
                undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
            }
        }
    }

	void BInputField(string value){
        if (!gameInfo.First().GetComponent<GameInfo>().uiInitialisation)
        {
            foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>())
            {
                if (s.gameObject.name == "SizeBSlider" || s.gameObject.name == "SizeXSlider" || s.gameObject.name == "SizeYSlider")
                {
                    float v;
                    float.TryParse(value, out v);
                    s.value = (v - 1) / 29;
                    if(s.gameObject.name == "SizeBSlider")
                    {
                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeB;
                        undoredo.First().GetComponent<UndoRedoValues>().sliderValue2 = undoredo.First().GetComponent<UndoRedoValues>().sliderESizeB2;
                    }
                    else if (s.gameObject.name == "SizeXSlider")
                    {
                        undoredo.First().GetComponent<UndoRedoValues>().slider = s.GetComponent<Slider>();
                    }
                    else if (s.gameObject.name == "SizeYSlider")
                    {
                        undoredo.First().GetComponent<UndoRedoValues>().slider2 = s.GetComponent<Slider>();
                    }
                }
            }
            foreach (GameObject go in selectable)
            {
                if (go.GetComponent<Clickable>().isSelected)
                {
                    float v;
                    float.TryParse(value, out v);
                    go.transform.localScale = new Vector3(v, go.transform.localScale.y, v);
                    undoredo.First().GetComponent<UndoRedoValues>().sliderGO = go.GetComponent<IDUndoRedo>().id;
                }
            }
        }
	}

	void EditableToggle(bool value){
		foreach (GameObject go in selectable) {
			if (go.GetComponent<IsEditable> () && go.GetComponent<Clickable>().isSelected) {
				go.GetComponent<IsEditable>().isEditable = value;
                if (!undoredo.First().GetComponent<UndoRedoValues>().goCreated && !undoredo.First().GetComponent<UndoRedoValues>().undoing && !undoredo.First().GetComponent<UndoRedoValues>().redoing && !undoredo.First().GetComponent<UndoRedoValues>().selectionChanged)
                {
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoFocusedObject.Push(go.GetComponent<IDUndoRedo>().id);
                }
            }
        }
        if (!undoredo.First().GetComponent<UndoRedoValues>().goCreated && !undoredo.First().GetComponent<UndoRedoValues>().undoing && !undoredo.First().GetComponent<UndoRedoValues>().redoing && !undoredo.First().GetComponent<UndoRedoValues>().selectionChanged)
        {
            foreach (Toggle t in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Toggle>())
            {
                if (t.gameObject.name == "Editable")
                {
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(5);
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoToggles.Push(t);
                }
            }
        }
        else
        {
            undoredo.First().GetComponent<UndoRedoValues>().goCreated = false;
        }
    }

	void DraggableToggle(bool value){
		foreach (GameObject go in selectable) {
			if (go.GetComponent<Draggable> () && go.GetComponent<Clickable>().isSelected) {
				go.GetComponent<Draggable>().canBeMovedOutOfEditor = value;
                if (!undoredo.First().GetComponent<UndoRedoValues>().goCreated && !undoredo.First().GetComponent<UndoRedoValues>().undoing && !undoredo.First().GetComponent<UndoRedoValues>().redoing && !undoredo.First().GetComponent<UndoRedoValues>().selectionChanged)
                {
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoFocusedObject.Push(go.GetComponent<IDUndoRedo>().id);
                }
            }
        }
        if (!undoredo.First().GetComponent<UndoRedoValues>().goCreated && !undoredo.First().GetComponent<UndoRedoValues>().undoing && !undoredo.First().GetComponent<UndoRedoValues>().redoing && !undoredo.First().GetComponent<UndoRedoValues>().selectionChanged)
        {
            foreach (Toggle t in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Toggle>())
            {
                if (t.gameObject.name == "Draggable")
                {
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(5);
                    undoredo.First().GetComponent<UndoRedoValues>().editorUndoToggles.Push(t);
                }
            }
        }
        else
        {
            undoredo.First().GetComponent<UndoRedoValues>().goCreated = false;
        }
    }

    void InputFieldValueChanged(string value)
    {
        undoredo.First().GetComponent<UndoRedoValues>().inputfieldChanged = true;
    }

    void ActivateEditorMode(){
		gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode = true;
	}

    void SaveOrTryOrValidate()
    {
        foreach (Text t in gameInfo.First().GetComponent<GameInfo>().alertEditorMode.GetComponentsInChildren<Text>())
        {
            if (t.gameObject.transform.parent.gameObject.name == "Continue")
            {
                if(t.text == "Try Level")
                {
                    ActivatePlayMode();
                }
                else if (t.text == "Save")
                {
                    if (File.Exists(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt"))
                    {
                        string text = File.ReadAllText(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt");
                        if (text.Contains("Editor_" + GameInfo.loadedLevelID))
                        {
                            edit = true;
                            gameInfo.First().GetComponent<GameInfo>().alertErase.SetActive(true);
                        }
                        else
                        {
                            SaveLevel(true);
                        }
                    }
                    else
                    {
                        SaveLevel(true);
                    }
                }
                else if (t.text == "Validate")
                {
                    ActivatePlayMode();
                    gameInfo.First().GetComponent<GameInfo>().validating = true;
                }
                gameInfo.First().GetComponent<GameInfo>().alertEditorMode.SetActive(false);
                break;
            }
        }
    }

    void CancelAlert()
    {
        gameInfo.First().GetComponent<GameInfo>().alertEditorMode.SetActive(false);
    }

    void NewSave()
    {
        erase = false;
        gameInfo.First().GetComponent<GameInfo>().alertErase.SetActive(false);
        SaveLevel(edit);
    }

    void Erase()
    {
        erase = true;
        gameInfo.First().GetComponent<GameInfo>().alertErase.SetActive(false);
        SaveLevel(edit);
    }

    void CheckAlertTry()
    {
        bool alert = false;
        foreach(GameObject ff in forceFields)
        {
            if(ff.GetComponent<IsEditable>().isEditable && ff.GetComponent<Draggable>().canBeMovedOutOfEditor)
            {
                alert = true;
                break;
            }
        }
        if (!alert)
        {
            ActivatePlayMode();
        }
        else
        {
            gameInfo.First().GetComponent<GameInfo>().alertEditorMode.SetActive(true);
            foreach(Text t in gameInfo.First().GetComponent<GameInfo>().alertEditorMode.GetComponentsInChildren<Text>())
            {
                if(t.gameObject.transform.parent.gameObject.name == "Continue")
                {
                    t.text = "Try Level";
                    break;
                }
            }
        }
    }

    void CheckAlertValidate()
    {
        bool target = false;
        foreach (Transform child in gameInfo.First().transform)
        {
            if (child.gameObject.tag == "Target")
            {
                target = true;
                break;
            }
        }

        if (target)
        {
            gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.SetActive(false);
            bool alert = false;
            foreach (GameObject ff in forceFields)
            {
                if (ff.GetComponent<IsEditable>().isEditable && ff.GetComponent<Draggable>().canBeMovedOutOfEditor)
                {
                    alert = true;
                    break;
                }
            }
            if (!alert)
            {
                ActivatePlayMode();
                gameInfo.First().GetComponent<GameInfo>().validating = true;
            }
            else
            {
                gameInfo.First().GetComponent<GameInfo>().alertEditorMode.SetActive(true);
                foreach (Text t in gameInfo.First().GetComponent<GameInfo>().alertEditorMode.GetComponentsInChildren<Text>())
                {
                    if (t.gameObject.transform.parent.gameObject.name == "Continue")
                    {
                        t.text = "Validate";
                        break;
                    }
                }
            }
        }
        else
        {
            foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.transform)
            {
                if (child.gameObject.name == "Title")
                {
                    child.gameObject.GetComponent<Text>().text = "A level can't be validated without a target";
                }
            }
            gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.SetActive(true);
        }
    }

    void ActivatePlayMode()
    {
        foreach (GameObject go in draggable)
        {
            go.GetComponent<Draggable>().isDraggable = go.GetComponent<Draggable>().canBeMovedOutOfEditor;
        }
        movingGO.First ().GetComponent<Draggable> ().initialPosition = movingGO.First ().transform.position;
		gameInfo.First ().GetComponent<GameInfo> ().levelEditorMode = false;
		foreach (GameObject go in forceFields) {
			ForceField ff = go.GetComponent<ForceField> ();
			ff.initialDirection = ff.direction;
			ff.initialSizeX = ff.transform.localScale.z;
			ff.initialSizeY = ff.transform.localScale.x;
			go.GetComponent<Mass>().initialValue = go.GetComponent<Mass>().value;
            go.GetComponent<Charge>().initialValue = go.GetComponent<Charge>().value;
            go.GetComponent<Draggable> ().initialPosition = go.transform.position;
		}
		Move mv = movingGO.First ().GetComponent<Move> ();
		mv.initialDirection = mv.direction;
		mv.initialPostion = mv.gameObject.transform.position;
		mv.initialSpeed = mv.speed;
	}

    void CheckAlertSave()
    {
        bool target = false;
        foreach(Transform child in gameInfo.First().transform)
        {
            if(child.gameObject.tag == "Target")
            {
                target = true;
                break;
            }
        }

        if (target)
        {
            gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.SetActive(false); 
            bool alert = false;
            foreach (GameObject ff in forceFields)
            {
                if (ff.GetComponent<IsEditable>().isEditable && ff.GetComponent<Draggable>().canBeMovedOutOfEditor)
                {
                    alert = true;
                    break;
                }
            }
            if (!alert)
            {
                if (File.Exists(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt"))
                {
                    string text = File.ReadAllText(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt");
                    if (text.Contains("Editor_" + GameInfo.loadedLevelID))
                    {
                        edit = true;
                        gameInfo.First().GetComponent<GameInfo>().alertErase.SetActive(true);
                    }
                    else
                    {
                        SaveLevel(true);
                    }
                }
                else
                {
                    SaveLevel(true);
                }
            }
            else
            {
                gameInfo.First().GetComponent<GameInfo>().alertEditorMode.SetActive(true);
                foreach (Text t in gameInfo.First().GetComponent<GameInfo>().alertEditorMode.GetComponentsInChildren<Text>())
                {
                    if (t.gameObject.transform.parent.gameObject.name == "Continue")
                    {
                        t.text = "Save";
                        break;
                    }
                }
            }
        }
        else
        {
            foreach(Transform child in gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.transform)
            {
                if(child.gameObject.name == "Title")
                {
                    child.gameObject.GetComponent<Text>().text = "A level can't be saved without a target";
                }
            }
            gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.SetActive(true);
        }
    }


    void SaveLevel(bool editing){
        LevelData level = new LevelData();
        GameInfo gi = gameInfo.First().GetComponent<GameInfo>();

        level.levelValidated = false;//has to be changed when/if validation is done
        level.nbTarget = 0;
        level.nbCircleObstacle = 0;
        level.nbSquareObstacle = 0;
        level.nbAttractive = 0;
        level.nbRepulsive = 0;
        level.nbUniform = 0;

        foreach(Transform child in gi.gameObject.transform)
        {
            GameObject go = child.gameObject;
            if(go.tag == "ForceField" && !(go.GetComponent<Draggable>().canBeMovedOutOfEditor && go.GetComponent<IsEditable>().isEditable))
            {
                ForceField ff = go.GetComponent<ForceField>();
                if(ff.ffType == 0)
                {
                    level.nbAttractive++;
                }else if (ff.ffType == 1)
                {
                    level.nbRepulsive++;
                }
                else if (ff.ffType == 2)
                {
                    level.nbUniform++;
                }
            }else if(go.tag == "Target")
            {
                level.nbTarget++;
            }else if(go.tag == "Obstacle")
            {
                if (go.name.Contains("Square"))
                {
                    level.nbSquareObstacle++;
                }else if (go.name.Contains("Circle"))
                {
                    level.nbCircleObstacle++;
                }
            }
        }

        level.targetPosx = new float[level.nbTarget];
        level.targetPosy = new float[level.nbTarget];
        level.targetPosz = new float[level.nbTarget];
        level.targetScalex = new float[level.nbTarget];
        level.targetScaley = new float[level.nbTarget];
        level.targetScalez = new float[level.nbTarget];
        level.circleObstaclePosx = new float[level.nbCircleObstacle];
        level.circleObstaclePosy = new float[level.nbCircleObstacle];
        level.circleObstaclePosz = new float[level.nbCircleObstacle];
        level.circleObstacleScalex = new float[level.nbCircleObstacle];
        level.circleObstacleScaley = new float[level.nbCircleObstacle];
        level.circleObstacleScalez = new float[level.nbCircleObstacle];
        level.squareObstaclePosx = new float[level.nbSquareObstacle];
        level.squareObstaclePosy = new float[level.nbSquareObstacle];
        level.squareObstaclePosz = new float[level.nbSquareObstacle];
        level.squareObstacleScalex = new float[level.nbSquareObstacle];
        level.squareObstacleScaley = new float[level.nbSquareObstacle];
        level.squareObstacleScalez = new float[level.nbSquareObstacle];
        level.squareObstacleRotationx = new float[level.nbSquareObstacle];
        level.squareObstacleRotationy = new float[level.nbSquareObstacle];
        level.squareObstacleRotationz = new float[level.nbSquareObstacle];
        level.squareObstacleRotationw = new float[level.nbSquareObstacle];
        level.attractivePosx = new float[level.nbAttractive];
        level.attractivePosy = new float[level.nbAttractive];
        level.attractivePosz = new float[level.nbAttractive];
        level.attractiveScalex = new float[level.nbAttractive];
        level.attractiveScaley = new float[level.nbAttractive];
        level.attractiveScalez = new float[level.nbAttractive];
        level.attractiveValue = new float[level.nbAttractive];
        level.attractiveSize = new float[level.nbAttractive];
        level.attractiveDraggable = new bool[level.nbAttractive];
        level.attractiveEditable = new bool[level.nbAttractive];
        level.repulsivePosx = new float[level.nbRepulsive];
        level.repulsivePosy = new float[level.nbRepulsive];
        level.repulsivePosz = new float[level.nbRepulsive];
        level.repulsiveScalex = new float[level.nbRepulsive];
        level.repulsiveScaley = new float[level.nbRepulsive];
        level.repulsiveScalez = new float[level.nbRepulsive];
        level.repulsiveValue = new float[level.nbRepulsive];
        level.repulsiveSize = new float[level.nbRepulsive];
        level.repulsiveDraggable = new bool[level.nbRepulsive];
        level.repulsiveEditable = new bool[level.nbRepulsive];
        level.uniformPosx = new float[level.nbUniform];
        level.uniformPosy = new float[level.nbUniform];
        level.uniformPosz = new float[level.nbUniform];
        level.uniformRotationx = new float[level.nbUniform];
        level.uniformRotationy = new float[level.nbUniform];
        level.uniformRotationz = new float[level.nbUniform];
        level.uniformRotationw = new float[level.nbUniform];
        level.uniformScalex = new float[level.nbUniform];
        level.uniformScaley = new float[level.nbUniform];
        level.uniformScalez = new float[level.nbUniform];
        level.uniformValue = new float[level.nbUniform];
        level.uniformSizeX = new float[level.nbUniform];
        level.uniformSizeY = new float[level.nbUniform];
        level.uniformDraggable = new bool[level.nbUniform];
        level.uniformEditable = new bool[level.nbUniform];
        
        int countTarget = 0;
        int countCircleObstacle = 0;
        int countSquareObstacle = 0;
        int countAttractive = 0;
        int countRepulsive = 0;
        int countUniform = 0;

        foreach (Transform child in gi.gameObject.transform)
        {
            GameObject go = child.gameObject;
            if (go.tag == "ForceField" && !(go.GetComponent<Draggable>().canBeMovedOutOfEditor && go.GetComponent<IsEditable>().isEditable))
            {
                ForceField ff = go.GetComponent<ForceField>();
                if (ff.ffType == 0)
                {
                    level.attractivePosx[countAttractive] = go.transform.position.x;
                    level.attractivePosy[countAttractive] = go.transform.position.y;
                    level.attractivePosz[countAttractive] = go.transform.position.z;
                    level.attractiveScalex[countAttractive] = go.transform.localScale.x;
                    level.attractiveScaley[countAttractive] = go.transform.localScale.y;
                    level.attractiveScalez[countAttractive] = go.transform.localScale.z;
                    level.attractiveValue[countAttractive] = go.GetComponent<Mass>().value;
                    level.attractiveSize[countAttractive] = ff.transform.localScale.z;
                    level.attractiveDraggable[countAttractive] = go.GetComponent<Draggable>().canBeMovedOutOfEditor;
                    level.attractiveEditable[countAttractive] = go.GetComponent<IsEditable>().isEditable;
                    countAttractive++;
                }
                else if (ff.ffType == 1)
                {
                    level.repulsivePosx[countRepulsive] = go.transform.position.x;
                    level.repulsivePosy[countRepulsive] = go.transform.position.y;
                    level.repulsivePosz[countRepulsive] = go.transform.position.z;
                    level.repulsiveScalex[countRepulsive] = go.transform.localScale.x;
                    level.repulsiveScaley[countRepulsive] = go.transform.localScale.y;
                    level.repulsiveScalez[countRepulsive] = go.transform.localScale.z;
                    level.repulsiveValue[countRepulsive] = go.GetComponent<Charge>().value;
                    level.repulsiveSize[countRepulsive] = ff.transform.localScale.z;
                    level.repulsiveDraggable[countRepulsive] = go.GetComponent<Draggable>().canBeMovedOutOfEditor;
                    level.repulsiveEditable[countRepulsive] = go.GetComponent<IsEditable>().isEditable;
                    countRepulsive++;
                }
                else if (ff.ffType == 2)
                {
                    level.uniformPosx[countUniform] = go.transform.position.x;
                    level.uniformPosy[countUniform] = go.transform.position.y;
                    level.uniformPosz[countUniform] = go.transform.position.z;
                    level.uniformRotationx[countUniform] = go.transform.rotation.x;
                    level.uniformRotationy[countUniform] = go.transform.rotation.y;
                    level.uniformRotationz[countUniform] = go.transform.rotation.z;
                    level.uniformRotationw[countUniform] = go.transform.rotation.w;
                    level.uniformScalex[countUniform] = go.transform.localScale.x;
                    level.uniformScaley[countUniform] = go.transform.localScale.y;
                    level.uniformScalez[countUniform] = go.transform.localScale.z;
                    level.uniformValue[countUniform] = go.GetComponent<Charge>().value;
                    level.uniformSizeX[countUniform] = ff.transform.localScale.z;
                    level.uniformSizeY[countUniform] = ff.transform.localScale.x;
                    level.uniformDraggable[countUniform] = go.GetComponent<Draggable>().canBeMovedOutOfEditor;
                    level.uniformEditable[countUniform] = go.GetComponent<IsEditable>().isEditable;
                    countUniform++;
                }
            }
            else if (go.tag == "Target")
            {
                level.targetPosx[countTarget] = go.transform.position.x;
                level.targetPosy[countTarget] = go.transform.position.y;
                level.targetPosz[countTarget] = go.transform.position.z;
                level.targetScalex[countTarget] = go.transform.localScale.x;
                level.targetScaley[countTarget] = go.transform.localScale.y;
                level.targetScalez[countTarget] = go.transform.localScale.z;
                countTarget++;
            }
            else if (go.tag == "Obstacle")
            {
                if (go.name.Contains("Square"))
                {
                    level.squareObstaclePosx[countSquareObstacle] = go.transform.position.x;
                    level.squareObstaclePosy[countSquareObstacle] = go.transform.position.y;
                    level.squareObstaclePosz[countSquareObstacle] = go.transform.position.z;
                    level.squareObstacleScalex[countSquareObstacle] = go.transform.localScale.x;
                    level.squareObstacleScaley[countSquareObstacle] = go.transform.localScale.y;
                    level.squareObstacleScalez[countSquareObstacle] = go.transform.localScale.z;
                    level.squareObstacleRotationx[countSquareObstacle] = go.transform.rotation.x;
                    level.squareObstacleRotationy[countSquareObstacle] = go.transform.rotation.y;
                    level.squareObstacleRotationz[countSquareObstacle] = go.transform.rotation.z;
                    level.squareObstacleRotationw[countSquareObstacle] = go.transform.rotation.w;
                    countSquareObstacle++;
                }
                else if (go.name.Contains("Circle"))
                {
                    level.circleObstaclePosx[countCircleObstacle] = go.transform.position.x;
                    level.circleObstaclePosy[countCircleObstacle] = go.transform.position.y;
                    level.circleObstaclePosz[countCircleObstacle] = go.transform.position.z;
                    level.circleObstacleScalex[countCircleObstacle] = go.transform.localScale.x;
                    level.circleObstacleScaley[countCircleObstacle] = go.transform.localScale.y;
                    level.circleObstacleScalez[countCircleObstacle] = go.transform.localScale.z;
                    countCircleObstacle++;
                }
            }
        }

        Move mv = movingGO.First().GetComponent<Move>();

        level.ballPosx = mv.gameObject.transform.position.x;
        level.ballPosy = mv.gameObject.transform.position.y;
        level.ballPosz = mv.gameObject.transform.position.z;
        level.ballMass = movingGO.First().GetComponent<Mass>().value;
        level.ballCharge = movingGO.First().GetComponent<Charge>().value;
        level.ballDirectionx = mv.direction.x;
        level.ballDirectiony = mv.direction.y;
        level.ballDirectionz = mv.direction.z;
        level.ballSpeed = mv.speed;
        level.ballEditable = movingGO.First().GetComponent<IsEditable>().isEditable;

        foreach(GameObject ffg in ffGenerator)
        {
            if(ffg.name == "AttractiveCircleFieldGenerator")
            {
                int.TryParse(ffg.GetComponentInChildren<InputField>().text, out level.maxAttractive);
            }else if (ffg.name == "RepulsiveCircleFieldGenerator")
            {
                int.TryParse(ffg.GetComponentInChildren<InputField>().text, out level.maxRepulsive);
            }else if (ffg.name == "UniformFieldGenerator")
            {
                int.TryParse(ffg.GetComponentInChildren<InputField>().text, out level.maxUniform);
            }
        }

        if (erase)
        {
            if (editing)
            {
                //serialize and save level
                BinaryFormatter binary = new BinaryFormatter();
                File.Delete(Application.persistentDataPath + "/Editor/Editor_" + GameInfo.loadedLevelID + ".dat");
                FileStream file = File.Create(Application.persistentDataPath + "/Editor/Editor_" + GameInfo.loadedLevelID + ".dat");
                binary.Serialize(file, level);
                file.Close();
            }
            else
            {
                //serialize and save level
                BinaryFormatter binary = new BinaryFormatter();
                File.Delete(Application.persistentDataPath + "/Level/Level_" + GameInfo.loadedLevelID + ".dat");
                FileStream file = File.Create(Application.persistentDataPath + "/Level/Level_" + GameInfo.loadedLevelID + ".dat");
                binary.Serialize(file, level);
                file.Close();
            }
        }
        else
        {
            if (editing)
            {
                //get number of levels
                int nbLevel = 0;
                if (File.Exists(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt"))
                {
                    string[] lines = File.ReadAllLines(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt");
                    nbLevel = lines.Length;
                }
                else
                {
                    File.Create(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt");
                }
                if (nbLevel == 0)
                {
                    File.AppendAllText(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt", "Editor_" + (nbLevel + 1));
                }
                else
                {
                    File.AppendAllText(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt", "\r\n" + "Editor_" + (nbLevel + 1));
                }

                //serialize and save level
                BinaryFormatter binary = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/Editor/Editor_" + (nbLevel + 1) + ".dat");
                binary.Serialize(file, level);
                file.Close();
            }
            else
            {
                //get number of levels
                int nbLevel = 0;
                if (File.Exists(Application.persistentDataPath + "/Level/Level_Names.txt"))
                {
                    string[] lines = File.ReadAllLines(Application.persistentDataPath + "/Level/Level_Names.txt");
                    nbLevel = lines.Length;
                }
                else
                {
                    File.Create(Application.persistentDataPath + "/Level/Level_Names.txt");
                }
                if (nbLevel == 0)
                {
                    File.AppendAllText(Application.persistentDataPath + "/Level/Level_Names.txt", "Level_" + (nbLevel + 1));
                }
                else
                {
                    File.AppendAllText(Application.persistentDataPath + "/Level/Level_Names.txt", "\r\n" + "Level_" + (nbLevel + 1));
                }

                //serialize and save level
                BinaryFormatter binary = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/Level/Level_" + (nbLevel + 1) + ".dat");
                binary.Serialize(file, level);
                file.Close();
            }
        }

        gameInfo.First().GetComponent<GameInfo>().levelSaved.SetActive(true);
    }

    void HideLevelSaved()
    {
        gameInfo.First().GetComponent<GameInfo>().levelSaved.SetActive(false);
    }

    void HideNoTargetAlert()
    {
        gameInfo.First().GetComponent<GameInfo>().noTargetAlertEditorMode.SetActive(false);
    }

    bool LoadLevel(string name, bool editing)
    {
        if((File.Exists(Application.persistentDataPath+ "/Level/" + name + ".dat") && !editing) || (File.Exists(Application.persistentDataPath + "/Editor/" + name + ".dat") && editing))
        {
            LevelData level = null;

            if (editing)
            {
                //load data from file
                BinaryFormatter binary = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/Editor/" + name + ".dat", FileMode.Open);
                level = (LevelData)binary.Deserialize(file);
                file.Close();
            }
            else
            {
                //load data from file
                BinaryFormatter binary = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/Level/" + name + ".dat", FileMode.Open);
                level = (LevelData)binary.Deserialize(file);
                file.Close();
            }

            //set level with data
            GameInfo gi = gameInfo.First().GetComponent<GameInfo>();
            foreach(GameObject ffg in ffGenerator)
            {
                if(ffg.name == "TargetGenerator")
                {
                    for(int i = 0; i < level.nbTarget; i++)
                    {
                        GameObject t = UnityEngine.Object.Instantiate(ffg.GetComponent<FFNbLimit>().ff);
                        t.transform.parent = gi.gameObject.transform;
                        t.transform.position = new Vector3(level.targetPosx[i], level.targetPosy[i], level.targetPosz[i]);
                        t.transform.localScale = new Vector3(level.targetScalex[i], level.targetScaley[i], level.targetScalez[i]);
                        GameObjectManager.bind(t);
                    }
                }else if (ffg.name == "CircleObstacleGenerator")
                {
                    for (int i = 0; i < level.nbCircleObstacle; i++)
                    {
                        GameObject co = UnityEngine.Object.Instantiate(ffg.GetComponent<FFNbLimit>().ff);
                        co.transform.parent = gi.gameObject.transform;
                        co.transform.position = new Vector3(level.circleObstaclePosx[i], level.circleObstaclePosy[i], level.circleObstaclePosz[i]);
                        co.transform.localScale = new Vector3(level.circleObstacleScalex[i], level.circleObstacleScaley[i], level.circleObstacleScalez[i]);
                        GameObjectManager.bind(co);
                    }
                }
                else if (ffg.name == "SquareObstacleGenerator")
                {
                    for (int i = 0; i < level.nbSquareObstacle; i++)
                    {
                        GameObject so = UnityEngine.Object.Instantiate(ffg.GetComponent<FFNbLimit>().ff);
                        so.transform.parent = gi.gameObject.transform;
                        so.transform.position = new Vector3(level.squareObstaclePosx[i], level.squareObstaclePosy[i], level.squareObstaclePosz[i]);
                        so.transform.localScale = new Vector3(level.squareObstacleScalex[i], level.squareObstacleScaley[i], level.squareObstacleScalez[i]);
                        so.transform.rotation = new Quaternion(level.squareObstacleRotationx[i], level.squareObstacleRotationy[i], level.squareObstacleRotationz[i], level.squareObstacleRotationw[i]);
                        GameObjectManager.bind(so);
                    }
                }
                else if (ffg.name == "AttractiveCircleFieldGenerator")
                {
                    for (int i = 0; i < level.nbAttractive; i++)
                    {
                        GameObject acf = UnityEngine.Object.Instantiate(ffg.GetComponent<FFNbLimit>().ff);
                        acf.transform.parent = gi.gameObject.transform;
                        acf.transform.position = new Vector3(level.attractivePosx[i], level.attractivePosy[i], level.attractivePosz[i]);
                        acf.transform.localScale = new Vector3(level.attractiveScalex[i], level.attractiveScaley[i], level.attractiveScalez[i]);
                        acf.GetComponent<Mass>().value = level.attractiveValue[i];
                        acf.GetComponent<Draggable>().isDraggable = level.attractiveDraggable[i];
                        acf.GetComponent<IsEditable>().isEditable = level.attractiveEditable[i];
                        ffg.GetComponent<FFNbLimit>().max = level.maxAttractive;
                        GameObjectManager.bind(acf);
                        ForceField ff = acf.GetComponent<ForceField>();
                        ff.initialSizeX = ff.transform.localScale.z;
                        ff.initialSizeY = ff.transform.localScale.x;
                        acf.GetComponent<Mass>().initialValue = acf.GetComponent<Mass>().value;
                        ff.initialDirection = ff.direction;
                    }
                }
                else if (ffg.name == "RepulsiveCircleFieldGenerator")
                {
                    for (int i = 0; i < level.nbRepulsive; i++)
                    {
                        GameObject rcf = UnityEngine.Object.Instantiate(ffg.GetComponent<FFNbLimit>().ff);
                        rcf.transform.parent = gi.gameObject.transform;
                        rcf.transform.position = new Vector3(level.repulsivePosx[i], level.repulsivePosy[i], level.repulsivePosz[i]) ;
                        rcf.transform.localScale = new Vector3(level.repulsiveScalex[i], level.repulsiveScaley[i], level.repulsiveScalez[i]);
                        rcf.GetComponent<Charge>().value = level.repulsiveValue[i];
                        rcf.GetComponent<Draggable>().isDraggable = level.repulsiveDraggable[i];
                        rcf.GetComponent<IsEditable>().isEditable = level.repulsiveEditable[i];
                        ffg.GetComponent<FFNbLimit>().max = level.maxRepulsive;
                        GameObjectManager.bind(rcf);
                        ForceField ff = rcf.GetComponent<ForceField>();
                        ff.initialSizeX = ff.transform.localScale.z;
                        ff.initialSizeY = ff.transform.localScale.x;
                        rcf.GetComponent<Charge>().initialValue = rcf.GetComponent<Charge>().value;
                        ff.initialDirection = ff.direction;
                    }
                }
                else if (ffg.name == "UniformFieldGenerator")
                {
                    for (int i = 0; i < level.nbUniform; i++)
                    {
                        GameObject uf = UnityEngine.Object.Instantiate(ffg.GetComponent<FFNbLimit>().ff);
                        uf.transform.parent = gi.gameObject.transform;
                        uf.transform.rotation = new Quaternion(level.uniformRotationx[i], level.uniformRotationy[i], level.uniformRotationz[i], level.uniformRotationw[i]);
                        uf.transform.position = new Vector3(level.uniformPosx[i], level.uniformPosy[i], level.uniformPosz[i]);
                        uf.transform.localScale = new Vector3(level.uniformScalex[i], level.uniformScaley[i], level.uniformScalez[i]);
                        uf.GetComponent<Charge>().value = level.uniformValue[i];
                        uf.GetComponent<Draggable>().isDraggable = level.uniformDraggable[i];
                        uf.GetComponent<IsEditable>().isEditable = level.uniformEditable[i];
                        ffg.GetComponent<FFNbLimit>().max = level.maxUniform;
                        GameObjectManager.bind(uf);
                        ForceField ff = uf.GetComponent<ForceField>();
                        ff.initialSizeX = ff.transform.localScale.z;
                        ff.initialSizeY = ff.transform.localScale.x;
                        uf.GetComponent<Charge>().initialValue = uf.GetComponent<Charge>().value;
                        ff.initialDirection = ff.direction;
                    }
                }
            }

            Move mv = movingGO.First().GetComponent<Move>();
            mv.gameObject.transform.position = new Vector3(level.ballPosx, level.ballPosy, level.ballPosz);
            movingGO.First().GetComponent<Mass>().value = level.ballMass;
            movingGO.First().GetComponent<Charge>().value = level.ballCharge;
            mv.direction = new Vector3(level.ballDirectionx, level.ballDirectiony, level.ballDirectionz);
            mv.speed = level.ballSpeed;
            movingGO.First().GetComponent<IsEditable>().isEditable = level.ballEditable;
            mv.initialPostion = mv.gameObject.transform.position;
            mv.initialSpeed = mv.speed;
            movingGO.First().GetComponent<Mass>().initialValue = movingGO.First().GetComponent<Mass>().value;
            movingGO.First().GetComponent<Charge>().initialValue = movingGO.First().GetComponent<Charge>().value;
            mv.playerSpeed = mv.speed;
            mv.initialDirection = mv.direction;
            mv.playerDirection = mv.direction;
            movingGO.First().GetComponent<Mass>().playerValue = movingGO.First().GetComponent<Mass>().value;
            movingGO.First().GetComponent<Charge>().playerValue = movingGO.First().GetComponent<Charge>().value;

            gi.askResetLevel = true;

            return true;
        }
        //Debug.Log("Level's file doesn't exist.");
        return false;
    }
}

[Serializable]
class LevelData{
	//public bool editorMode;
	public bool levelValidated;
	public int nbTarget;
	public int nbCircleObstacle;
	public int nbSquareObstacle;
	public int nbAttractive;
	public int nbRepulsive;
	public int nbUniform;

	public float[] targetPosx;
    public float[] targetPosy;
    public float[] targetPosz;
    public float[] targetScalex;
    public float[] targetScaley;
    public float[] targetScalez;
    public float[] circleObstaclePosx;
    public float[] circleObstaclePosy;
    public float[] circleObstaclePosz;
    public float[] circleObstacleScalex;
    public float[] circleObstacleScaley;
    public float[] circleObstacleScalez;
    public float[] squareObstaclePosx;
    public float[] squareObstaclePosy;
    public float[] squareObstaclePosz;
    public float[] squareObstacleRotationx;
    public float[] squareObstacleRotationy;
    public float[] squareObstacleRotationz;
    public float[] squareObstacleRotationw;
    public float[] squareObstacleScalex;
    public float[] squareObstacleScaley;
    public float[] squareObstacleScalez;

    public float[] attractivePosx;
    public float[] attractivePosy;
    public float[] attractivePosz;
    public float[] attractiveScalex;
    public float[] attractiveScaley;
    public float[] attractiveScalez;
    public float[] attractiveValue;
	public float[] attractiveSize;
	public bool[] attractiveDraggable;
	public bool[] attractiveEditable;

	public float[] repulsivePosx;
    public float[] repulsivePosy;
    public float[] repulsivePosz;
    public float[] repulsiveScalex;
    public float[] repulsiveScaley;
    public float[] repulsiveScalez;
    public float[] repulsiveValue;
	public float[] repulsiveSize;
	public bool[] repulsiveDraggable;
	public bool[] repulsiveEditable;

	public float[] uniformPosx;
    public float[] uniformPosy;
    public float[] uniformPosz;
    public float[] uniformRotationx;
    public float[] uniformRotationy;
    public float[] uniformRotationz;
    public float[] uniformRotationw;
    public float[] uniformScalex;
    public float[] uniformScaley;
    public float[] uniformScalez;
    public float[] uniformValue;
	public float[] uniformSizeX;
    public float[] uniformSizeY;
    public bool[] uniformDraggable;
	public bool[] uniformEditable;

	public float ballPosx;
    public float ballPosy;
    public float ballPosz;
    public float ballMass;
	public float ballCharge;
	public float ballDirectionx;
    public float ballDirectiony;
    public float ballDirectionz;
    public float ballSpeed;
	public bool ballEditable;

	public int maxAttractive;
	public int maxRepulsive;
	public int maxUniform;
}