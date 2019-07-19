using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePopManager : MonoBehaviour {
	public static MessagePopManager instance;
	public GameObject popBoard;
	public Text txt;
	
	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		HidePop();
	}

	public void ShowPop(string _str, float _time = 4f)
	{
		txt.text = _str;
		popBoard.SetActive(true);
		CancelInvoke();
		Invoke("HidePop", _time);
	}

	public void HidePop()
	{
		CancelInvoke();
		popBoard.SetActive(false);
	}
}
