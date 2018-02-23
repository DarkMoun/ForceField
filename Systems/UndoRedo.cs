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

    public UndoRedo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();

        ur.focusedObject = new Stack<int>();
        ur.editorFocusedObject = new Stack<int>();
        ur.undoDeletedIDs = new Stack<int>();
        ur.editorUndoDeletedIDs = new Stack<int>();

        //values for undo in play mode
        ur.undoActionTypes = new Stack<int>();
        ur.undoSliders = new Stack<Slider>();
        ur.undoSliderValues = new Stack<float>();
        ur.undoDraggedGO = new Stack<GameObject>();
        ur.undoDraggedPositions = new Stack<Vector3>();
        ur.undoDeletedTypes = new Stack<int>();
        ur.undoDeletedPositions = new Stack<Vector3>();
        ur.undoDeletedDirections = new Stack<float>();
        ur.undoDeletedSizes = new Stack<float>();
        ur.undoDeletedValues = new Stack<float>();
        ur.undoCreatedGO = new Stack<int>();

        //values for undo in editor mode
        ur.editorUndoActionTypes = new Stack<int>();
        ur.editorUndoSliders = new Stack<Slider>();
        ur.editorUndoSliderValues = new Stack<float>();
        ur.editorUndoDraggedGO = new Stack<GameObject>();
        ur.editorUndoDraggedPositions = new Stack<Vector3>();
        ur.editorUndoDeletedTypes = new Stack<int>();
        ur.editorUndoDeletedPositions = new Stack<Vector3>();
        ur.editorUndoDeletedDirections = new Stack<float>();
        ur.editorUndoDeletedSizes = new Stack<float>();
        ur.editorUndoDeletedValues = new Stack<float>();
        ur.editorUndoCreatedGO = new Stack<int>();
        ur.editorUndoGeneratorTypes = new Stack<int>();
        ur.editorUndoGeneratorMax = new Stack<int>();
        ur.editorUndoToggles = new Stack<Toggle>();
        
        foreach(Transform child in gameInfo.First().GetComponent<GameInfo>().gameButtons.transform)
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
                        //b.onClick.AddListener(Redo);
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
        ur.idCount = 1;
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

        if (!editorMode && gameInfo.First().GetComponent<GameInfo>().levelEditorMode || gameInfo.First().GetComponent<GameInfo>().askClearPlayUndo)
        {
            ClearPlayUndo();
            gameInfo.First().GetComponent<GameInfo>().askClearPlayUndo = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ur.selectionChanged = gameInfo.First().GetComponent<GameInfo>().selectedChanged;
        }

        if (!ur.undoing)
        {
            if ((Input.GetMouseButtonUp(0) || ur.inputfieldChanged) && gameInfo.First().GetComponent<GameInfo>().selectedGO == 1 && ASliderValueIsDifferent() && !ur.selectionChanged && !undoredo.First().GetComponent<UndoRedoValues>().goCreated)
            {
                sliderPushDone = false;
                ur.inputfieldChanged = false;
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
                    ur.editorFocusedObject.Push(ur.sliderGO);
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
                    ur.focusedObject.Push(ur.sliderGO);
                    sliderPushDone = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && gameInfo.First().GetComponent<GameInfo>().selectedGO == 1)
        {
            SaveSliderValuesOnSelect();
        }
        
        ur.undoing = false;
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
        ur.focusedObject.Clear();
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
        ur.undoCreatedGO.Clear();
    }

    public void Undo()
    {
        UndoRedoValues ur = undoredo.First().GetComponent<UndoRedoValues>();
        ur.undoing = true;

        if ((ur.editorUndoActionTypes.Count != 0 && editorMode) || (ur.undoActionTypes.Count != 0 && !editorMode))
        {
            if (ur.theObjectIsSelected || (editorMode && ur.editorUndoActionTypes.Peek() != 0 && ur.editorUndoActionTypes.Peek() != 5) || (!editorMode && ur.editorUndoActionTypes.Peek() != 0))
            {
                if (editorMode)
                {
                    int action = ur.editorUndoActionTypes.Pop();
                    switch (action)
                    {
                        case 0:
                            float v2 = ur.editorUndoSliderValues.Pop();
                            float v = ur.editorUndoSliderValues.Pop();
                            if(v2 == -1)
                            {
                                Slider s = ur.editorUndoSliders.Pop();
                                s.value = v;
                            }
                            else
                            {
                                Slider s2 = ur.editorUndoSliders.Pop();
                                Slider s = ur.editorUndoSliders.Pop();
                                s2.value = v2;
                                s.value = v;
                            }
                            break;

                        case 1:
                            GameObject goDragged = ur.editorUndoDraggedGO.Pop();
                            goDragged.transform.position = ur.editorUndoDraggedPositions.Pop();
                            break;

                        case 2:
                            int type = ur.editorUndoDeletedTypes.Pop();
                            int id = ur.editorUndoDeletedIDs.Pop();
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
                                            ff.GetComponent<ForceField>().sizey = ur.editorUndoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().sizex = ur.editorUndoDeletedSizes.Pop();
                                            ff.GetComponent<Mass>().value = ur.editorUndoDeletedValues.Pop();
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
                                            ff.GetComponent<ForceField>().sizey = ur.editorUndoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().sizex = ur.editorUndoDeletedSizes.Pop();
                                            ff.GetComponent<Charge>().value = ur.editorUndoDeletedValues.Pop();
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
                                            ff.GetComponent<ForceField>().sizey = ur.editorUndoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().sizex = ur.editorUndoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().direction = ur.editorUndoDeletedDirections.Pop();
                                            ff.GetComponent<Charge>().value = ur.editorUndoDeletedValues.Pop();
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
                            GameObjectManager.unbind(goCreated);
                            GameObject.Destroy(goCreated);
                            break;

                        case 4:
                            int gtype = ur.editorUndoGeneratorTypes.Pop();
                            foreach (GameObject generator in ffGenerator)
                            {
                                if (generator.GetComponent<FFNbLimit>().ffType == gtype)
                                {
                                    generator.GetComponentInChildren<InputField>().text = ""+ur.editorUndoGeneratorMax.Pop();
                                }
                            }
                            break;

                        case 5:
                            Toggle t = ur.editorUndoToggles.Pop();
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
                            s.value = ur.undoSliderValues.Pop();
                            break;

                        case 1:
                            GameObject goDragged = ur.undoDraggedGO.Pop();
                            goDragged.transform.position = ur.undoDraggedPositions.Pop();
                            break;

                        case 2:
                            int type = ur.undoDeletedTypes.Pop();
                            int id = ur.undoDeletedIDs.Pop();
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
                                            ff.GetComponent<ForceField>().sizey = ur.undoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().sizex = ur.undoDeletedSizes.Pop();
                                            ff.GetComponent<Mass>().value = ur.undoDeletedValues.Pop();
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
                                            ff.GetComponent<ForceField>().sizey = ur.undoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().sizex = ur.undoDeletedSizes.Pop();
                                            ff.GetComponent<Charge>().value = ur.undoDeletedValues.Pop();
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
                                            ff.GetComponent<ForceField>().sizey = ur.undoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().sizex = ur.undoDeletedSizes.Pop();
                                            ff.GetComponent<ForceField>().direction = ur.undoDeletedDirections.Pop();
                                            ff.GetComponent<Charge>().value = ur.undoDeletedValues.Pop();
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
                    goID = ur.editorFocusedObject.Pop();
                }
                else
                {
                    goID = ur.focusedObject.Pop();
                }
                foreach (GameObject g in clickableGO)
                {
                    //unselect all objects different to goID
                    if(g.GetComponent<IDUndoRedo>().id == goID)
                    {
                        g.GetComponent<Clickable>().isSelected = true;
                        gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
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
}