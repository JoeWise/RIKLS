using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUI_manager : MonoBehaviour {

     public int menuID = 1;
     public GameObject[] menuPanels;
     private GameObject branchPanel;
     private GameObject leafPanel;
     private GameObject playButton;
 
     // Use this for initialization
     void Start () {
        menuPanels = GameObject.FindGameObjectsWithTag("MenuPanel");
 	
 		leafPanel = GameObject.Find("Panel-tree");
        branchPanel = GameObject.Find("Panel-branch");
        playButton = GameObject.Find("Button-play");

        switchToMenu (menuID);
     }
     
     // Update is called once per frame
     void Update () {
 
     }
 
    public void switchToMenu(int menuID) {
 
        foreach(GameObject panel in menuPanels)
        {
            panel.gameObject.SetActive(false);
            Debug.Log (panel.name);
        }
 
        switch (menuID) {
 	    	case 0:
           		branchPanel.gameObject.SetActive(true);
                break;
            case 1:
            	leafPanel.gameObject.SetActive(true);
                break;
        }
    }

    public void changePlayText(){
    	if(playButton.GetComponentInChildren<Text>().text == "Play")
    		playButton.GetComponentInChildren<Text>().text = "Pause";
    	else
    		playButton.GetComponentInChildren<Text>().text = "Play";

    }
 }