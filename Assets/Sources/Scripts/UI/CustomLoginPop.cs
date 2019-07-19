using System.Collections;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using UnityEngine.UI;

public class CustomLoginPop : MonoBehaviour {
    public InputField customID;
    public InputField customPW;
    public Text idGuide;
    public Text pwGuide;
    BackendReturnObject bro;
    bool IsSuccess = false;

    private static CustomLoginPop customLoginPop;

    public static CustomLoginPop Instance()
    {
        if (!customLoginPop)
        {
            customLoginPop = FindObjectOfType(typeof(CustomLoginPop)) as CustomLoginPop;
            if (!customLoginPop)
                Debug.LogWarning("There needs to be one active CustomLoginPop script on a GameObject in your scene.");
        }

        return customLoginPop;
    }

    private void OnEnable()
    {
        idGuide.gameObject.SetActive(false);
        pwGuide.gameObject.SetActive(false);
    }

    public bool CheckIdpwNotNull()
    {
        bool check = true;

        if(string.IsNullOrEmpty(customID.text))
        {
            check = false;
            SetIdGuide("아이디를 입력해주세요.");
        }
        else
        {
            idGuide.gameObject.SetActive(false);
        }

        if (string.IsNullOrEmpty(customPW.text))
        {
            check = false;
            SetPWGuide("비밀번호를 입력해주세요.");
        }
        else
        {
            pwGuide.gameObject.SetActive(false);
        }

        return check;
    }

    private void Update()
    {
        if (IsSuccess)
        {
            Backend.BMember.SaveToken(bro);
            IsSuccess = false;

            // 유저정보 초기화
            BackEndServerManager.instance.OnBackendAuthorized();
        }
    }

    public void CustomLogin() 
    {
        if(CheckIdpwNotNull())
        {
            Backend.BMember.CustomLogin(customID.text, customPW.text, CustomCallback);
        }
	}

    public void CustomSignUp()
    {
        if (CheckIdpwNotNull())
        {
            Backend.BMember.CustomSignUp(customID.text, customPW.text, CustomCallback);
        }
    }

    private void CustomCallback(BackendReturnObject callback)
    {
        Debug.Log(callback);
        if (callback.IsSuccess())
        {
            bro = callback;
            IsSuccess = true;
        }
        else
        {
            //string errCode = callback.GetErrorCode();
            string msg = string.Empty;

            switch(int.Parse(callback.GetStatusCode()))
            {
                case 401:
                    msg = callback.GetMessage().Contains("customId") ? "존재하지 않는 아이디입니다." : "잘못된 비밀번호 입니다.";
                    break;
                case 409:
                    msg = "중복된 아이디입니다.";
                    break;
                case 403: // 차단
                    msg = callback.GetErrorCode();
                    break;
                default:
                    msg = callback.GetMessage();
                    break;
            }

            if(msg.Contains("비밀번호"))
            {
                Dispatcher.Instance.Invoke(() => SetPWGuide(msg));
            }
            else
            {
                Dispatcher.Instance.Invoke(() => SetIdGuide(msg));
            }

        }
    }


    private void SetIdGuide(string msg)
    {
        idGuide.text = msg;
        idGuide.gameObject.SetActive(true);
    }

    private void SetPWGuide(string msg)
    {
        pwGuide.text = msg;
        pwGuide.gameObject.SetActive(true);
    }

}
