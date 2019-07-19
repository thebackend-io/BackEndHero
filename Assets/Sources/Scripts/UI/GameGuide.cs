using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGuide : MonoBehaviour {
	Animator myAnim;

	void Start()
	{		
		GameManager.OnGameReady += OnGameReady;
		Player.OnThrow += OnThrow;
		myAnim = GetComponent<Animator>();
	}

	void OnGameReady ()
	{
		myAnim.SetBool("isShow", true);
	}
	
	void OnThrow ()
	{
		myAnim.SetBool("isShow", false);
	}
}
