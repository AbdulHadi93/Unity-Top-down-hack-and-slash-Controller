using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivotScript : MonoBehaviour {

	private GameObject player;
	private RaycastHit hit;

	public Vector3[] CamPosition;
	public Vector3[] CamAngle;
	public GameObject CameraPivot;
	private float transitionTime=2;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
	}

	void Update () {
		if (!RayCastPlayer()) {
			if (CameraPivot.transform.localPosition != CamPosition[1]) {
				CameraPivot.transform.localPosition = Vector3.Lerp (CameraPivot.transform.localPosition,CamPosition[1],Time.deltaTime*transitionTime);
			}
			if (CameraPivot.transform.localEulerAngles != CamAngle[1]) {
				CameraPivot.transform.localEulerAngles = Vector3.Lerp (CameraPivot.transform.localEulerAngles,CamAngle[1],Time.deltaTime*transitionTime);
			}
		}
		else if (RayCastPlayer()) {
			if (CameraPivot.transform.localPosition != CamPosition[0]) {
				CameraPivot.transform.localPosition = Vector3.Lerp (CameraPivot.transform.localPosition,CamPosition[0],Time.deltaTime*transitionTime);
			}
			if (CameraPivot.transform.localEulerAngles != CamAngle[0]) {
				CameraPivot.transform.localEulerAngles = Vector3.Lerp (CameraPivot.transform.localEulerAngles,CamAngle[0],Time.deltaTime*transitionTime);
			}
		}
	}

	bool RayCastPlayer(){
		Vector3 fwd = (player.transform.position - transform.position).normalized;
		if (Physics.Raycast (transform.position, fwd, out hit, 100)) {
			if (hit.transform.CompareTag("Player")) {
				return true;
			} else {
				return false;
			}
		} else {
			return false;
		}
	}

}