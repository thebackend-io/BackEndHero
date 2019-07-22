using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Include Backend
using BackEnd;
using BackEnd.Tcp;
using LitJson;

public class BackEndChatManager : MonoBehaviour {
    #region 가이드
    /* 정보
	 * SDK 버전 : Backend-3.8.3
	 * 
	 */
    #endregion
    public static BackEndChatManager instance = null;

    public Slider publicFilteringOn;

    private bool chatStatus;
    private ChannelType channelType;
    private List<ChannelNodeObject> channelList;
    private List<string> participantList;
    private ChatItem chatItem;
    private bool isFilterOn;
    private Image sliderImage;
    private string myNickName;

    private const int maxLength = 50;
    private const string infoText = "안내";
    private const string CHAT_INACTIVE = "채팅서버에 접속할 수 없습니다. 잠시 후 다시 시도해주세요.";
    private const string CHAT_ENTER = "{0} 채널에 입장하였습니다."; // '채널 별칭' 채널에 입장하였습니다.
    private const string CHAT_MSGLENGTHEXCEED = "입력 제한 글자수를 초과하였습니다.";
    private const string CHAT_UNKNOWNCMD = "잘못된 명령어입니다.";
    private const string CHAT_NOTARGET = "대상을 입력해주세요.";
    private const string CHAT_UNVAILDTARGET = "잘못된 대상입니다.";
    private const string CHAT_BLOCKFAIL = "존재하지 않는 닉네임입니다.";
    private const string CHAT_BLOCKSUCCESS = "{0}님을 차단합니다.";
    private const string CHAT_UNBLOCKSUCCESS = "{0}님을 차단 해제합니다.";
    private const string CHAT_REPORT = "{0}님을 신고하였습니다.";
    private const string CHAT_NOMSG = "귓속말을 입력해주세요.";
    private const string CHAT_WHISPER = "{0}에게 귓속말 : {1}";
    private  Color32 SLIDER_ON = new Color32(100, 108, 224, 255);
    private  Color32 SLIDER_OFF = new Color32(255, 255, 255, 255);

    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    void Start() {
        chatItem = null;
        chatStatus = false;
        isFilterOn = true;
        channelType = ChannelType.Public;
        channelList = new List<ChannelNodeObject>();
        participantList = new List<string>();

        if (publicFilteringOn)
        {
            sliderImage = publicFilteringOn.transform.GetChild(3).GetChild(0).GetComponent<Image>();
        }

        ChatHandlers();
    }

    void Update() {
        if (chatStatus)
        {
            Backend.Chat.Poll();
        }
    }

    void OnApplicationQuit()
    {
        // 게임 종료 시 채팅 채널 접속 해제
        LeaveChatServer();
    }

    #region 채팅창 상호작용
    /*
     * 채팅창 열기
     * 채팅창 닫기
     * 채팅 채널 접속
     * 채팅 채널 퇴장
     */

    //채팅창 열기
    public void OpenChatUI()
    {
        // 채팅창을 열 때 호출
        if(!chatStatus) 
        {
            // 채팅창을 열 때 채팅서버에 접속이 되어 있지 않으면 채팅 채널 접속
            if(!EnterChatServer()) 
            {
                Debug.Log("Fail To Enter Chat Server");
                return;
            }
        }
        Debug.Log("Open Chat UI");  
        BackEndUIManager.instance.ShowChatUI();
    }

    // 채팅창 닫기
    public void CloseChatUI()
    {
        // 채팅창을 닫을 때 호출
        Debug.Log("Close Chat UI");       
        BackEndUIManager.instance.CloseChatUI();
    }

    // 채팅채널 접속
    public bool EnterChatServer()
    {
        if (!GetChatStatus())
        {
            return false;
        }
        if (!GetChannelList())
        {
            return false;
        }

        foreach (ChannelNodeObject channel in channelList)
        {
            // 인원수 체크
            if (channel.joinedUserCount >= channel.maxUserCount)
            {
                continue;
            }
            // 활성화된 채널에 참가하고 루프 중단
            JoinChannel(channel);
            break;
        }
        return true;
    }

    public void EnterChatServerInUserInfo()
    {
        if (!GetChatStatus())
        {
            return;
        }
        if (!GetChannelList())
        {
            return;
        }

        foreach (ChannelNodeObject channel in channelList)
        {
            // 인원수 체크
            if (channel.joinedUserCount >= channel.maxUserCount)
            {
                continue;
            }
            // 활성화된 채널에 참가하고 루프 중단
            JoinChannel(channel);
            break;
        }
        return;
    }

