using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInput : MonoSingleton<MobileInput>
{
    public bool tap, release, hold;
    public Vector2 swipeDelta;

    private Vector2 initialPosition;
    private Player player = null;

    private bool isGameStart = false;
    private bool isTouchEnable = false;

    void OnEnable()
    {
        GameManager.OnGameOver += OnGameOver;
        GameManager.OnGameStart += OnGameStart;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnGameStart -= OnGameStart;
    }

    IEnumerator GameStart()
    {
        // GameStart시 0.5초후 터치 입력 활성화
        yield return new WaitForSeconds(0.5f);
        isTouchEnable = true;
    }

    void OnGameStart()
    {
        StartCoroutine("GameStart");
    }

    void OnGameOver()
    {
        isGameStart = false;
        isTouchEnable = false;
    }

    private void Update()
    {
        if(isGameStart == false && isTouchEnable == true) 
        {
            // 게임시작은 false이고 터치입력이 true 일 때 
            // 게임세작을 true로 활성화 시킴
            if(Input.GetMouseButtonDown(0)) 
            {
                isGameStart = true;
                isTouchEnable = false;
            }
        }

        if(!isGameStart)
        {
            return;
        }

        release = tap = false;
        swipeDelta = Vector2.zero;

        if (Input.GetMouseButtonDown(0))
        {
            if(player)
            {
                initialPosition = player.GetPlayerPos();
                initialPosition = Camera.main.WorldToScreenPoint(initialPosition);
            }
            else
            {
                initialPosition = Input.mousePosition;
            }
            hold = tap = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            release = true;
            hold = false;
            CalcSwipDelta();
        }

        if (hold)
        {
            CalcSwipDelta();
        }  
    }

    private void CalcSwipDelta()
    {
        Vector2 nowPos = (Vector2)Input.mousePosition;
        swipeDelta = (Vector2)nowPos - initialPosition;
        if (nowPos.y < initialPosition.y)
        {
            swipeDelta.y = 0.1f;
        }
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }
}
