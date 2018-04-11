using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cooldown : MonoBehaviour {
	public enum Ability{dash,dodge,slay,spin}
	public Ability SelectedAbility;
	public Image cooldownimage;
	public float totaltime;
	float timeleft;
	Button btn;
	// Use this for initialization
	void OnEnable () {
		timeleft = totaltime;
		btn = GetComponent<Button> ();
		cooldownimage.fillAmount = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (!btn.interactable)
			CooldownImage ();
	}
	void CooldownImage()
	{
		timeleft -= Time.deltaTime;
		cooldownimage.fillAmount = timeleft / totaltime;
		if (timeleft <= 0) {
			btn.interactable = true;
		}
	}
	public void AbIlityPress()
	{
		switch (SelectedAbility) {
		case Ability.dash:
			btn.interactable = false;
			timeleft = totaltime;
			GuiManager.instance.Player.Dash ();
			break;
		case Ability.dodge:
			btn.interactable = false;
			timeleft = totaltime;
			GuiManager.instance.Player.Dodge ();
			break;
		case Ability.slay:
			btn.interactable = false;
			timeleft = totaltime;
			GuiManager.instance.Player.Slay();
			break;
		case Ability.spin:
			btn.interactable = false;
			timeleft = totaltime;
			GuiManager.instance.Player.Spin ();
			break;
		}
	}
}
