using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlashTextPro : MonoBehaviour {
	public TextMeshProUGUI flashText;
    public TextMeshPro flashText3D;
    public float flashIntervalSec = 0.5f;
    public Color defaultColor = Color.white;
    public Color flashColor = Color.yellow;

    private void OnEnable () {
        CancelInvoke();
        InvokeRepeating("ChangeColor", 0, flashIntervalSec);
	}

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void ChangeColor () {
        if (flashText != null)
        {
            if (flashText.color == flashColor)
            {
                flashText.color = defaultColor;
            }
            else
            {
                flashText.color = flashColor;
            }
        }

        if (flashText3D != null)
        {
            if (flashText3D.color == flashColor)
            {
                flashText3D.color = defaultColor;
            }
            else
            {
                flashText3D.color = flashColor;
            }
        }
    }
}
