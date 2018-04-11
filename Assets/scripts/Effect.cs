using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour {
    Image image;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnEnable()
    {
        image = this.GetComponent<Image>();
        image.color = new Vector4(1,1,1,1);
        StartCoroutine(fade(0));
    }
    IEnumerator fade(int value)
    {

        while (true)
        {
            Color newcolor=image.color;
            newcolor.a -= 0.5f*Time.deltaTime;
            image.color = newcolor;
            if (image.color.a < 0)
            {
                gameObject.SetActive(false);
            }
            yield return null;
        }
    }
    public void OnDisable()
    {
        StopAllCoroutines();
        image.CrossFadeAlpha(1, 0, true);
    }
}
