using UnityEngine;

public class GameInfo : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    //UI gameobjects references
    public GameObject uiParameters;//ui for force fields
    public GameObject ballParameters;//ui for the ball
    public GameObject uniformRotator;//ui to rotate uniform force fields
    public GameObject distanceDisplayer;//display the distance between 2 selected object or the position of the seleccted object
    public GameObject distanceInGame;
    public GameObject editorUI;//ui only visible in editor mode
    public GameObject gameButtons;//pause, play, reset, go to menu
    public GameObject alertEditorMode;
    public GameObject levelSaved;

    public bool gamePaused = true;
    public bool askResetLevel = false;//if true, the level will be reset
    public bool askResetBall = false;//if true, the ball position will be reset and the ball parameters will be set to the values of the player
    public bool generatorInitialised = false;//true if generators are initialised at the beginning of the level
    public bool uiInitialisation = false;
    public bool levelEditorMode = false;//true in editor mode, false in play mode
    public bool justEnteredEditorMode = true;
    public static bool loadedFromEditor = false;//this is true when the user selected editor mode from menu and false when a created level is opened in play mode (which still opens "LevelEditor" scene)
    public static int loadedLevelID = -1;

    //Texts for the end of a level
    public GameObject levelClearedText;
    public GameObject levelLostText;
    public bool levelCleared = false;
    public bool levelLost = false;

    public int selectedGO = 0;//number of selected gameobjects
	public bool selectedChanged = true; //true when the selected object just changed
	public bool selectedChangedEM = true; //same as selectedChanged but in editor mode
    public int nbSelectableGO = 1;

	public bool objectDragged = false;
    public bool ballDirectionChanging = false;
	public bool mouseOverHUD = false;
	public bool mouseNotOverObject = true;
	public Vector3 cameraPosBeforeDrag;
	public Vector3 mousePosBeforeCameraDrag;
	public bool canMoveCamera = false;
}