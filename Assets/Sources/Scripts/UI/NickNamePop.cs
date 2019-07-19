using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NickNamePop : MonoBehaviour {

    public GameObject inputField;
	
    public Button exitButton;
    public GameObject errMsg;
    private InputField inpFld;
    private Image sideImg;
    private static NickNamePop nicknamePop;

    private bool isDuplicateCheck = false;
    private string tmpNickName = "";
    private Text errMsgText;
    private const string DUPLICATE_MSG = "중복된 닉네임입니다.";
    private const string ERROR_MSG = "알수없는 에러가 발생하였습니다. 잠시 후 다시 시도해주세요.";
    private const string NULLORLONG_MSG = "공백 혹은 너무 긴 닉네임입니다.";
    private const string SUCCESS_MSG = "사용할 수 있는 닉네임입니다.";
    private const string MUST_MSG = "먼저 중복체크를 해주세요.";
    private Color32 NORMAL_COLOR = new Color32(191, 191, 191, 255);
    private Color32 ERROR_COLOR = new Color32(179, 88, 249, 255);
    private Color32 SUCCESS_COLOR = new Color32(0, 0, 0, 255);

    void Awake()
    {
        errMsgText = errMsg.GetComponent<Text>();
        sideImg = inputField.GetComponent<Image>();
        inpFld = inputField.GetComponent<InputField>();
        errMsg.SetActive(false);
    }
    public static NickNamePop Instance()
    {
        if (!nicknamePop)
        {
            nicknamePop = FindObjectOfType(typeof(NickNamePop)) as NickNamePop;
            if (!nicknamePop)
                Debug.LogWarning("There needs to be one active NickNamePop script on a GameObject in your scene.");
        }

        return nicknamePop;
    }


    public void UpdateNickName () 
    {
        if(isDuplicateCheck)
        {
            BackEndServerManager.instance.UpdateNickname (tmpNickName);
            DisableErrMsg();
        }
        else
        {
            EnableErrMsg(MUST_MSG, false);
        }
	}

    public void DuplicateNickNameCheck() 
    {
        isDuplicateCheck = false;
        string tmp = inpFld.text;
        int errCode = BackEndServerManager.instance.DuplicateNickNameCheck(tmp);

        if(errCode == 0) 
        {
            // 중복체크 성공
            tmpNickName = tmp;
            isDuplicateCheck = true;
            EnableErrMsg(SUCCESS_MSG, true);
        }
        else 
        {
            // 중복체크 실패
            if(errCode == 400) 
            {
                EnableErrMsg(NULLORLONG_MSG, false);
            }
            else if(errCode == 409) 
            {
                EnableErrMsg(DUPLICATE_MSG, false);
            }
            else 
            {
                EnableErrMsg(ERROR_MSG, false);
            }
        }
    }

    public void TextFieldChange()
    {
        isDuplicateCheck = false;
        DisableErrMsg();
    }

    public void SetExitButtonActive(bool active)
    {
        exitButton.gameObject.SetActive(active);
    }

	void OnEnable()
	{
		inpFld.text = BackEndServerManager.instance.GetNickName();
	}

	void OnDisable()
	{
		inpFld.text = BackEndServerManager.instance.GetNickName();
        DisableErrMsg();
	}

    void EnableErrMsg(string msg, bool flag)
    {
        errMsg.SetActive(true);
        errMsgText.text = msg;
        if(flag)
        {
            sideImg.color = NORMAL_COLOR;
            errMsgText.color = SUCCESS_COLOR;
        }
        else 
        {
            sideImg.color = ERROR_COLOR;
            errMsgText.color = ERROR_COLOR;
        }   
    }

    void DisableErrMsg()
    {
        errMsg.SetActive(false);
        sideImg.color = NORMAL_COLOR;
    }
}
