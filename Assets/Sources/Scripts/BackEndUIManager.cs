using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackEndUIManager : MonoBehaviour
{
    public static BackEndUIManager instance;

    // ui 오브젝트 -----------------
    public GameObject loginUI;
    public GameObject customLoginUI;
    public GameObject nickNameUI;
    public GameObject noticeListUI;
    public GameObject noticeUI;
    public GameObject rankUI;
    public GameObject privacyUI;
    public GameObject updateUI;
    public GameObject chatUI;
    public GameObject userListUI;
    public GameObject reportUI;
    public Text reportUserName;

    public GameObject processingCoverUI;
    public GameObject btnGroupUI;
    public GameObject btnRemoveAds;
    public GameObject cmdUI;
    public GameObject gameCloseUI;
    public GameObject gameSettingUI;

    public GameObject baseChatFormat;
    public GameObject chatList;
    public InputField inputField;

    public GameObject baseNameFormat;
    public GameObject userList;
    public Text versionInfo;
    public GameObject googleLogin;
    // -----------------------------

    public List<GameObject> gameObjects = new List<GameObject>();

    private ToggleGroup reportReason;
    private InputField reportDetailReason;

    private const string CHAT_NICK = "[{0}]";
    private string clickNickName = "";

    private Color32 infoTextColor = new Color32(246, 94, 94, 255);
    private Color32 whisperColor = new Color32(144, 151, 255, 255);
    private const int LIMIT_NUM_CHAT = 100;


    void Awake()
    {
        instance = this;
    }
    void Start()
    {

        GameManager.OnGameReady += OnGameReady;
        GameManager.OnGameStart += OnGameStart;

        try
        {
            gameObjects.Add(loginUI);
            gameObjects.Add(customLoginUI);
            gameObjects.Add(nickNameUI);
            gameObjects.Add(noticeListUI);
            gameObjects.Add(noticeUI);
            gameObjects.Add(rankUI);
            gameObjects.Add(privacyUI);
            gameObjects.Add(updateUI);
            gameObjects.Add(chatUI);
            gameObjects.Add(reportUI);
            gameObjects.Add(gameCloseUI);
            gameObjects.Add(gameSettingUI);

            // 신고 UI 관련
            reportReason = reportUI.GetComponentInChildren<ToggleGroup>();
            reportDetailReason = reportUI.GetComponentInChildren<InputField>();

#if UNITY_IOS
            googleLogin.SetActive(false);
#endif
            // 버전 정보
            versionInfo.text = "Ver." + Application.version;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if(Input.GetKey(KeyCode.Escape))
        {
            gameCloseUI.SetActive(true);
        }
#endif
    }

    void OnApplicationQuit()
    {
        // 게임 종료 시 객체 삭제
        ClearChat();
        ClearUserList();
    }

    void OnGameReady()
    {
        btnGroupUI.SetActive(true);
        versionInfo.gameObject.SetActive(true);
    }

    void OnGameStart()
    {
        btnGroupUI.SetActive(false);
        versionInfo.gameObject.SetActive(false);
    }

    // ==================================================
    public void SetProcessing(bool _ing = true)
    {
        processingCoverUI.SetActive(_ing);
    }

    // ==================================================
    // 모든 뒤끝 UI 닫기
    public void CloseAll()
    {
        foreach (GameObject @object in gameObjects)
        {
            @object.SetActive(false);
        }
    }

    // 로그인 UI 표시
    public void ShowLoginUI()
    {
        CloseUI(privacyUI);
        ShowUI(loginUI);
    }
    // 커스텀 로그인 UI 표시
    public void ShowCustomLoginUI()
    {
        ShowUI(customLoginUI);
    }
    // 닉네임 변경 UI 표시
    public void ShowNickNameUI(bool active)
    {
        ShowUI(nickNameUI);
        NickNamePop.Instance().SetExitButtonActive(active);
    }
    // 공지사항 리스트 UI 표시
    public void ShowNoticeListUI()
    {
        ShowUI(noticeListUI);
    }
    // 공지사항 닫기
    public void CloseNoticeUI()
    {
        CloseUI(noticeUI);
    }
    // 랭크 UI 표시 
    public void ShowRankUI()
    {
        ShowUI(rankUI);
    }
    // 개인정보 UI 표시
    public void ShowPrivacyUI()
    {
        ShowUI(privacyUI);
    }
    // 업데이트 UI 표시
    public void ShowUpdateUI(bool active)
    {
        updateUI.GetComponentInChildren<Button>().gameObject.SetActive(active);
        ShowUI(updateUI);
    }
    // 설정 UI 표시
    public void ShowSettingUI()
    {
        gameSettingUI.SetActive(true);
    }

    #region 채팅관련 UI
    /*
     * 채팅 UI 열기/닫기
     * InputField에 있는 텍스트를 BackEndChatManager로 전달
     * 클라이언트에 저장된 모든 채팅기록 삭제
     * cmd 패널 열기/닫기
     * 유저 리스트 UI 열기/닫기
     * 유저 신고
     */

    //채팅 UI 표시
    public void ShowChatUI()
    {
        ShowUI(chatUI);
        // 채팅창을 켤 때 위치를 최하단으로 초기화
        Vector3 nowPos = chatList.GetComponent<RectTransform>().localPosition;
        nowPos.y = 0;
        chatList.GetComponent<RectTransform>().localPosition = nowPos;
        
        //채팅 UI에 포함된 하위 UI 모두 닫기
        CloseUserList();
        CloseCmdPanal();
        CloseReportUI();
    }
    // 채팅 UI 닫기
    public void CloseChatUI()
    {
        // 채팅 UI에 포함된 하위 UI 포함 모든 채팅 UI 닫기
        CloseUserList();
        CloseCmdPanal();
        CloseUI(chatUI);
        CloseReportUI();
    }

    // 서버에서 수신된 메시지를 UI에 추가
    public void AddChat(ChatItem chatItem)
    {
        // 총 메시지 갯수 체크
        CheckNumOfChat();

        GameObject newMsg = Instantiate(baseChatFormat, chatList.transform);

        Text[] texts = newMsg.GetComponentsInChildren<Text>();
        texts[0].text = string.Format(CHAT_NICK, chatItem.Nickname);
        texts[1].text = chatItem.Contents;

        // 다른 유저에게 온 메시지 인 경우
        if (chatItem.session != BackEnd.Tcp.SessionInfo.None)
        {
            Button nickButton = newMsg.GetComponentInChildren<Button>();
            nickButton.onClick.AddListener(delegate {
                // 닉네임에 버튼이벤트 동적으로 연결
                // 닉네임을 누르면 cmd 패널 오픈
                OpenCmdPanal();
            });
        }
        // 시스템 메시지인 경우
        else if(!chatItem.isWhisper)
        {
            foreach (Text text in texts)
            {
                text.color = infoTextColor;
            }
        }

        // 귓속말인 경우
        if (chatItem.isWhisper)
        {
            foreach (Text text in texts)
            {
                text.color = whisperColor;
            }
        }
    }

    // 전송 버튼을 누르면 BackEndChatManager의 ChatToChannel 호출해 inputField 내에 있는 메시지를 보냄
    public void SendChatMsgToChannel()
    {
        if (inputField.text == "")
        {
            return;
        }

        BackEndChatManager.instance.ChatToChannel(inputField.text);
        inputField.text = "";
    }

    // 모든 채팅기록 초기화
    public void ClearChat()
    {
        int numOfChat = chatList.transform.childCount;
        for (int i = 0; i < numOfChat; ++i)
        {
            Destroy(chatList.transform.GetChild(i).gameObject);
        }
        chatList.transform.DetachChildren();
    }

    // 채팅 리스트 내 메시지 갯수 체크
    public void CheckNumOfChat()
    {
        // LIMIT_NUM_CHAT 이상의 메시지가 쌓이면 위에서부터 삭제
        int numOfChat = chatList.transform.childCount;
        if(numOfChat >= LIMIT_NUM_CHAT) 
        {
            GameObject deleteMsg = chatList.transform.GetChild(0).gameObject;
            deleteMsg.transform.SetParent(null);
            // 제일 위 객체를 삭제
            Destroy(deleteMsg);
        }
    }

    // CMD 패널 UI 열기
    public void OpenCmdPanal()
    {
        cmdUI.SetActive(true);

        GameObject nowObject = EventSystem.current.currentSelectedGameObject;

        // 닉네임 설정 (괄호를 지우기)
        string nickname = nowObject.GetComponentsInChildren<Text>()[0].text;
        nickname = nickname.Replace("[", "");
        nickname = nickname.Replace("]", "");
        clickNickName = nickname;

        Text userName = cmdUI.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        userName.text = nickname;
    }

    // CMD UI 닫기
    public void CloseCmdPanal()
    {
        cmdUI.SetActive(false);
    }

    // 귓속말 커맨드 입력 (/w nickname)
    public void WhisperButtonClick()
    {
        inputField.text = "/w " + clickNickName+" ";
        CloseCmdPanal();
        CloseUserList();
    }

    // 신고 UI 표시
    public void ReportButtonClick()
    {
        OpenReportUI(clickNickName);
        CloseCmdPanal();
        CloseUserList();
    }

    // 차단 커맨드 사용 (/b nickname)
    public void BlockButtonClick()
    {
        inputField.text = "/b " + clickNickName;
        CloseCmdPanal();
        CloseUserList();
    }

    // 모든 유저목록 초기화
    public void ClearUserList()
    {
        int numOfUser = userList.transform.childCount;
        for (int i = 0; i < numOfUser; ++i)
        {
            Destroy(userList.transform.GetChild(i).gameObject);
        }
        userList.transform.DetachChildren();
    }
    // 유저목록 띄우기
    public void ShowUserList(List<string> list)
    {
        CloseCmdPanal();
        CloseReportUI();
        userListUI.SetActive(true);
        ClearUserList();

        foreach (string user in list)
        {
            GameObject newUser = Instantiate(baseNameFormat, userList.transform);
            Text text = newUser.GetComponent<Text>();
            text.text = user;

            Button nickButton = newUser.GetComponentInChildren<Button>();
            nickButton.onClick.AddListener(delegate {
                // 닉네임에 버튼이벤트 동적으로 연결
                // cmd 패널 오픈
                OpenCmdPanal();
            });
        }
    }
    // 유저목록 UI 닫기
    public void CloseUserList()
    {
        userListUI.SetActive(false);
        cmdUI.SetActive(false);
    }
    // 유저목록 UI가 표시되어 있는니 체크
    public bool IsOpenUserList()
    {
        return userListUI.activeSelf;
    }
    // 신고 UI 열기
    public void OpenReportUI(string nickname)
    {
        clickNickName = nickname;
        reportUserName.text = nickname;
        reportUI.SetActive(true);
    }
    // 신고 UI 닫기
    public void CloseReportUI()
    {
        reportUI.SetActive(false);
        // 토글 전부 off
        int numOfButton = reportReason.transform.childCount;
        for (int i = 0; i < numOfButton; ++i)
        {
            Transform child = reportReason.transform.GetChild(i);
            Toggle toggle = child.GetComponent<Toggle>();
            toggle.isOn = false;
        }
    }
    // 유저 신고
    public void ReportUser()
    {
        int numOfButton = reportReason.transform.childCount;
        string reason = "null";
        for(int i=0; i<numOfButton; ++i)
        {
            Transform child = reportReason.transform.GetChild(i);
            Toggle toggle = child.GetComponent<Toggle>();
            if(toggle.isOn)
            {
                reason = child.GetComponentInChildren<Text>().text;
                toggle.isOn = false;
                break;
            }
        }

        if(reason == "null")
        {
            BackEndServerManager.instance.ShowMessage("신고사유를 선택해주세요.");
            return;
        }

        string detail = reportDetailReason.text;
        BackEndChatManager.instance.ReportUser(clickNickName, reason, detail); 
        reportDetailReason.text = "";
        CloseReportUI();
    }
#endregion

    // 광고제거버튼 활성화 / 비활성화
    public void SetRemoveAds (bool active)
    {
        //이미 구매한 경우 구매버튼 비활성화
        btnRemoveAds.SetActive(!active);
    }

    // 마켓 링크 열기
    public void GetLatestVersion()
    {
#if UNITY_ANDROID
        Application.OpenURL("market://details?id="+Application.identifier);
#elif UNITY_IOS
        Application.OpenURL("https://itunes.apple.com/kr/app/apple-store/"+"id1461432877");
#endif
    }

    // 처리상황 반환
	public bool IsProcessing() {
         return (loginUI.activeSelf||nickNameUI.activeSelf||noticeUI.activeSelf||rankUI.activeSelf||processingCoverUI.activeSelf);
     }

    // 해당 UI 표시
    private void ShowUI(GameObject uiObejct)
    {
        foreach (GameObject x in gameObjects)
        {
            if (x == uiObejct)
            {
                x.SetActive(true);
            }
            else
            {
                x.SetActive(false);
            }
        }
    }
    // 해당 UI 닫기
    private void CloseUI(GameObject uiObejct)
    {
        foreach (GameObject x in gameObjects)
        {
            if (x == uiObejct)
            {
                x.SetActive(false);
            }
        }
    }

#region 게임 종료 관련
    /*
     * 게임 종료 팝업 ON / OFF
     */

    public void GameClose()
    {
        // 에디터에서는 실행안됨
        Application.Quit();
    }

#endregion

}
