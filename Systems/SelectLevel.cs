using FYFY;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class SelectLevel : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family menuInfo = FamilyManager.getFamily(new AllOfComponents(typeof(MenuInfo)));

	public SelectLevel()
    {
        MenuInfo mi = menuInfo.First().GetComponent<MenuInfo>();
        GameInfo.loadedFromEditor = false;//this is true when the user selected editor mode from menu

        if(!Directory.Exists(Application.persistentDataPath + "/Level"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Level");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/Editor"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Editor");
        }

        //add listeners to all buttons in the menu
        foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "MainMenu") {
				foreach (Transform c in child) {
					if (c.gameObject.name == "LevelEditor") {
						c.gameObject.GetComponent<Button> ().onClick.AddListener (OpenEditorMenu);
					} else if (c.gameObject.name == "Play") {
						c.gameObject.GetComponent<Button> ().onClick.AddListener (OpenPlayMenu);
					}
				}
            }
            else if (child.gameObject.name == "Play")
            {
                foreach (Transform c in child)
                {
                    if (c.gameObject.name == "InitialLevels")
                    {//initial levels are levels created from unity editor
                        c.gameObject.GetComponent<Button>().onClick.AddListener(OpenInitialLevels);
                    }
                    else if (c.gameObject.name == "CreatedLevels")
                    {//levels created with the game editor
                        c.gameObject.GetComponent<Button>().onClick.AddListener(OpenCreatedLevels);
                    }
                    else if (c.gameObject.name == "Back")
                    {
                        c.gameObject.GetComponent<Button>().onClick.AddListener(BackFromPlay);
                    }
                }
            }
        }

        if (File.Exists(Application.persistentDataPath + "/Level/Level_Names.txt"))
        {
            string[] lines = File.ReadAllLines(Application.persistentDataPath + "/Level/Level_Names.txt");
            mi.nbCLevel = lines.Length;
        }
        else
        {
            File.Create(Application.persistentDataPath + "/Level/Level_Names.txt");
        }
        if (File.Exists(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt"))
        {
            string[] lines = File.ReadAllLines(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt");
            mi.nbELevel = lines.Length;
        }
        else
        {
            File.Create(Application.persistentDataPath + "/Editor/Editing_Level_Names.txt");
        }

        Transform iLevels = null;
        Transform cLevels = null;
        Transform eLevels = null;
        foreach (Transform child in menuInfo.First().transform)
        {
            //empty gameobjects that will contain all level buttons
            if (child.gameObject.name == "InitialLevels")
            {
                iLevels = child;
            }
            else if (child.gameObject.name == "CreatedLevels")
            {
                cLevels = child;
            }
            else if (child.gameObject.name == "Editor")
            {
                eLevels = child;
            }
        }
        for (int i = 0; i < mi.nbILevel; i++)
        {
            GameObject button = Object.Instantiate(mi.buttonPrefab);
            button.transform.SetParent(iLevels, false);
            button.GetComponent<LevelSelector>().levelID = i + 1;
            button.GetComponentInChildren<Text>().text += button.GetComponent<LevelSelector>().levelID.ToString();//add the level ID to the prefab button text which is "Level "
            button.GetComponent<Button>().onClick.AddListener(delegate {
                GameInfo.loadedFromEditor = false;
                GameInfo.loadedLevelID = button.GetComponent<LevelSelector>().levelID;
                GameObjectManager.loadScene("level" + button.GetComponent<LevelSelector>().levelID.ToString());
            });//add listener on each button to load the level on click
            button.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * i), 0);//position of the button under the previous one
        }
        for (int i = 0; i < mi.nbCLevel; i++)
        {
            GameObject button = Object.Instantiate(mi.buttonPrefab);
            button.transform.SetParent(cLevels, false);
            button.GetComponent<LevelSelector>().levelID = i + 1;
            button.GetComponentInChildren<Text>().text += button.GetComponent<LevelSelector>().levelID.ToString();//add the level ID to the prefab button text which is "Level "
            button.GetComponent<Button>().onClick.AddListener(delegate {
                GameInfo.loadedFromEditor = false;
                GameInfo.loadedLevelID = button.GetComponent<LevelSelector>().levelID;
                GameObjectManager.loadScene("LevelEditor");
            });//add listener on each button to load the level on click
            button.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * i), 0);//position of the button under the previous one
        }
        for (int i = 0; i < mi.nbELevel; i++)
        {
            GameObject button = Object.Instantiate(mi.buttonPrefab);
            button.transform.SetParent(eLevels, false);
            button.GetComponent<LevelSelector>().levelID = i + 1;
            button.GetComponentInChildren<Text>().text = "Editor " + button.GetComponent<LevelSelector>().levelID.ToString();//add the level ID to the prefab button text which is "Level "
            button.GetComponent<Button>().onClick.AddListener(delegate {
                GameInfo.loadedFromEditor = true;
                GameInfo.newLevel = false;
                GameInfo.loadedLevelID = button.GetComponent<LevelSelector>().levelID;
                GameObjectManager.loadScene("LevelEditor");
            });//add listener on each button to load the level on click
            button.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * i), 0);//position of the button under the previous one
        }
        //creation of "Back" buttons
        GameObject b = Object.Instantiate(mi.buttonPrefab);
        b.transform.SetParent(iLevels, false);
        b.GetComponentInChildren<Text>().text = "Back";
        b.GetComponent<Button>().onClick.AddListener(BackFromILevels);
        b.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * mi.nbILevel), 0);
        GameObject b2 = Object.Instantiate(mi.buttonPrefab);
        b2.transform.SetParent(cLevels, false);
        b2.GetComponentInChildren<Text>().text = "Back";
        b2.GetComponent<Button>().onClick.AddListener(BackFromCLevels);
        b2.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * mi.nbCLevel), 0);
        GameObject b3 = Object.Instantiate(mi.buttonPrefab);
        b3.transform.SetParent(eLevels, false);
        b3.GetComponentInChildren<Text>().text = "New Level";
        b3.GetComponent<Button>().onClick.AddListener(OpenEditor);
        b3.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * mi.nbELevel), 0);
        GameObject b4 = Object.Instantiate(mi.buttonPrefab);
        b4.transform.SetParent(eLevels, false);
        b4.GetComponentInChildren<Text>().text = "Back";
        b4.GetComponent<Button>().onClick.AddListener(BackFromEditor);
        b4.transform.position = new Vector3(Screen.width / 2, Screen.height - (35f + 50f * (mi.nbELevel+1)), 0);
    }

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}

	void OpenEditor()
    {
        GameInfo.loadedFromEditor = true;
        GameInfo.newLevel = true;
        GameInfo.loadedLevelID = menuInfo.First().GetComponent<MenuInfo>().nbELevel;
        GameObjectManager.loadScene ("LevelEditor");
	}

	void OpenPlayMenu(){
		foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "MainMenu") {
				child.gameObject.SetActive (false);
			} else if (child.gameObject.name == "Play") {
				child.gameObject.SetActive (true);
			}
		}
	}

    void OpenEditorMenu()
    {
        foreach (Transform child in menuInfo.First().transform)
        {
            if (child.gameObject.name == "MainMenu")
            {
                child.gameObject.SetActive(false);
            }
            else if (child.gameObject.name == "Editor")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    void OpenInitialLevels(){
		foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "Play") {
				child.gameObject.SetActive (false);
			} else if (child.gameObject.name == "InitialLevels") {
				child.gameObject.SetActive (true);
			}
		}
	}

	void OpenCreatedLevels(){
		foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "Play") {
				child.gameObject.SetActive (false);
			} else if (child.gameObject.name == "CreatedLevels") {
				child.gameObject.SetActive (true);
			}
		}
	}

	void BackFromPlay(){
		foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "MainMenu") {
				child.gameObject.SetActive (true);
			} else if (child.gameObject.name == "Play") {
				child.gameObject.SetActive (false);
			}
		}
	}

    void BackFromEditor()
    {
        foreach (Transform child in menuInfo.First().transform)
        {
            if (child.gameObject.name == "MainMenu")
            {
                child.gameObject.SetActive(true);
            }
            else if (child.gameObject.name == "Editor")
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void BackFromILevels(){
		foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "Play") {
				child.gameObject.SetActive (true);
			} else if (child.gameObject.name == "InitialLevels") {
				child.gameObject.SetActive (false);
			}
		}
	}

	void BackFromCLevels(){
		foreach (Transform child in menuInfo.First().transform) {
			if (child.gameObject.name == "Play") {
				child.gameObject.SetActive (true);
			} else if (child.gameObject.name == "CreatedLevels") {
				child.gameObject.SetActive (false);
			}
		}
	}
}