    // 채팅채널 퇴장
    public void LeaveChatServer()
    {
        if(!chatStatus)
        {
            return;
        }
        // 현재 채널을 떠남
        Debug.Log("LeaveChatServer");
        Backend.Chat.LeaveChannel(channelType);
        //participantList.Clear();
    }
    #endregion

    #region 채팅서버 접속
    /*
     * 뒤끝챗 서버 접속
     * 활성화 된 일반채널 접속
     */

    // 뒤끝챗 서버 접속
    private bool GetChatStatus()
    {
        //현재 채팅서버 상태를 호출함
        BackendReturnObject chatStatusBRO = Backend.Chat.GetChatStatus();
        chatStatus = false;
        channelType = ChannelType.Public;

        if (!chatStatusBRO.IsSuccess())
        {
            BackEndServerManager.instance.ShowMessage(chatStatusBRO.ToString());
            Debug.Log("Fail To Connect Chat Server - " + chatStatusBRO);
            return false;
        }

        string chatServerStatus = chatStatusBRO.GetReturnValuetoJSON()["chatServerStatus"]["chatServer"].ToString();

        chatStatus |= chatServerStatus.Equals("y");
        Debug.Log("chatStatus - " + chatStatus);

        if (!chatStatus)
        {
            BackEndServerManager.instance.ShowMessage(CHAT_INACTIVE);
            return false;
        }

        return true;
    }

    // 활성화된 채널 리스트 받아오기 (public)
    private bool GetChannelList()
    {
        // Public 채널 리스트 받아오기
        if (!chatStatus)
        {
            return false;
        }

        channelList.Clear();
        BackendReturnObject chatChannelStatusBRO = Backend.Chat.GetChannelList();

        if (!chatChannelStatusBRO.IsSuccess() || chatChannelStatusBRO.IsServerError())
        {
            BackEndServerManager.instance.ShowMessage(CHAT_INACTIVE);
            chatStatus = false;
            Debug.Log("Fail To Get Chat Channel - " + chatChannelStatusBRO);
            return false;
        }

        JsonData rows = chatChannelStatusBRO.GetReturnValuetoJSON()["rows"];
        ChannelNodeObject node;

        foreach (JsonData data in rows)
        {
            node = new ChannelNodeObject(channelType, data["uuid"].ToString(), (int)data["joinedUserCount"], (int)data["maxUserCount"],
                                         data["serverHostName"].ToString(), data["serverPort"].ToString(), data["alias"].ToString());
            channelList.Add(node);
        }

        return true;
    }

    // 활성화된 채널에 참여
    private void JoinChannel(ChannelNodeObject node)
    {
        ErrorInfo info;
        Backend.Chat.JoinChannel(node.type, node.host, node.port, node.channel_uuid, out info);

        // 채널 입장 메시지
        chatItem = new ChatItem(SessionInfo.None, infoText, string.Format(CHAT_ENTER, node.alias), false);
        BackEndUIManager.instance.AddChat(chatItem);
    }
    #endregion

