using UnityEngine;
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

    public Stack<int> undoFocusedObject;
    public Stack<int> editorUndoFocusedObject;
    public Stack<int> redoFocusedObject;
    public Stack<int> editorRedoFocusedObject;
    public bool theObjectIsSelected;
    public int idCount;
    public bool selectionChanged;

    //values for undo in play mode
    public Stack<int> undoActionTypes;
    public Stack<Slider> undoSliders;
    public Stack<float> undoSliderValues;
    public Stack<int> undoDraggedGO;
    public Stack<Vector3> undoDraggedPositions;
    public Stack<int> undoDeletedTypes;//ff types for ff
    public Stack<int> undoDeletedIDs;
    public Stack<Vector3> undoDeletedPositions;
    public Stack<float> undoDeletedDirections;
    public Stack<float> undoDeletedSizes;
    public Stack<float> undoDeletedValues;
    public Stack<bool> undoDeletedDraggable;
    public Stack<bool> undoDeletedEditable;
    public Stack<int> undoCreatedGO;

    //values for undo in editor mode
    public Stack<int> editorUndoActionTypes;
    public Stack<Slider> editorUndoSliders;
    public Stack<float> editorUndoSliderValues;
    public Stack<int> editorUndoDraggedGO;
    public Stack<Vector3> editorUndoDraggedPositions;
    public Stack<int> editorUndoDeletedTypes;//ff types for ff, -1 for target, -2 for circle obstacle, -3 for square obstacle
    public Stack<int> editorUndoDeletedIDs;
    public Stack<Vector3> editorUndoDeletedPositions;
    public Stack<float> editorUndoDeletedDirections;
    public Stack<float> editorUndoDeletedSizes;
    public Stack<float> editorUndoDeletedValues;
    public Stack<bool> editorUndoDeletedDraggable;
    public Stack<bool> editorUndoDeletedEditable;
    public Stack<int> editorUndoCreatedGO;
    public Stack<int> editorUndoGeneratorTypes;
    public Stack<int> editorUndoGeneratorMax;
    public Stack<Toggle> editorUndoToggles;

    //values for redo in play mode
    public Stack<int> redoActionTypes;
    public Stack<Slider> redoSliders;
    public Stack<float> redoSliderValues;
    public Stack<int> redoDraggedGO;
    public Stack<Vector3> redoDraggedPositions;
    public Stack<int> redoDeletedIDs;
    public Stack<int> redoCreatedGO;
    public Stack<Vector3> redoCreatedGOPositions;
    public Stack<int> redoCreatedGOTypes;//ff types

    //values for redo in editor mode
    public Stack<int> editorRedoActionTypes;
    public Stack<Slider> editorRedoSliders;
    public Stack<float> editorRedoSliderValues;
    public Stack<int> editorRedoDraggedGO;
    public Stack<Vector3> editorRedoDraggedPositions;
    public Stack<int> editorRedoDeletedIDs;
    public Stack<int> editorRedoCreatedGO;
    public Stack<Vector3> editorRedoCreatedGOPositions;
    public Stack<int> editorRedoCreatedGOTypes;//ff types for ff, -1 for target, -2 for circle obstacle, -3 for square obstacle
    public Stack<int> editorRedoGeneratorTypes;
    public Stack<int> editorRedoGeneratorMax;
    public Stack<Toggle> editorRedoToggles;

    public bool undoing;
    public bool redoing;
    public bool inputfieldChanged;
    public Slider slider;
    public Slider slider2;
    public float sliderValue;
    public float sliderValue2;
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
    public float sliderESizeB2;
    public int sliderGO;
    public bool draggedAtCreation;
    public bool goCreated;
    public int maxValue;
}