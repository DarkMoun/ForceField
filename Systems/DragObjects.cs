using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class DragObjects : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family draggable = FamilyManager.getFamily(new AllOfComponents(typeof(Draggable), typeof(Clickable)));
	private Family gameInfo = FamilyManager.getFamily(new AllOfComponents(typeof(GameInfo)));
    private Family clickableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Clickable), typeof(PointerSensitive)));
    private Family undoredo = FamilyManager.getFamily(new AllOfComponents(typeof(UndoRedoValues)));

    protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
        if (!gameInfo.First().GetComponent<GameInfo>().playing)
        {
            bool mouseOverHUD = gameInfo.First().GetComponent<GameInfo>().mouseOverHUD;
            foreach (GameObject go in draggable)
            {
                PointerOver po = go.GetComponent<PointerOver>();
                if (po && !mouseOverHUD)
                {//if mouse over the object and not over ui
                    if (Input.GetMouseButtonDown(0))
                    {//if mouse's left button just got pressed
                     //set the go as "dragged"
                        go.GetComponent<Draggable>().dragged = true;//the go is currently dragged
                        gameInfo.First().GetComponent<GameInfo>().objectDragged = true;//an object is dragged
                        go.GetComponent<Draggable>().positionBeforeDrag = go.transform.position;//store the current go position as position before drag
                        go.GetComponent<Draggable>().fromMouseToCenter = go.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f));//vector from mouse position to screen center before drag (positions in the world)
                    }
                }
            }
            if (!Input.GetMouseButton(0) && gameInfo.First().GetComponent<GameInfo>().objectDragged)
            {//if the left click is released and an object is dragged
                foreach (GameObject go in draggable)
                {
                    if (go.GetComponent<Draggable>().dragged)
                    {//find the dragged object
                     //set go to not dragged
                        go.GetComponent<Draggable>().dragged = false;
                        gameInfo.First().GetComponent<GameInfo>().objectDragged = false;
                        gameInfo.First().gameObject.GetComponent<GameInfo>().selectedChanged = true;
                        go.GetComponent<Draggable>().canBeMoved = false;
                        Color uipColor = gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color;
                        Color uipdColor = gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color;
                        Color bpColor = gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color;
                        Color urColor = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color;
                        gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color = new Color(uipColor.r, uipColor.g, uipColor.b, 1);     //show uiP
                        gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color = new Color(uipdColor.r, uipdColor.g, uipdColor.b, 1);     //show uiP delete button
                        gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color = new Color(bpColor.r, bpColor.g, bpColor.b, 1);   //show bP
                        gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color = new Color(urColor.r, urColor.g, urColor.b, 1);   //show ur
                    }
                }
            }
            if (gameInfo.First().GetComponent<GameInfo>().objectDragged)
            {//if an object is dragged
                foreach (GameObject go in draggable)
                {
                    Draggable d = go.GetComponent<Draggable>();
                    if (d.isDraggable && d.dragged)
                    {
                        Color uipColor = gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color;
                        Color uipdColor = gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color;
                        Color bpColor = gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color;
                        Color urColor = gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color;
                        gameInfo.First().GetComponent<GameInfo>().uiParameters.GetComponent<Image>().color = new Color(uipColor.r, uipColor.g, uipColor.b, 75f / 255);     //hide uiP
                        gameInfo.First().GetComponent<GameInfo>().uipDelete.GetComponent<Image>().color = new Color(uipdColor.r, uipdColor.g, uipdColor.b, 75f / 255);     //hide uiP delete button
                        gameInfo.First().GetComponent<GameInfo>().ballParameters.GetComponent<Image>().color = new Color(bpColor.r, bpColor.g, bpColor.b, 75f / 255);   //hide bP
                        gameInfo.First().GetComponent<GameInfo>().uniformRotator.GetComponent<Image>().color = new Color(urColor.r, urColor.g, urColor.b, 75f / 255);   //hide ur
                        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y - 0.5f)) + d.fromMouseToCenter;//new position of the object
                        if (!d.canBeMoved)
                        {
                            if ((newPos - d.positionBeforeDrag).magnitude > 1)
                            {
                                /* the object can be moved only if the distance between the position before 
                                 * the drag and the current position of the mouse is bigger than a certain value
                                 * this way the object won't be dragged with a miss click */
                                d.canBeMoved = true;

                                if (undoredo.First().GetComponent<UndoRedoValues>().draggedAtCreation)
                                {
                                    undoredo.First().GetComponent<UndoRedoValues>().draggedAtCreation = false;
                                }
                                else
                                {
                                    if (gameInfo.First().GetComponent<GameInfo>().levelEditorMode)
                                    {
                                        undoredo.First().GetComponent<UndoRedoValues>().editorUndoActionTypes.Push(1);
                                        undoredo.First().GetComponent<UndoRedoValues>().editorUndoDraggedGO.Push(go.GetComponent<IDUndoRedo>().id);
                                        undoredo.First().GetComponent<UndoRedoValues>().editorUndoDraggedPositions.Push(go.transform.position);
                                    }
                                    else
                                    {
                                        undoredo.First().GetComponent<UndoRedoValues>().undoActionTypes.Push(1);
                                        undoredo.First().GetComponent<UndoRedoValues>().undoDraggedGO.Push(go.GetComponent<IDUndoRedo>().id);
                                        undoredo.First().GetComponent<UndoRedoValues>().undoDraggedPositions.Push(go.transform.position);
                                    }
                                }
                            }
                        }
                        if (d.canBeMoved)
                        {
                            go.transform.position = newPos;//move the object to the new position
                            if (gameInfo.First().GetComponent<GameInfo>().selectedGO > gameInfo.First().GetComponent<GameInfo>().nbSelectableGO)
                            {
                                foreach (GameObject g in clickableGO)
                                {
                                    //unselect all objects
                                    if (g.GetComponent<Clickable>().isSelected)
                                    {
                                        g.GetComponent<Clickable>().isSelected = false;
                                    }
                                }
                            }
                            go.GetComponent<Clickable>().isSelected = true;//select the moved object
                        }
                    }
                }
            }
        }
	}
}