    #region 채널 관련 상호작용
    /* 채팅 핸들러 설정
     * 필터링
     * 도배장지 메시지
     * 자동 접속 종료 메시지
     * 입장
     * 유저목록
     * 퇴장
     * 채팅
     * 귓속말
     * 오류 핸들러
     */
    void ChatHandlers()
    {
        // 채팅 필터링 설정
        Backend.Chat.SetFilterReplacementChar('*');
        if (publicFilteringOn)
        {
            publicFilteringOn.onValueChanged.AddListener(value =>
            {
                Debug.Log("Chat Filter Status : " + value);
                isFilterOn = (int)value == 0 ? false : true;
                // 채팅이 활성화 안되어 있으면 그냥 리턴
                if (!chatStatus)
                {
                    return;
                }
                Backend.Chat.SetFilterUse(isFilterOn);
                if (sliderImage)
                {
                    if (isFilterOn)
                    {
                        sliderImage.color = SLIDER_ON;
                    }
                    else
                    {
                        sliderImage.color = SLIDER_OFF;
                    }
                }
            });
        }

        // 도배 방지 메시지 설정
        Backend.Chat.SetRepeatedChatBlockMessage("도배하면 안되요.");
        // 자동 접속 종료 메시지 설정
        Backend.Chat.SetTimeoutMessage("챗 안해서 채널에서 퇴장당했습니다.");

        // 현재 일반 채널에 접속해 있는 모든 게이머 정보 받아오기
        // 플레이어가 일반 채널에 입장할 때마다 호출됨
        Backend.Chat.OnSessionListInChannel = (args) =>
        {
            participantList.Clear();

            // 채널에 참여중인 유저 목록
            foreach (SessionInfo info in args.SessionList)
            {
                participantList.Add(info.NickName);
            }
        };

        // 플레이어 혹은 다른 유저가 채널에 입장하면 호출됨
        Backend.Chat.OnJoinChannel = (args) =>
        {
            Debug.Log("OnEnterChannel " + args.ErrInfo);

            if (args.ErrInfo == ErrorInfo.Success)
            {
                // 다른 유저가 접속한 경우
                if (args.Session.IsRemote)
                {
                    chatItem = new ChatItem(SessionInfo.None, infoText, string.Format("{0}이 입장했습니다.", args.Session.NickName), false);
                    BackEndUIManager.instance.AddChat(chatItem);
                }
                // 내가 접속한 경우
                else
                {
                    myNickName = args.Session.NickName;
                    Backend.Chat.SetFilterUse(isFilterOn);
                }

                // 접속한 세션이 채널 참여자 리스트에 존재하지 않을 경우 리스트에 추가
                if (!participantList.Contains(args.Session.NickName))
                {
                    participantList.Add(args.Session.NickName);
                }
            }
            else
            {
                // 접속 실패한 경우
                BackEndServerManager.instance.ShowMessage(string.Format("OnEnterChannel : {0}", args.ErrInfo.Reason));
            }
        };

        // 플레이어 혹은 다른 유저가 채널에서 퇴장하면 호출됨
        Backend.Chat.OnLeaveChannel = (args) =>
        {
            Debug.Log("OnLeaveChannel " + args.ErrInfo);

            // 플레이어가 채널에서 퇴장한 경우
            if (!args.Session.IsRemote)
            {
                if (args.ErrInfo.Category != ErrorCode.Success)
                {
                    if (args.ErrInfo.Category == ErrorCode.DisconnectFromRemote &&
                       args.ErrInfo.Detail == ErrorCode.ChannelTimeout)
                    {
                        BackEndServerManager.instance.ShowMessage(args.ErrInfo.Reason);
                    }
                    else
                    {
                        BackEndServerManager.instance.ShowMessage(string.Format("OnLeaveChannel : {0}", args.ErrInfo.Reason));
                    }
                    CloseChatUI();
                }
            }
            // 다른 유저가 채널에서 퇴장한 경우
            else
            {
                chatItem = new ChatItem(SessionInfo.None, infoText, string.Format("{0}이 퇴장했습니다.", args.Session.NickName), false);
                BackEndUIManager.instance.AddChat(chatItem);

                participantList.Remove(args.Session.NickName);
            }
        };

        // 채팅이 왔을 때 호출됨
        Backend.Chat.OnChat = (args) =>
        {
            Debug.Log(string.Format("OnChat {0}, {1}", DateTime.Now.TimeOfDay, args.Message));
            if (args.ErrInfo == ErrorInfo.Success)
            {
                chatItem = new ChatItem(args.From, args.From.NickName, args.Message, args.From.IsRemote);
                BackEndUIManager.instance.AddChat(chatItem);

            }
            else if (args.ErrInfo.Category == ErrorCode.OperationFailed_Category_Chat)
            {
                //도배방지 메시지
                if (args.ErrInfo.Detail == ErrorCode.BannedChat)
                {
                    chatItem = new ChatItem(SessionInfo.None, infoText, args.ErrInfo.Reason, false);
                    BackEndUIManager.instance.AddChat(chatItem);
                }
            }

        };
        // 귓속말이 왔을 때 호출됨
        Backend.Chat.OnWhisper = (args) =>
        {
            Debug.Log(string.Format("OnWhisper from {0}, to {1} : {2}", args.From.NickName, args.To.NickName, args.Message));
            if (args.ErrInfo == ErrorInfo.Success)
            {
                if(myNickName != args.From.NickName)
                {
                    string tmpMsg = string.Format("의 귓속말 : {0}", args.Message);
                    chatItem = new ChatItem(args.From, args.From.NickName, tmpMsg, true, args.From.IsRemote);
                }
                else
                {
                    chatItem = new ChatItem(SessionInfo.None, args.From.NickName, string.Format(CHAT_WHISPER, args.To.NickName, args.Message), true, args.From.IsRemote);
                }
                BackEndUIManager.instance.AddChat(chatItem);
            }
            else
            {
                Debug.Log(args.ErrInfo);
            }
        };

        // Exception 발생 시
        Backend.Chat.OnException = (e) =>
        {
            Debug.LogError(e);
        };
    }

