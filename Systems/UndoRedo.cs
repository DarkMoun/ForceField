using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;

public class UndoRedo : FSystem
{
    private Family undoredo = FamilyManager.getFamily(new AllOfComponents(typeof(UndoRedoValues)));
    private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
    private Family ffGenerator = FamilyManager.getFamily(new AnyOfTags("FFGenerator"));
    private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));

    private bool editorMode;
    private bool sliderPushDone = true;
    private int nbActions;
    private int nbActionsEditor;
    private GameObject focusedGO;

    public UndoRedo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();

        ur.undoFocusedObject = new Stack<int>();
        ur.editorUndoFocusedObject = new Stack<int>();
        ur.redoFocusedObject = new Stack<int>();
        ur.editorRedoFocusedObject = new Stack<int>();
        ur.undoDeletedIDs = new Stack<int>();
        ur.editorUndoDeletedIDs = new Stack<int>();
        ur.redoDeletedIDs = new Stack<int>();
        ur.editorRedoDeletedIDs = new Stack<int>();

        //values for undo in play mode
        ur.undoActionTypes = new Stack<int>();
        ur.undoSliders = new Stack<Slider>();
        ur.undoSliderValues = new Stack<float>();
        ur.undoDraggedGO = new Stack<int>();
        ur.undoDraggedPositions = new Stack<Vector3>();
        ur.undoDeletedTypes = new Stack<int>();
        ur.undoDeletedPositions = new Stack<Vector3>();
        ur.undoDeletedDirections = new Stack<float>();
        ur.undoDeletedSizes = new Stack<float>();
        ur.undoDeletedValues = new Stack<float>();
        ur.undoDeletedDraggable = new Stack<bool>();
        ur.undoDeletedEditable = new Stack<bool>();
        ur.undoCreatedGO = new Stack<int>();

        //values for undo in editor mode
        ur.editorUndoActionTypes = new Stack<int>();
        ur.editorUndoSliders = new Stack<Slider>();
        ur.editorUndoSliderValues = new Stack<float>();
        ur.editorUndoDraggedGO = new Stack<int>();
        ur.editorUndoDraggedPositions = new Stack<Vector3>();
        ur.editorUndoDeletedTypes = new Stack<int>();
        ur.editorUndoDeletedPositions = new Stack<Vector3>();
        ur.editorUndoDeletedDirections = new Stack<float>();
        ur.editorUndoDeletedSizes = new Stack<float>();
        ur.editorUndoDeletedValues = new Stack<float>();
        ur.editorUndoDeletedDraggable = new Stack<bool>();
        ur.editorUndoDeletedEditable = new Stack<bool>();
        ur.editorUndoCreatedGO = new Stack<int>();
        ur.editorUndoGeneratorTypes = new Stack<int>();
        ur.editorUndoGeneratorMax = new Stack<int>();
        ur.editorUndoToggles = new Stack<Toggle>();

        //values for redo in play mode
        ur.redoActionTypes = new Stack<int>();
        ur.redoSliders = new Stack<Slider>();
        ur.redoSliderValues = new Stack<float>();
        ur.redoDraggedGO = new Stack<int>();
        ur.redoDraggedPositions = new Stack<Vector3>();
        ur.redoCreatedGO = new Stack<int>();
        ur.redoCreatedGOPositions = new Stack<Vector3>();
        ur.redoCreatedGOTypes = new Stack<int>();

        //values for redo in editor mode
        ur.editorRedoActionTypes = new Stack<int>();
        ur.editorRedoSliders = new Stack<Slider>();
        ur.editorRedoSliderValues = new Stack<float>();
        ur.editorRedoDraggedGO = new Stack<int>();
        ur.editorRedoDraggedPositions = new Stack<Vector3>();
        ur.editorRedoCreatedGO = new Stack<int>();
        ur.editorRedoGeneratorTypes = new Stack<int>();
        ur.editorRedoGeneratorMax = new Stack<int>();
        ur.editorRedoToggles = new Stack<Toggle>();
        ur.editorRedoCreatedGOPositions = new Stack<Vector3>();
        ur.editorRedoCreatedGOTypes = new Stack<int>();

        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().gameButtons.transform)
        {
            if(child.gameObject.name == "UndoRedo")
            {
                foreach(Button b in child.gameObject.GetComponentsInChildren<Button>())
                {
                    if(b.gameObject.name == "Undo")
                    {
                        b.onClick.AddListener(Undo);
                    }
                    else if(b.gameObject.name == "Redo")
                    {
                        b.onClick.AddListener(Redo);
                    }
                }
            }
        }

        editorMode = gameInfo.First().GetComponent<GameInfo>().levelEditorMode;
        ur.inputfieldChanged = false;
        ur.theObjectIsSelected = false;
        ur.goCreated = false;
        ur.draggedAtCreation = false;
        ur.undoing = false;
        ur.redoing = false;
        ur.idCount = 1;
        nbActions = ur.undoActionTypes.Count;
        nbActionsEditor = ur.editorUndoActionTypes.Count;
    }

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();

        /*int a = -1;
        int ae = -1;
        if(ur.undoActionTypes.Count != 0)
        {
            a = ur.undoActionTypes.Peek();
        }
        if (ur.editorUndoActionTypes.Count != 0)
        {
            ae = ur.editorUndoActionTypes.Peek();
        }
        Debug.Log("Count: " + ur.undoActionTypes.Count + ": " + a + "   " + "CountE: " + ur.editorUndoActionTypes.Count + ": " + ae);*/

        if (!ur.redoing)
        {
            if (nbActions < ur.undoActionTypes.Count)
            {
                ClearPlayRedo();
            }
            if (nbActionsEditor < ur.editorUndoActionTypes.Count)
            {
                ClearEditorRedo();
            }
        }
        nbActions = ur.undoActionTypes.Count;
        nbActionsEditor = ur.editorUndoActionTypes.Count;

        if (!editorMode && gameInfo.First().GetComponent<GameInfo>().levelEditorMode || gameInfo.First().GetComponent<GameInfo>().askClearPlayUndo)
        {
            ClearPlayUndo();
            gameInfo.First().GetComponent<GameInfo>().askClearPlayUndo = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ur.selectionChanged = gameInfo.First().GetComponent<GameInfo>().selectedChanged;
        }

        if (!ur.undoing && !ur.redoing)
        {
            if ((Input.GetMouseButtonUp(0) || ur.inputfieldChanged) && gameInfo.First().GetComponent<GameInfo>().selectedGO == 1 && ASliderValueIsDifferent() && !ur.selectionChanged && !undoredo.First().GetComponent<UndoRedoValues>().goCreated)
            {
                sliderPushDone = false;
                ur.inputfieldChanged = false;
            }
            else if (undoredo.First().GetComponent<UndoRedoValues>().goCreated)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    undoredo.First().GetComponent<UndoRedoValues>().goCreated = false;
                }
            }
            if (editorMode)
            {
                if (!sliderPushDone)
                {
                    ur.editorUndoActionTypes.Push(0);
                    ur.editorUndoSliders.Push(ur.slider);
                    if(ur.sliderValue2 != -1)
                    {
                        ur.editorUndoSliders.Push(ur.slider2);
                    }
                    ur.editorUndoSliderValues.Push(ur.sliderValue);
                    ur.editorUndoSliderValues.Push(ur.sliderValue2);
                    ur.editorUndoFocusedObject.Push(ur.sliderGO);
                    sliderPushDone = true;
                }
            }
            else
            {
                if (!sliderPushDone)
                {
                    ur.undoActionTypes.Push(0);
                    ur.undoSliders.Push(ur.slider);
                    ur.undoSliderValues.Push(ur.sliderValue);
                    ur.undoFocusedObject.Push(ur.sliderGO);
                    sliderPushDone = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && gameInfo.First().GetComponent<GameInfo>().selectedGO == 1)
        {
            SaveSliderValuesOnSelect();
        }

        if (ur.redoing)
        {
            nbActions = ur.undoActionTypes.Count;
            nbActionsEditor = ur.editorUndoActionTypes.Count;
        }
        ur.undoing = false;
        ur.redoing = false;
        editorMode = gameInfo.First().GetComponent<GameInfo>().levelEditorMode;
    }

    public void SaveSliderValuesOnSelect()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().uiParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "SizeSlider")
            {
                ur.sliderFFSize = uiE.GetComponent<Slider>().value;
            }
            else if (uiE.name == "ValueSlider")
            {
                ur.sliderFFValue = uiE.GetComponent<Slider>().value;
            }
        }
        ur.sliderFFDirection = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponentInChildren<Slider>().value;
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "DirectionSlider")
            {
                ur.sliderBallDirection = uiE.GetComponent<Slider>().value;
            }
            else if (uiE.name == "SpeedSlider")
            {
                ur.sliderBallSpeed = uiE.GetComponent<Slider>().value;
            }
            else if (uiE.name == "MassSlider")
            {
                ur.sliderBallMass = uiE.GetComponent<Slider>().value;
            }
            else if (uiE.name == "ChargeSlider")
            {
                ur.sliderBallCharge = uiE.GetComponent<Slider>().value;
            }
        }
        if (editorMode)
        {
            foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>())
            {
                if(s.gameObject.name == "SizeXSlider")
                {
                    ur.sliderESizeX = s.value;
                    ur.sliderESizeB = s.value;
                }
                else if (s.gameObject.name == "SizeYSlider")
                {
                    ur.sliderESizeY = s.value;
                    ur.sliderESizeB2 = s.value;
                }
            }
        }
        ur.selectionChanged = false;
    }

    public bool ASliderValueIsDifferent()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        if (ur.sliderFFDirection != gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponentInChildren<Slider>().value)
        {
            return true;
        }
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().uiParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "SizeSlider")
            {
                if(ur.sliderFFSize != uiE.GetComponent<Slider>().value)
                {
                    return true;
                }
            }
            else if (uiE.name == "ValueSlider")
            {
                if(ur.sliderFFValue != uiE.GetComponent<Slider>().value){
                    return true;
                }
            }
        }
        foreach (Transform child in gameInfo.First().GetComponent<GameInfo>().ballParameters.transform)
        {
            GameObject uiE = child.gameObject;
            if (uiE.name == "DirectionSlider")
            {
                if(ur.sliderBallDirection != uiE.GetComponent<Slider>().value)
                {
                    return true;
                }
            }
            else if (uiE.name == "SpeedSlider")
            {
                if(ur.sliderBallSpeed != uiE.GetComponent<Slider>().value)
                {
                    return true;
                }
            }
            else if (uiE.name == "MassSlider")
            {
                if(ur.sliderBallMass != uiE.GetComponent<Slider>().value)
                {
                    return true;
                }
            }
            else if (uiE.name == "ChargeSlider")
            {
                if(ur.sliderBallCharge != uiE.GetComponent<Slider>().value)
                {
                    return true;
                }
            }
        }
        if (editorMode)
        {
            foreach (Slider s in gameInfo.First().GetComponent<GameInfo>().editorUI.GetComponentsInChildren<Slider>())
            {
                if (s.gameObject.name == "SizeXSlider")
                {
                    if(ur.sliderESizeX != s.value)
                    {
                        return true;
                    }
                }
                else if (s.gameObject.name == "SizeYSlider")
                {
                    if(ur.sliderESizeY != s.value)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void ClearPlayUndo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        //values for undo in play mode
        ur.undoFocusedObject.Clear();
        ur.undoActionTypes.Clear();
        ur.undoSliders.Clear();
        ur.undoSliderValues.Clear();
        ur.undoDraggedGO.Clear();
        ur.undoDraggedPositions.Clear();
        ur.undoDeletedTypes.Clear();
        ur.undoDeletedPositions.Clear();
        ur.undoDeletedDirections.Clear();
        ur.undoDeletedSizes.Clear();
        ur.undoDeletedValues.Clear();
        ur.undoDeletedIDs.Clear();
        ur.undoCreatedGO.Clear();
    }

    public void ClearPlayRedo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        //values for redo in play mode
        ur.redoFocusedObject.Clear();
        ur.redoActionTypes.Clear();
        ur.redoSliders.Clear();
        ur.redoSliderValues.Clear();
        ur.redoDraggedGO.Clear();
        ur.redoDraggedPositions.Clear();
        ur.redoDeletedIDs.Clear();
        ur.redoCreatedGO.Clear();
        ur.redoCreatedGOPositions.Clear();
        ur.redoCreatedGOTypes.Clear();
    }

    public void ClearEditorRedo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        //values for redo in play mode
        ur.editorRedoFocusedObject.Clear();
        ur.editorRedoActionTypes.Clear();
        ur.editorRedoSliders.Clear();
        ur.editorRedoSliderValues.Clear();
        ur.editorRedoDraggedGO.Clear();
        ur.editorRedoDraggedPositions.Clear();
        ur.editorRedoDeletedIDs.Clear();
        ur.editorRedoCreatedGO.Clear();
        ur.editorRedoCreatedGOPositions.Clear();
        ur.editorRedoCreatedGOTypes.Clear();
        ur.editorRedoGeneratorTypes.Clear();
        ur.editorRedoGeneratorMax.Clear();
        ur.editorRedoToggles.Clear();
        ur.editorRedoDeletedIDs.Clear();
    }

    public void Undo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        ur.undoing = true;

        if ((ur.editorUndoActionTypes.Count != 0 && editorMode) || (ur.undoActionTypes.Count != 0 && !editorMode))
        {
            if (ur.theObjectIsSelected || (editorMode && ur.editorUndoActionTypes.Peek() != 0 && ur.editorUndoActionTypes.Peek() != 5) || (!editorMode && ur.undoActionTypes.Peek() != 0))
            {
                if (editorMode)
                {
                    int action = ur.editorUndoActionTypes.Pop();
                    switch (action)
                    {
                        case 0:
                            ur.editorRedoActionTypes.Push(0);
                            ur.editorRedoFocusedObject.Push(focusedGO.GetComponent<IDUndoRedo>().id);
                            float v2 = ur.editorUndoSliderValues.Pop();
                            float v = ur.editorUndoSliderValues.Pop();
                            if(v2 == -1)
                            {
                                Slider s = ur.editorUndoSliders.Pop();
                                ur.editorRedoSliders.Push(s);
                                ur.editorRedoSliderValues.Push(s.value);
                                ur.editorRedoSliderValues.Push(-1);
                                s.value = v;
                            }
                            else
                            {
                                Slider s2 = ur.editorUndoSliders.Pop();
                                Slider s = ur.editorUndoSliders.Pop();
                                ur.editorRedoSliders.Push(s);
                                ur.editorRedoSliders.Push(s2);
                                ur.editorRedoSliderValues.Push(s.value);
                                ur.editorRedoSliderValues.Push(s2.value);
                                s2.value = v2;
                                s.value = v;
                            }
                            break;

                        case 1:
                            int goID = ur.editorUndoDraggedGO.Pop();
                            ur.editorRedoActionTypes.Push(1);
                            ur.editorRedoDraggedGO.Push(goID);
                            GameObject goDragged = null;
                            foreach(GameObject go in clickableGO)
                            {
                                if(go.GetComponent<IDUndoRedo>().id == goID)
                                {
                                    goDragged = go;
                                }
                            }
                            ur.editorRedoDraggedPositions.Push(goDragged.transform.position);
                            goDragged.transform.position = ur.editorUndoDraggedPositions.Pop();
                            break;

                        case 2:
                            int type = ur.editorUndoDeletedTypes.Pop();
                            int id = ur.editorUndoDeletedIDs.Pop();
                            ur.editorRedoActionTypes.Push(2);
                            ur.editorRedoDeletedIDs.Push(id);
                            switch (type)
                            {
                                case -3:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "SquareObstacleGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new obstacle corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorUndoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float sy = ur.editorUndoDeletedSizes.Pop();
                                            float sx = ur.editorUndoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(sy, ff.transform.localScale.y, sx);
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case -2:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "CircleObstacleGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new obstacle corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorUndoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float sy = ur.editorUndoDeletedSizes.Pop();
                                            float sx = ur.editorUndoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(sy, ff.transform.localScale.y, sx);
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case -1:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "TargetGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new target
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorUndoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float sy = ur.editorUndoDeletedSizes.Pop();
                                            float sx = ur.editorUndoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(sy, ff.transform.localScale.y, sx);
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case 0:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "AttractiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorUndoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float y = ur.editorUndoDeletedSizes.Pop();
                                            float x = ur.editorUndoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(y, ff.transform.localScale.y, x);
                                            ff.GetComponent<Mass>().value = ur.editorUndoDeletedValues.Pop();
                                            ff.GetComponent<Draggable>().canBeMovedOutOfEditor = ur.editorUndoDeletedDraggable.Pop();
                                            ff.GetComponent<IsEditable>().isEditable = ur.editorUndoDeletedEditable.Pop();
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case 1:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "RepulsiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorUndoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float y = ur.editorUndoDeletedSizes.Pop();
                                            float x = ur.editorUndoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(y, ff.transform.localScale.y, x);
                                            ff.GetComponent<Charge>().value = ur.editorUndoDeletedValues.Pop();
                                            ff.GetComponent<Draggable>().canBeMovedOutOfEditor = ur.editorUndoDeletedDraggable.Pop();
                                            ff.GetComponent<IsEditable>().isEditable = ur.editorUndoDeletedEditable.Pop();
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case 2:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "UniformFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorUndoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float y = ur.editorUndoDeletedSizes.Pop();
                                            float x = ur.editorUndoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(y, ff.transform.localScale.y, x);
                                            ff.GetComponent<ForceField>().direction = ur.editorUndoDeletedDirections.Pop();
                                            ff.GetComponent<Charge>().value = ur.editorUndoDeletedValues.Pop();
                                            ff.GetComponent<Draggable>().canBeMovedOutOfEditor = ur.editorUndoDeletedDraggable.Pop();
                                            ff.GetComponent<IsEditable>().isEditable = ur.editorUndoDeletedEditable.Pop();
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case 3:
                            int idCreated= ur.editorUndoCreatedGO.Pop();
                            GameObject goCreated = null;
                            foreach(GameObject go in clickableGO)
                            {
                                if(go.GetComponent<IDUndoRedo>().id == idCreated)
                                {
                                    goCreated = go;
                                    break;
                                }
                            }
                            ur.editorRedoActionTypes.Push(3);
                            ur.editorRedoCreatedGO.Push(idCreated);
                            ur.editorRedoCreatedGOPositions.Push(goCreated.transform.position);
                            if(goCreated.tag == "ForceField")
                            {
                                ur.editorRedoCreatedGOTypes.Push(goCreated.GetComponent<ForceField>().ffType);
                            }
                            else if (goCreated.tag == "Target")
                            {
                                ur.editorRedoCreatedGOTypes.Push(-1);
                            }
                            else if (goCreated.name.Contains("CircleObstacle"))
                            {

                                ur.editorRedoCreatedGOTypes.Push(-2);
                            }
                            else if (goCreated.name.Contains("SquareObstacle"))
                            {
                                ur.editorRedoCreatedGOTypes.Push(-3);
                            }
                            GameObjectManager.unbind(goCreated);
                            GameObject.Destroy(goCreated);
                            break;

                        case 4:
                            int gtype = ur.editorUndoGeneratorTypes.Pop();
                            ur.editorRedoActionTypes.Push(4);
                            ur.editorRedoGeneratorTypes.Push(gtype);
                            foreach (GameObject generator in ffGenerator)
                            {
                                if (generator.GetComponent<FFNbLimit>().ffType == gtype)
                                {
                                    int max;
                                    int.TryParse(generator.GetComponentInChildren<InputField>().text, out max);
                                    ur.editorRedoGeneratorMax.Push(max);
                                    generator.GetComponentInChildren<InputField>().text = ""+ur.editorUndoGeneratorMax.Pop();
                                }
                            }
                            break;

                        case 5:
                            Toggle t = ur.editorUndoToggles.Pop();
                            ur.editorRedoActionTypes.Push(5);
                            ur.editorRedoFocusedObject.Push(focusedGO.GetComponent<IDUndoRedo>().id);
                            ur.editorRedoToggles.Push(t);
                            t.isOn = !t.isOn;
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    int action = ur.undoActionTypes.Pop();
                    switch (action)
                    {
                        case 0:
                            Slider s = ur.undoSliders.Pop();
                            ur.redoActionTypes.Push(0);
                            ur.redoFocusedObject.Push(focusedGO.GetComponent<IDUndoRedo>().id);
                            ur.redoSliders.Push(s);
                            ur.redoSliderValues.Push(s.value);
                            s.value = ur.undoSliderValues.Pop();
                            break;

                        case 1:
                            int goID = ur.undoDraggedGO.Pop();
                            ur.redoActionTypes.Push(1);
                            ur.redoDraggedGO.Push(goID);
                            GameObject goDragged = null;
                            foreach (GameObject go in clickableGO)
                            {
                                if (go.GetComponent<IDUndoRedo>().id == goID)
                                {
                                    goDragged = go;
                                }
                            }
                            ur.redoDraggedPositions.Push(goDragged.transform.position);
                            goDragged.transform.position = ur.undoDraggedPositions.Pop();
                            break;

                        case 2:
                            int type = ur.undoDeletedTypes.Pop();
                            int id = ur.undoDeletedIDs.Pop();
                            ur.redoActionTypes.Push(2);
                            ur.redoDeletedIDs.Push(id);
                            switch (type)
                            {
                                case 0:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "AttractiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.undoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float y = ur.undoDeletedSizes.Pop();
                                            float x = ur.undoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(y, ff.transform.localScale.y, x);
                                            ff.GetComponent<Mass>().value = ur.undoDeletedValues.Pop();
                                            ff.GetComponent<Draggable>().canBeMovedOutOfEditor = ur.undoDeletedDraggable.Pop();
                                            ff.GetComponent<IsEditable>().isEditable = ur.undoDeletedEditable.Pop();
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                            generator.GetComponent<FFNbLimit>().available--;
                                        }
                                    }
                                    break;

                                case 1:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "RepulsiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.undoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float y = ur.undoDeletedSizes.Pop();
                                            float x = ur.undoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(y, ff.transform.localScale.y, x);
                                            ff.GetComponent<Charge>().value = ur.undoDeletedValues.Pop();
                                            ff.GetComponent<Draggable>().canBeMovedOutOfEditor = ur.undoDeletedDraggable.Pop();
                                            ff.GetComponent<IsEditable>().isEditable = ur.undoDeletedEditable.Pop();
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                            generator.GetComponent<FFNbLimit>().available--;
                                        }
                                    }
                                    break;

                                case 2:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "UniformFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.undoDeletedPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            float y = ur.undoDeletedSizes.Pop();
                                            float x = ur.undoDeletedSizes.Pop();
                                            ff.transform.localScale = new Vector3(y, ff.transform.localScale.y, x);
                                            ff.GetComponent<ForceField>().direction = ur.undoDeletedDirections.Pop();
                                            ff.GetComponent<Charge>().value = ur.undoDeletedValues.Pop();
                                            ff.GetComponent<Draggable>().canBeMovedOutOfEditor = ur.undoDeletedDraggable.Pop();
                                            ff.GetComponent<IsEditable>().isEditable = ur.undoDeletedEditable.Pop();
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                            generator.GetComponent<FFNbLimit>().available--;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case 3:
                            int idCreated = ur.undoCreatedGO.Pop();
                            GameObject goCreated = null;
                            foreach (GameObject go in clickableGO)
                            {
                                if (go.GetComponent<IDUndoRedo>().id == idCreated)
                                {
                                    goCreated = go;
                                    break;
                                }
                            }
                            foreach (GameObject generator in ffGenerator)
                            {
                                if (generator.GetComponent<FFNbLimit>().ffType == goCreated.GetComponent<ForceField>().ffType)
                                {
                                    generator.GetComponent<FFNbLimit>().available++;
                                }
                            }
                            ur.redoActionTypes.Push(3);
                            ur.redoCreatedGO.Push(idCreated);
                            ur.redoCreatedGOPositions.Push(goCreated.transform.position);
                            ur.redoCreatedGOTypes.Push(goCreated.GetComponent<ForceField>().ffType);
                            GameObjectManager.unbind(goCreated);
                            GameObject.Destroy(goCreated);
                            break;

                        default:
                            break;
                    }
                }
                ur.theObjectIsSelected = false;
            }
            else
            {
                int goID = -1;
                if (editorMode)
                {
                    goID = ur.editorUndoFocusedObject.Pop();
                }
                else
                {
                    goID = ur.undoFocusedObject.Pop();
                }
                foreach (GameObject g in clickableGO)
                {
                    //unselect all objects different to goID
                    if(g.GetComponent<IDUndoRedo>().id == goID)
                    {
                        g.GetComponent<Clickable>().isSelected = true;
                        gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                        focusedGO = g;
                    }
                    else if (g.GetComponent<Clickable>().isSelected)
                    {
                        g.GetComponent<Clickable>().isSelected = false;
                    }
                }
                ur.theObjectIsSelected = true;
                Undo();
            }
        }
    }

    public void Redo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        ur.redoing = true;

        if ((ur.editorRedoActionTypes.Count != 0 && editorMode) || (ur.redoActionTypes.Count != 0 && !editorMode))
        {
            if (ur.theObjectIsSelected || (editorMode && ur.editorRedoActionTypes.Peek() != 0 && ur.editorRedoActionTypes.Peek() != 5) || (!editorMode && ur.redoActionTypes.Peek() != 0))
            {
                if (editorMode)
                {
                    int action = ur.editorRedoActionTypes.Pop();
                    switch (action)
                    {
                        case 0:
                            ur.editorUndoActionTypes.Push(0);
                            ur.editorUndoFocusedObject.Push(focusedGO.GetComponent<IDUndoRedo>().id);
                            float v2 = ur.editorRedoSliderValues.Pop();
                            float v = ur.editorRedoSliderValues.Pop();
                            if (v2 == -1)
                            {
                                Slider s = ur.editorRedoSliders.Pop();
                                ur.editorUndoSliders.Push(s);
                                ur.editorUndoSliderValues.Push(s.value);
                                ur.editorUndoSliderValues.Push(-1);
                                s.value = v;
                            }
                            else
                            {
                                Slider s2 = ur.editorRedoSliders.Pop();
                                Slider s = ur.editorRedoSliders.Pop();
                                ur.editorUndoSliders.Push(s);
                                ur.editorUndoSliders.Push(s2);
                                ur.editorUndoSliderValues.Push(s.value);
                                ur.editorUndoSliderValues.Push(s2.value);
                                s2.value = v2;
                                s.value = v;
                            }
                            break;

                        case 1:
                            int goID = ur.editorRedoDraggedGO.Pop();
                            ur.editorUndoActionTypes.Push(1);
                            ur.editorUndoDraggedGO.Push(goID);
                            GameObject goDragged = null;
                            foreach (GameObject go in clickableGO)
                            {
                                if (go.GetComponent<IDUndoRedo>().id == goID)
                                {
                                    goDragged = go;
                                }
                            }
                            ur.editorUndoDraggedPositions.Push(goDragged.transform.position);
                            goDragged.transform.position = ur.editorRedoDraggedPositions.Pop();
                            break;

                        case 2:
                            int idDeleted = ur.editorRedoDeletedIDs.Pop();
                            GameObject goDeleted = null;
                            foreach (GameObject go in clickableGO)
                            {
                                if (go.GetComponent<IDUndoRedo>().id == idDeleted)
                                {
                                    goDeleted = go;
                                    break;
                                }
                            }
                            ur.editorUndoActionTypes.Push(2);
                            if (goDeleted.tag == "ForceField")
                            {
                                ur.editorUndoDeletedDirections.Push(goDeleted.GetComponent<ForceField>().direction);
                                ur.editorUndoDeletedPositions.Push(goDeleted.transform.position);
                                ur.editorUndoDeletedSizes.Push(goDeleted.transform.localScale.z);
                                ur.editorUndoDeletedSizes.Push(goDeleted.transform.localScale.x);
                                ur.editorUndoDeletedTypes.Push(goDeleted.GetComponent<ForceField>().ffType);
                                ur.editorUndoDeletedDraggable.Push(goDeleted.GetComponent<Draggable>().canBeMovedOutOfEditor);
                                ur.editorUndoDeletedEditable.Push(goDeleted.GetComponent<IsEditable>().isEditable);
                                if (goDeleted.GetComponent<ForceField>().ffType == 0)
                                {
                                    ur.editorUndoDeletedValues.Push(goDeleted.GetComponent<Mass>().value);
                                }
                                else if (goDeleted.GetComponent<ForceField>().ffType == 1 || goDeleted.GetComponent<ForceField>().ffType == 2)
                                {
                                    ur.editorUndoDeletedValues.Push(goDeleted.GetComponent<Charge>().value);
                                }
                            }
                            else
                            {
                                ur.editorUndoDeletedPositions.Push(goDeleted.transform.position);
                                ur.editorUndoDeletedSizes.Push(goDeleted.transform.localScale.z);
                                ur.editorUndoDeletedSizes.Push(goDeleted.transform.localScale.x);
                                if (goDeleted.tag == "Target")
                                {
                                    ur.editorUndoDeletedTypes.Push(-1);
                                }
                                else if (goDeleted.name.Contains("CircleObstacle"))
                                {

                                    ur.editorUndoDeletedTypes.Push(-2);
                                }
                                else if (goDeleted.name.Contains("SquareObstacle"))
                                {

                                    ur.editorUndoDeletedTypes.Push(-3);
                                }
                            }
                            ur.editorUndoDeletedIDs.Push(goDeleted.GetComponent<IDUndoRedo>().id);
                            GameObjectManager.unbind(goDeleted);
                            GameObject.Destroy(goDeleted);
                            break;

                        case 3:
                            int type = ur.editorRedoCreatedGOTypes.Pop();
                            int id = ur.editorRedoCreatedGO.Pop();
                            ur.editorUndoActionTypes.Push(3);
                            ur.editorUndoCreatedGO.Push(id);
                            switch (type)
                            {
                                case -3:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "SquareObstacleGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new obstacle corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorRedoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case -2:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "CircleObstacleGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new obstacle corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorRedoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case -1:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "TargetGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new target
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorRedoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case 0:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "AttractiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorRedoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case 1:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "RepulsiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorRedoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                case 2:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "UniformFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.editorRedoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                            break;

                        case 4:
                            int gtype = ur.editorRedoGeneratorTypes.Pop();
                            ur.editorUndoActionTypes.Push(4);
                            ur.editorUndoGeneratorTypes.Push(gtype);
                            foreach (GameObject generator in ffGenerator)
                            {
                                if (generator.GetComponent<FFNbLimit>().ffType == gtype)
                                {
                                    int max;
                                    int.TryParse(generator.GetComponentInChildren<InputField>().text, out max);
                                    ur.editorUndoGeneratorMax.Push(max);
                                    generator.GetComponentInChildren<InputField>().text = "" + ur.editorRedoGeneratorMax.Pop();
                                }
                            }
                            break;

                        case 5:
                            Toggle t = ur.editorRedoToggles.Pop();
                            ur.editorUndoActionTypes.Push(5);
                            ur.editorUndoFocusedObject.Push(focusedGO.GetComponent<IDUndoRedo>().id);
                            ur.editorUndoToggles.Push(t);
                            t.isOn = !t.isOn;
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    int action = ur.redoActionTypes.Pop();
                    switch (action)
                    {
                        case 0:
                            Slider s = ur.redoSliders.Pop();
                            ur.undoActionTypes.Push(0);
                            ur.undoFocusedObject.Push(focusedGO.GetComponent<IDUndoRedo>().id);
                            ur.undoSliders.Push(s);
                            ur.undoSliderValues.Push(s.value);
                            s.value = ur.redoSliderValues.Pop();
                            break;

                        case 1:
                            int goID = ur.redoDraggedGO.Pop();
                            ur.undoActionTypes.Push(1);
                            ur.undoDraggedGO.Push(goID);
                            GameObject goDragged = null;
                            foreach (GameObject go in clickableGO)
                            {
                                if (go.GetComponent<IDUndoRedo>().id == goID)
                                {
                                    goDragged = go;
                                }
                            }
                            ur.undoDraggedPositions.Push(goDragged.transform.position);
                            goDragged.transform.position = ur.redoDraggedPositions.Pop();
                            break;

                        case 2:
                            int idDeleted = ur.redoDeletedIDs.Pop();
                            GameObject goDeleted = null;
                            foreach (GameObject go in clickableGO)
                            {
                                if (go.GetComponent<IDUndoRedo>().id == idDeleted)
                                {
                                    goDeleted = go;
                                    break;
                                }
                            }
                            ur.undoActionTypes.Push(2);
                            ur.undoDeletedDirections.Push(goDeleted.GetComponent<ForceField>().direction);
                            ur.undoDeletedPositions.Push(goDeleted.transform.position);
                            ur.undoDeletedSizes.Push(goDeleted.transform.localScale.z);
                            ur.undoDeletedSizes.Push(goDeleted.transform.localScale.x);
                            ur.undoDeletedTypes.Push(goDeleted.GetComponent<ForceField>().ffType);
                            ur.undoDeletedDraggable.Push(goDeleted.GetComponent<Draggable>().canBeMovedOutOfEditor);
                            ur.undoDeletedEditable.Push(goDeleted.GetComponent<IsEditable>().isEditable);
                            if (goDeleted.GetComponent<ForceField>().ffType == 0)
                            {
                                ur.undoDeletedValues.Push(goDeleted.GetComponent<Mass>().value);
                            }
                            else if (goDeleted.GetComponent<ForceField>().ffType == 1 || goDeleted.GetComponent<ForceField>().ffType == 2)
                            {
                                ur.undoDeletedValues.Push(goDeleted.GetComponent<Charge>().value);
                            }
                            ur.undoDeletedIDs.Push(goDeleted.GetComponent<IDUndoRedo>().id);
                            foreach (GameObject generator in ffGenerator)
                            {
                                if (generator.GetComponent<FFNbLimit>().ffType == goDeleted.GetComponent<ForceField>().ffType)
                                {
                                    generator.GetComponent<FFNbLimit>().available++;
                                }
                            }
                            GameObjectManager.unbind(goDeleted);
                            GameObject.Destroy(goDeleted);
                            break;

                        case 3:
                            int type = ur.redoCreatedGOTypes.Pop();
                            int id = ur.redoCreatedGO.Pop();
                            ur.undoActionTypes.Push(3);
                            ur.undoCreatedGO.Push(id);
                            switch (type)
                            {
                                case 0:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "AttractiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.redoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                            generator.GetComponent<FFNbLimit>().available--;
                                        }
                                    }
                                    break;

                                case 1:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "RepulsiveCircleFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.redoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                            generator.GetComponent<FFNbLimit>().available--;
                                        }
                                    }
                                    break;

                                case 2:
                                    foreach (GameObject generator in ffGenerator)
                                    {
                                        if (generator.name == "UniformFieldGenerator")
                                        {
                                            GameObject ff = Object.Instantiate(generator.GetComponent<FFNbLimit>().ff);//create a new force field corresponding to the type
                                            ff.transform.SetParent(gameInfo.First().transform);
                                            GameObjectManager.bind(ff);//bind to FYFY
                                            ff.transform.position = ur.redoCreatedGOPositions.Pop();//set position
                                            foreach (GameObject g in clickableGO)
                                            {
                                                //unselect all objects
                                                if (g.GetComponent<Clickable>().isSelected)
                                                {
                                                    g.GetComponent<Clickable>().isSelected = false;
                                                }
                                            }
                                            ff.GetComponent<Clickable>().isSelected = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                                            gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChangedEM = true;
                                            ff.GetComponent<IDUndoRedo>().id = id;
                                            generator.GetComponent<FFNbLimit>().available--;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }
                ur.theObjectIsSelected = false;
            }
            else
            {
                int goID = -1;
                if (editorMode)
                {
                    goID = ur.editorRedoFocusedObject.Pop();
                }
                else
                {
                    goID = ur.redoFocusedObject.Pop();
                }
                foreach (GameObject g in clickableGO)
                {
                    //unselect all objects different to goID
                    if (g.GetComponent<IDUndoRedo>().id == goID)
                    {
                        g.GetComponent<Clickable>().isSelected = true;
                        gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                        focusedGO = g;
                    }
                    else if (g.GetComponent<Clickable>().isSelected)
                    {
                        g.GetComponent<Clickable>().isSelected = false;
                    }
                }
                ur.theObjectIsSelected = true;
                Redo();
            }
        }
    }
}