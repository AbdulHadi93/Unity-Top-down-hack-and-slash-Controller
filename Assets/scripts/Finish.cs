using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour {
	EnemyMovement[] levelenemies;
	public Objective[] objectives;
	public GameObject enemycontainer;
	public GameObject objectivecontainer;
	bool iscomplete = false;
	int achievedstars = 0;
	public int Totalhitpoints = 0;
	public List<Waves> levelwaves;
	public int waveno = 0;
	// Use this for initialization
	void Start () {
		levelenemies = enemycontainer.GetComponentsInChildren<EnemyMovement>();
		objectives=objectivecontainer.GetComponentsInChildren<Objective>();
	}

	// Update is called once per frame
	void Update () {
		GuiManager.instance.sethittext(Totalhitpoints.ToString());
	}
	public Objective[] getobjective
	{
		get { return objectives; }
	}
	void OnTriggerEnter(Collider hit)
	{
		//if (hit.tag == "playerbone"&&hit as BoxCollider)
		//{
		//    if (iscomplete)
		//    {
		//        GuiManager.instance.levelComplete();
		//        achievedstars += FindObjectOfType<LevelManager>().checkthirdstar();
		//        checkHitpoints();
		//        int currstar = PlayerPrefs.GetInt("Level" + LevelManager.lvlno + "stars");
		//        if (achievedstars > currstar)
		//            PlayerPrefs.SetInt("Level" + LevelManager.lvlno + "stars", achievedstars);
		//        GuiManager.instance.delaygivestars(achievedstars);
		//    }
		//    else if (CheckAllObjectiveCleared() && !CheckAllEnemiesDead())
		//    {
		//        StartCoroutine(GuiManager.instance.showkillall());
		//    }
		//}
		if (hit.tag == "playerbone" && hit as BoxCollider)
		{
			StartCoroutine(enablewave());
//			FindObjectOfType<indicator>().gameObject.SetActive(false);
			GetComponent<Collider>().enabled = false;
		}
	}
	IEnumerator enablewave()
	{
		yield return new WaitForSeconds(2);

		if (waveno < levelwaves.Count)
		{
			StartCoroutine(GuiManager.instance.showwave("Wave "+(1+waveno).ToString()));
			for (int i = 0; i < levelwaves[waveno].enemies.Count; i++)
			{
				levelwaves[waveno].enemies[i].gameObject.SetActive(true);
				levelwaves[waveno].enemies[i].startfight = true;
			}
		}
		else
			GuiManager.instance.levelComplete();
	}
	void checkHitpoints()
	{
		if (Totalhitpoints >= FindObjectOfType<LevelManager>().levelTimes[LevelManager.lvlno].maxhitpoints)
		{
			achievedstars++;
		}
	}
	bool CheckAllEnemiesDead()
	{
		int deadcount = 0;
		for (int i = 0; i < levelenemies.Length; i++)
		{
			if (levelenemies[i].isdead)
				deadcount++;
		}
		if (deadcount == levelenemies.Length)
			return true;
		else
			return false;
	}
	bool CheckAllObjectiveCleared()
	{
		int objectivecount = 0;
		for (int i = 0; i < objectives.Length; i++)
		{
			if (objectives[i].isclear)
				objectivecount++;
		}
		if (objectivecount == objectives.Length)
			return true;
		else
		{

			return false;
		}
	}
	public void CheckLevelCompletion()
	{
		if (CheckAllObjectiveCleared() && !iscomplete && CheckAllEnemiesDead())
		{
			achievedstars++;
			iscomplete = true;
			GetComponent<BoxCollider>().isTrigger = true;
		}
	}
	public void checkwavedead()
	{
		if(waveno<levelwaves.Count)
		{
			for (int i = 0; i < levelwaves[waveno].enemies.Count; i++)
			{

				if (!levelwaves[waveno].enemies[i].isdead)
					return;
			}
			waveno++;
			StartCoroutine(enablewave());
		}
	}
}
[System.Serializable]
public class Waves : System.Object
{
	public List<EnemyMovement> enemies;
}