    // 필터링 슬라이더 설정
    public void SliderButtonFunc()
    {
        int value = (int)publicFilteringOn.value;
        if(value == 0)
        {
            publicFilteringOn.value = 1;
        }
        else
        {
            publicFilteringOn.value = 0;
        }
    }

    // 유저 목록 보기
    public void ShowAllUserListInChannel()
    {
        if(!BackEndUIManager.instance.IsOpenUserList())
        {
            BackEndUIManager.instance.ShowUserList(participantList);
        }
        else
        {
            BackEndUIManager.instance.CloseUserList();
        }
    }

    #endregion

    #region 메시지
    /*
     * 메시지 체크 (길이 등등)
     * 일반 채널에 메시지 보내기
     * 커맨드 명령어 수행
     */

    // 메시지 길이 체크
    private int GetStringLength(string msg)
    {
        return System.Text.Encoding.Unicode.GetByteCount(msg);
    }

    // 채널에 메시지 전송
    public void ChatToChannel(string msg)
    {
        if(!chatStatus)
        {
            // 채팅 상태가 false이면 곧바로 리턴
            return;
        }

        if(GetStringLength(msg) > maxLength)
        {
            // 글자수가 최대 글자수를 넘어가는 경우 오류메시지를 띄우고 리턴
            BackEndServerManager.instance.ShowMessage(CHAT_MSGLENGTHEXCEED);
            return;
        }

        if(msg.Length <= 0)
        {
            // 글자수가 0 이하인 경우 어떤 동작도 수행하지 않고 바로 리턴
            // 오류메시지는 표시하지 않음
            return;
        }

        if(msg.StartsWith("/", System.StringComparison.CurrentCulture))
        {
            // command 명령어인 경우

            // 띄어쓰기 단위로 단어 분활
            string[] msgSplit = msg.Split(' ');

            if (msgSplit.Length < 2)
            {
                CmdError(CHAT_NOTARGET);
                return;
            }

            string nickName = msgSplit[1];

            // 닉네임이 내 닉네임이거나, 공백이면 에러메시지 표시
            if(nickName.Equals(myNickName) || string.IsNullOrEmpty(nickName))
            {
                CmdError(CHAT_UNVAILDTARGET);
                return;
            }

            if (IsWhisperCmd(msgSplit[0]))
            {
                if(msgSplit.Length < 3)
                {
                    CmdError(CHAT_NOMSG);
                }
                var wMsgLen = msgSplit[0].Length + msgSplit[1].Length + 2;
                if(wMsgLen < msg.Length)
                {
                    string wMsg = msg.Substring(wMsgLen);
                    Backend.Chat.Whisper(nickName, wMsg);
                }
            }
            else if(IsBlockCmd(msgSplit[0]))
            {
                BlockUser(nickName);
            }
            else if(IsUnblockCmd(msgSplit[0]))
            {
                UnBlockUser(nickName);
            }
            else if(IsReportCmd(msgSplit[0]))
            {
                BackEndUIManager.instance.OpenReportUI(nickName);
            }
            else
            {
                CmdError(CHAT_UNKNOWNCMD);
            }
        }
        else
        {
            //플레이어가 채팅창에 입력한 내용을 현재 채널로 전송
            Backend.Chat.ChatToChannel(channelType, msg);
        }
    }

    // 귓속말
    private bool IsWhisperCmd(string message)
    {
        return message.Equals("/ㄱ")
                    || message.Equals("/w")
                    || message.Equals("/귓");
    }
    // 차단
    private bool IsBlockCmd(string message)
    {
        return message.Equals("/b")
                    || message.Equals("/차단");
    }
    // 차단 해제
    private bool IsUnblockCmd(string message)
    {
        return message.Equals("/ub")
                      || message.Equals("/차단해제");
    }
    // 신고
    private bool IsReportCmd(string message)
    {
        return message.Equals("/신고");
    }
    // 알수없는 커맨드
    private void CmdError(string msg)
    {
        chatItem = new ChatItem(SessionInfo.None, infoText, msg, false);
        BackEndUIManager.instance.AddChat(chatItem);
    }
    #endregion

