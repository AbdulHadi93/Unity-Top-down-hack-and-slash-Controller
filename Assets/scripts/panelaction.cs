using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class panelaction : MonoBehaviour {
    public GameObject[] locks;
    public starsimage[] LevelStars;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnEnable()
    {
        disablelocks();
        //displaystars();
    }
    void disablelocks()
    {
        int lvlno = PlayerPrefs.GetInt("Levelno");
        for (int i = 0; i < lvlno; i++)
        {
            locks[i].SetActive(false);
        }
    }
    void displaystars()
    {
        int unlockedlevels = PlayerPrefs.GetInt("Levelno");
        for(int i=0;i<unlockedlevels;i++)
        {
            LevelStars[i].parent.SetActive(true);
            int lvlstars = PlayerPrefs.GetInt("Level" + (i+1) + "stars");
            for (int j = 0; j < lvlstars; j++)
            {

                LevelStars[i].stars[j].SetActive(true);
            }
        }
    }
}
[System.Serializable]
public class starsimage : System.Object
{
    public GameObject[] stars;
    public GameObject parent;
}
