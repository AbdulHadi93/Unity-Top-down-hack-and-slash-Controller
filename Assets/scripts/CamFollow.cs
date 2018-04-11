using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour {
	
    public Transform Player;
    public Vector3 offset;
    private Vector3 defaultoffset;
    List<Renderer> objects;
    public float distance=2;
	// Use this for initialization
	void Start () {
		//Time.timeScale = 0.25f;
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        defaultoffset = offset;
	}
    void LateUpdate()
    {
        //checkRay();
        //transform.position = Player.position;
        //transform.position -= transform.forward * distance;

        Vector3 newpos = Player.position + offset;
        transform.position = Vector3.Lerp(transform.position, newpos, 3 * Time.deltaTime);
        //transform.LookAt(Player.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.LookRotation(Player.transform.position - transform.position)), Time.deltaTime * 2);
        
    }
   /* void checkRay()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint (Player.transform.position+new Vector3(0,1f,0));
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
           if (hit.collider.tag != "Player")
            {
                distance--;
            }
        }
        Debug.DrawRay (ray.origin, ray.direction *  50, Color.yellow);
    }*/
}