    #region 커맨드 명령어
    /*
     * 유저 차단
     * 유저 차단 해제
     * 유저 신고
     */

    // 유저 차단
    private void BlockUser(string nickName)
    {
        Backend.Chat.BlockUser(nickName, arg =>
        {
        ChatItem tmpMsg;
            // 차단 성공한 경우
            if (arg)
            {
                tmpMsg = new ChatItem(SessionInfo.None, infoText, string.Format(CHAT_BLOCKSUCCESS, nickName), false); 
            }
            // 차단 실패한 경우
            else
            {
                tmpMsg = new ChatItem(SessionInfo.None, infoText, CHAT_BLOCKFAIL, false);
            }
            // add chat에서 인스턴스 생성은 메인스레드에서만 가능
            // 그러므로 디스패처에 이벤트 등
            Dispatcher.Instance.Invoke(()=>
            {
                BackEndUIManager.instance.AddChat(tmpMsg);
            });
        });
    }

    // 유저 차단 해제
    private void UnBlockUser(string nickName)
    {
        ChatItem tmpMsg;
        if (Backend.Chat.UnblockUser(nickName))
        {
            tmpMsg = new ChatItem(SessionInfo.None, infoText, string.Format(CHAT_UNBLOCKSUCCESS, nickName), false);
        }
        else
        {
            tmpMsg = new ChatItem(SessionInfo.None, infoText, CHAT_BLOCKFAIL, false);
        }
        BackEndUIManager.instance.AddChat(tmpMsg);
    }

    // 유저 신고
    public void ReportUser(string nickName, string reason, string detail)
    {
        Backend.Chat.ReportUser(nickName, reason, detail, bro =>
        {
            if(bro.IsSuccess())
            {
                ChatItem tmpMsg;
                tmpMsg = new ChatItem(SessionInfo.None, infoText, string.Format(CHAT_REPORT, nickName), false);
                Dispatcher.Instance.Invoke(() =>
                {
                    BackEndUIManager.instance.AddChat(tmpMsg);
                });
            }
            else
            {
                Debug.Log("Report Fail : " + bro);
                BackEndServerManager.instance.ShowMessage("유저신고를 실패하였습니다. - 사유를 입력해주세요.");
            }

            
        });
    }

    #endregion


}

// 채널 노드 관련 클래스
public class ChannelNodeObject
{
    public ChannelType type;    // 채널 타입
    public string channel_uuid; // 채널 uuid
    public string participants; // 채널 참가자
    public int joinedUserCount; // 참가 인원수
    public int maxUserCount;    // 최대 참가 인원 (100)
    public string host;         // host
    public ushort port;         // port
    public string alias;        // 채널 별명

    public ChannelNodeObject(ChannelType type, string uuid, int joinedUser, int maxUser, string host, string port, string alias)
    {
        this.type = type;
        this.channel_uuid = uuid;
        this.joinedUserCount = joinedUser;
        this.maxUserCount = maxUser;
        this.participants = joinedUser + "/" + maxUser;
        this.host = host;
        this.port = ushort.Parse(port);
        this.alias = alias;
    }
}

// 채팅 메시지 관련 클래스
public class ChatItem
{
    internal SessionInfo session { get; set; } // 메시지 보낸 세션 정보
    internal bool IsRemote { get; set; }       // true : 다른유저가 보낸 메시지, false : 내가 보낸 메시지
    internal bool isWhisper { get; set; }      // 귓속말 여부
    internal string Nickname;                  // 보낸이 이름
    internal string Contents;                  // 실제 메시지

    internal ChatItem(SessionInfo session, string nick, string cont, bool isWhisper, bool IsRemote)
    {
        this.session = session;
        Nickname = nick;
        Contents = cont;
        this.isWhisper = isWhisper;
        this.IsRemote = IsRemote;
    }

    internal ChatItem(SessionInfo session, string nick, string cont, bool IsRemote)
    {
        this.session = session;
        Nickname = nick;
        Contents = cont;
        isWhisper = false;
        this.IsRemote = IsRemote;
    }
}
