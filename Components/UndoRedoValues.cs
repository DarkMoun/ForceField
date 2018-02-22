﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UndoRedoValues : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    /* Action types:
     * 0 - a slider changed
     * 1 - an object was dragged
     * 2 - an object was deleted
     * 3 - an object was created
     * 4 - a generator max value was changed
     * 5 - a toggle value was changed
     */

    public Stack<GameObject> focusedObject;
    public Stack<GameObject> editorFocusedObject;
    public bool theObjectIsSelected;

    //values for undo in play mode
    public Stack<int> undoActionTypes;
    public Stack<Slider> undoSliders;
    public Stack<float> undoSliderValues;
    public Stack<GameObject> undoDraggedGO;
    public Stack<Vector3> undoDraggedPositions;
    public Stack<int> undoDeletedTypes;//ff types for ff
    public Stack<Vector3> undoDeletedPositions;
    public Stack<float> undoDeletedDirections;
    public Stack<float> undoDeletedSizes;
    public Stack<float> undoDeletedValues;
    public Stack<GameObject> undoCreatedGO;

    //values for undo in editor mode
    public Stack<int> editorUndoActionTypes;
    public Stack<Slider> editorUndoSliders;
    public Stack<float> editorUndoSliderValues;
    public Stack<GameObject> editorUndoDraggedGO;
    public Stack<Vector3> editorUndoDraggedPositions;
    public Stack<int> editorUndoDeletedTypes;//ff types for ff, -1 for target, -2 for circle obstacle, -3 for square obstacle
    public Stack<Vector3> editorUndoDeletedPositions;
    public Stack<float> editorUndoDeletedDirections;
    public Stack<float> editorUndoDeletedSizes;
    public Stack<float> editorUndoDeletedValues;
    public Stack<GameObject> editorUndoCreatedGO;
    public Stack<int> editorUndoGeneratorTypes;
    public Stack<int> editorUndoGeneratorMax;
    public Stack<Toggle> editorUndoToggles;

    public Slider slider;
    public float sliderValue;
    public bool sliderValueChanged;
    public float sliderFFSize;
    public float sliderFFValue;
    public float sliderFFDirection;
    public float sliderBallDirection;
    public float sliderBallSpeed;
    public float sliderBallMass;
    public float sliderBallCharge;
    public float sliderESizeX;
    public float sliderESizeY;
    public float sliderESizeB;
    public GameObject sliderGO;
    public int maxValue;
}