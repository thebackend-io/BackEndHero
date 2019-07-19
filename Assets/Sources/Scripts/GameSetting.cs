using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour {
	public static GameSetting instance = null;
	public ToggleGroup pushGroup;
	public ToggleGroup vibrateGroup;

	// 0 : on, 1 : off
	private Toggle[] pushToggle;
	private Toggle[] vibrateToggle;

	private const int ON_VALUE = 0;
	private const int OFF_VALUE = 1;
	private bool pushValue = false;
	private bool vibrateValue = false;
	
	void Awake()
	{
		if(!instance) {
			instance = this;
		}
	}
	public void InitializeSetting()
	{
		pushToggle = pushGroup.GetComponentsInChildren<Toggle>();
		vibrateToggle = vibrateGroup.GetComponentsInChildren<Toggle>();

		// 게임 시작시 저장된 데이터 불러오기
		Load();

		// 토글의 값이 바뀔 때 값이 바로 변경되게 리스너 추가
		pushToggle[ON_VALUE].onValueChanged.AddListener(value =>
        {
            BackEndServerManager.instance.SetPush(value, true);
			pushValue = value;
			Save();
        });
		vibrateToggle[ON_VALUE].onValueChanged.AddListener(value =>
        {
            GameManager.instance.isVibrateOn = value;
			vibrateValue = value;
			Save();
        });
	}

	int BoolToInt(bool val) {
		if(val) {
			return 1;
		}
		return 0;
	}

	bool IntToBool(int val) {
		if(val>0) {
			return true;
		}
		return false;
	}

	void Save()
	{
		// 현재 데이터 저장
		PlayerPrefs.SetInt("Push", BoolToInt(pushValue));
		PlayerPrefs.SetInt("Vibrate", BoolToInt(vibrateValue));
	}

	void Load()
	{
		// 저장된 데이터가 존재하면 불러오기
		if(!PlayerPrefs.HasKey("Push")) {
			// 저장된 데이터가 없으면 초기화 후 세이브
			BackEndServerManager.instance.SetPush(pushValue, false);
			GameManager.instance.isVibrateOn = vibrateValue;
			Save();
			return;
		}

		pushValue = IntToBool(PlayerPrefs.GetInt("Push"));
		vibrateValue = IntToBool(PlayerPrefs.GetInt("Vibrate"));

		// 불러온 데이터를 토글에 대입
		pushToggle[ON_VALUE].isOn = pushValue;
		pushToggle[OFF_VALUE].isOn = !pushValue;
		BackEndServerManager.instance.SetPush(pushValue, false);

		vibrateToggle[ON_VALUE].isOn = vibrateValue;
		vibrateToggle[OFF_VALUE].isOn = !vibrateValue;
		GameManager.instance.isVibrateOn = vibrateValue;
	}
}
