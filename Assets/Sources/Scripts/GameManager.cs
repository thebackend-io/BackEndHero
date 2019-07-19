using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {
    #pragma warning disable 0067
    #pragma warning disable 0414
	public static GameManager instance;

	#region VARS
    [Header("Menu UI")]
    public GameObject startMenuUI;
    public GameObject gameOverUI;
    public GameObject hudCanvas;

    public TextMeshPro scoreTxt;
    public Text bestScoreTxt;
	

    [Header("Settings")]
    public static int score;
    public static int bestScore;
    enum Level {one=30, two=50, three=70, four=100};
    Level nowLevel = Level.one;

    public static bool isPlayingGame = false;

    [HideInInspector]
    public int difficultyLevel = 0;
    #endregion

	#region Actions-Events
	public static event Action OnGameOver = delegate {};
    public static event Action OnGameReady = delegate {};
	public static event Action OnGameStart = delegate {};
	public static event Action<float> OnTimeInterValChanged = delegate{};
	#endregion

    // 게임 진동 ON/OFF
    public bool isVibrateOn {get; set;}
    // 레벨 관련
    public float levelSpeed {get;set;}
    public delegate void LevelUp();
    public static LevelUp OnLevelUp;

	void Awake () {
		instance = this;

        Application.targetFrameRate = 60;
    }
	
	void Start () {
        bestScore = 0;
        levelSpeed = 0.8f;
		GameReady ();
        GameSetting.instance.InitializeSetting(); //게임설정 초기화
	}

    public void ResetScore ()
    {
        scoreTxt.text = "0";
    }

    public void AddScore (int _score = 1)
    {
        score += _score;
        // 레벨에 따라 스피드 증가
        if(score >= (int)(nowLevel)) {
            switch(nowLevel) {
                case Level.one:
                    nowLevel = Level.two;
                    levelSpeed += 0.2f;
                    break;
                case Level.two:
                    nowLevel = Level.three;
                    levelSpeed += 0.3f;
                    break;
                case Level.three:
                    nowLevel = Level.four;
                    levelSpeed += 0.4f;
                    break;
                case Level.four:
                default:
                    levelSpeed += 0.05f;
                    break;
            }
            OnLevelUp();
        }
        scoreTxt.text = score.ToString();
    }

    public void UpdateHighScore()
    {
        bestScoreTxt.text = "HIGH SCORE : " + bestScore.ToString();
    }

    public void GameReady () {
        isPlayingGame = false;
        startMenuUI.SetActive (true);
        gameOverUI.SetActive (false);
        OnGameReady ();
        ResetScore();
        scoreTxt.gameObject.SetActive(false);
        levelSpeed = 0.8f;
    }

    public void GameStart () {
        score = 0;
        isPlayingGame = true;
        startMenuUI.SetActive (false);
        gameOverUI.SetActive (false); 
        OnGameStart ();
        scoreTxt.gameObject.SetActive(true);
    }

    public void GameOver () {
        isPlayingGame = false;
        startMenuUI.SetActive (false);
        gameOverUI.SetActive (true);
        OnGameOver ();
        if (score > bestScore) {
            bestScore = score;
            BackEndServerManager.instance.UpdateScore2(score);
        }
        scoreTxt.gameObject.SetActive(false);
    }
}
