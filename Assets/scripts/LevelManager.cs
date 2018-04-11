using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {
    public GameObject[] Levels;
    public static int lvlno;
    public LevelTimes[] levelTimes;
    public float time=0;
    string minutetext,secondtext;
    bool stoptimer = false;
	// Use this for initialization
	void Start () {
        Levels[lvlno].SetActive(true);
        time = levelTimes[lvlno].maxtime;
	}
	
	// Update is called once per frame
	void Update () {
        timer();
	}
    public int checkthirdstar()
    {
        if (time > levelTimes[lvlno].minstartime)
        {
            return 1;
        }
        else
            return 0;
    }
    void timer()
    {
        if (!stoptimer)
        {
            if (time <= 0)
            {
                GuiManager.instance.gameover("time over");
                minutetext = "00";
                secondtext = "00";
                stoptimer = true;
            }
            else
            {
                time -= Time.deltaTime;
                float minutes = Mathf.Floor(time / 60);
                float seconds = Mathf.RoundToInt(time % 60);
                //if (minutes < 10)
                //    minutetext = "0" + minutes.ToString("00");
                //else
                    minutetext = minutes.ToString("00");
                //if (seconds < 10)
                //    secondtext = "0" + seconds.ToString();
                //else
                    secondtext = seconds.ToString("00");
            }
        }
        GuiManager.instance.settext(minutetext,secondtext);
    }
}
[System.Serializable]
public class LevelTimes:System.Object
{
    public float maxtime=0;
    public float minstartime=0;
    public int maxhitpoints = 0;
}
