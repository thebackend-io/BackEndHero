using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIGameOver : MonoBehaviour {
    public GameObject guideText;

    [HideInInspector]
    public bool isTouchEnable = false;

	void Update () {
        if (isTouchEnable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameManager.instance.GameReady();
            }
        }
	}

    private void OnEnable ()
    {
        SetTouchDisable();
        CancelInvoke();
        Invoke("SetTouchEnable", 3f);
    }

    private void OnDisable ()
    {
        CancelInvoke();
        SetTouchDisable();
    }

    private void SetTouchEnable ()
    {
        isTouchEnable = true;
        if (!guideText.activeSelf) {
            guideText.SetActive(true);
        }
    }

    private void SetTouchDisable()
    {
        isTouchEnable = false;
        if (guideText.activeSelf) {
            guideText.SetActive(false);
        }
    }
}
