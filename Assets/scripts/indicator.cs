using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class indicator : MonoBehaviour {
    public Transform target;
    //public RectTransform CanvasTransform;
    //Finish finish;
    public int i = 0;
    public Transform[] points;
	// Use this for initialization
	void Start () {
        //CanvasTransform = transform.parent.GetComponent<RectTransform>();
        //finish = FindObjectOfType<Finish>();
        target = points[0];
	}
	// Update is called once per frame
    //void Update () {
    //    if (target && target.GetComponent<Objective>())
    //    {
    //        if (!target.GetComponent<Objective>().isclear)
    //        {
    //            Vector3 newpos = target.transform.position;
    //            newpos.y = transform.position.y;
    //            transform.LookAt(newpos);
    //        }
    //        else if (i < finish.getobjective.Length - 1)
    //        {
    //            i++;
    //            target = finish.getobjective[i].gameObject;
    //        }
    //        else
    //            target = null;
    //    }
    //    else
    //    {
    //        Vector3 newpos = finish.transform.position;
    //        newpos.y = transform.position.y;
    //        transform.LookAt(newpos);
    //    }
    //}
    void Update()
    {
        points[i].gameObject.SetActive(true);
        Vector3 newpos = points[i].position;
        newpos.y = transform.position.y;
        transform.LookAt(newpos);
    }
}
