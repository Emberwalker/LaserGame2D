using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Derived from https://xinyustudio.wordpress.com/2015/08/06/unity3d-progressbar-using-new-ui-system/
public class ProgressBar : MonoBehaviour {

	Image foregroundImage;

	public void SetProgress(float progress) {
		Debug.Log ("BAR: " + foregroundImage + "/" + progress);
		if (foregroundImage != null) foregroundImage.fillAmount = progress/100f;
	}

	void Start () {
		foregroundImage = gameObject.GetComponent<Image> ();		
	}
}
