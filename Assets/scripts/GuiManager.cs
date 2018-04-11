using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuiManager : MonoBehaviour {
    public Image fadepanel;
    public GameObject completepanel,gameplaypanel,gameoverpanel,nextbtn,killall;
    public static GuiManager instance;
    TPlayerMovement player;
    public Image soundbtn;
    public Sprite soundon, soundoff;
    public GameObject[] stars;
    public Text minutetext, secondtext,hittex;
    public Sprite[] failedsprites;
    public Text wavetext;
	// Use this for initialization
	void Start () {
        instance = this;
        DontDestroyOnLoad(instance);
        SceneManager.LoadScene("menu");
        PlayerPrefs.SetInt("Levelno", 6);
        if (!PlayerPrefs.HasKey("Levelno"))
        {
            PlayerPrefs.SetInt("Levelno",1);
            PlayerPrefs.SetInt("sound",1);
        }
        checksoundstate();
	}
    public IEnumerator showwave(string s)
    {
        wavetext.text = s;
        wavetext.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        wavetext.gameObject.SetActive(false);
    }
    public void sethittext(string t)
    {
        hittex.text = t;
    }
    public IEnumerator showkillall()
    {
        killall.SetActive(true);
        yield return new WaitForSeconds(5);
        killall.SetActive(false);
    }
    void checksoundstate()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            AudioListener.pause = false;
            soundbtn.sprite = soundon;
        }
        else if (PlayerPrefs.GetInt("sound") == 0)
        {
            AudioListener.pause = true;
            soundbtn.sprite = soundoff;
        }
    }
    public void togglesound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            AudioListener.pause = true;
            PlayerPrefs.SetInt("sound", 0);
            soundbtn.sprite = soundoff;
        }
        else if (PlayerPrefs.GetInt("sound") == 0)
        {
            AudioListener.pause = false;
            PlayerPrefs.SetInt("sound", 1);
            soundbtn.sprite = soundon;
        }
    }
    public TPlayerMovement Player
    {
        set { player = value; }
        get { return player; }
    }
	// Update is called once per frame
	void Update () {
		
	}

    public void loadscene(string scene)
    {
        Disablestars();
        SceneManager.LoadScene(scene);
        Time.timeScale = 1;
    }
    public void play(int lvl)
    {
        Time.timeScale = 1;
        LevelManager.lvlno = lvl;
        SceneManager.LoadScene("gameplay");
        fadepanel.gameObject.SetActive(true);
    }
    public void DisablePanel(GameObject panel)
    {
        if (panel.name == "Pause screen")
            Time.timeScale = 1;
        panel.SetActive(false);
    }
    public void EnablePanel(GameObject panel)
    {
        if (panel.name == "Pause screen")
            Time.timeScale = 0;
        panel.SetActive(true);
    }
    public void Attack()
    {
        player.ComboCounter();
    }
    public void Dodge()
    {
        player.Dodge();
    }
	public void Dash()
	{
		player.Dash();
	}
	public void Slay()
	{
		player.Slay();
	}
	public void Spin()
	{
		player.Spin();
	}
    public void levelComplete()
    {
        gameplaypanel.SetActive(false);
        completepanel.SetActive(true);
        if (LevelManager.lvlno < 6)
        {
            if (PlayerPrefs.GetInt("Levelno") == LevelManager.lvlno)
            {
                int temp = LevelManager.lvlno;
                temp++;
                PlayerPrefs.SetInt("Levelno", temp);
            }
        }
        else
            nextbtn.SetActive(false);
    }
    public void delaygivestars(int count)
    {
        StartCoroutine(enablestars(count));
    }
    IEnumerator enablestars(int count)
    {
        for (int i = 0; i < count; i++)
        {
            stars[i].SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }
        Time.timeScale = 0;
    }

    public void Disablestars()
    {
        StopAllCoroutines();
        for (int i = 0; i < 3; i++)
        {
            stars[i].SetActive(false);
        }
    }
    public void NextLevel()
    {
        Disablestars();
            int temp = LevelManager.lvlno;
            temp++;
            LevelManager.lvlno = temp;
            loadscene("gameplay");
    }
    public void gameover(string type)
    {
        StartCoroutine(delaygameover(type));
    }
    IEnumerator delaygameover(string type)
    {
        yield return new WaitForSeconds(2);
        if (type == "dead")
            gameoverpanel.GetComponent<Image>().sprite = failedsprites[0];
        else if (type == "time over")
            gameoverpanel.GetComponent<Image>().sprite = failedsprites[1];
        gameplaypanel.SetActive(false);
        Time.timeScale = 0;
        gameoverpanel.SetActive(true);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void settext(string min,string sec)
    {
        minutetext.text = min;
        secondtext.text = sec;
    }
}
