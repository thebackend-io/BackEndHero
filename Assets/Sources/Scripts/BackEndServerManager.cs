using System;
using System.Collections.Generic;
using UnityEngine;
//  Include GPGS namespace
using GooglePlayGames;
using GooglePlayGames.BasicApi;
// Include Backend
using BackEnd;
using LitJson;

public class BackEndServerManager : MonoBehaviour
{
    #region 가이드
    /* 정보
	 * SDK 버전 : Backend-3.8.3
	 * 커스텀 회원가입
	 * 페더레이션 회원 가입 (GPGS)
	 * 닉네임 설정
	 * 공지사항
	 * 랭킹
	 * 영수증 검증
	 */
    #endregion

    public static BackEndServerManager instance;

    public string rankUuid = "";
    private string currentVersion = "";

    //비동기로 가입, 로그인을 할때에는 Update()에서 처리를 해야합니다. 이 값은 Update에서 구현하기 위한 플래그 값 입니다.
    BackendReturnObject bro = new BackendReturnObject();

    bool isSuccess = false;

    // 커스텀 아이디 비밀번호 부가정보
    string id, pw, etc;
    string nickName; // 닉네임
    string userInDateScore;
    string userInDatePurchase;
    private string ScoreTableName = "score", ScoreColumnName = "score";
    private string PurchastTableName = "purchase", PurchaseColumnName = "purchase";
    internal List<Notice> noticeList = new List<Notice>();

    void Awake()
    {
        instance = this;
    }

    // 뒷끝 예제
    void Start()
    {
#if UNITY_ANDROID
        // ----- GPGS -----
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestEmail()
            .RequestIdToken()
            .Build();

        //커스텀된 정보로 GPGS 초기화
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        //GPGS 시작.
        PlayGamesPlatform.Activate();
#endif
        currentVersion = Application.version;
        // ----- 뒤끝 -----
        BackEndUIManager.instance.SetProcessing(true); //프로세싱 팝업 켜기

        // 서버가 초기화되지 않은 경우
        if (!Backend.IsInitialized)
        {
            // 초기화
            Backend.Initialize(backendCallback);
        }
    }

    void Update()
    {
        if (isSuccess)
        {
            Backend.BMember.SaveToken(bro);
            isSuccess = false;
            bro.Clear();
            OnBackendAuthorized();
		}

		Dispatcher.Instance.InvokePending();
    }

    // 초기화 함수 이후에 실행되는 callback 
    void backendCallback(BackendReturnObject BRO)
    {
        //프로세싱 팝업 끄기
        DispatcherAction(() => BackEndUIManager.instance.SetProcessing(false));

        // 초기화 성공한 경우 실행
        if (BRO.IsSuccess())
        {
            // 구글 해시키 획득 
            if (!string.IsNullOrEmpty(Backend.Utils.GetGoogleHash()))
                Debug.Log(Backend.Utils.GetGoogleHash());

            // 서버시간 획득
            Debug.Log(Backend.Utils.GetServerTime());
            // Application 버전 확인
            CheckApplicationVersion();
        }
        // 초기화 실패한 경우 실행
        else
        {
            ShowMessage("초기화 실패", 3f);
            Debug.Log("초기화 실패 - " + BRO);
        }
    }

    private void CheckApplicationVersion()
    {
        Backend.Utils.GetLatestVersion(versionBRO =>
        {
            if (versionBRO.IsSuccess())
            {
                string latest = versionBRO.GetReturnValuetoJSON()["version"].ToString();
                Debug.Log("version info - current: " + currentVersion + " latest: " + latest);
                if (currentVersion != latest)
                {
                    int type = (int)versionBRO.GetReturnValuetoJSON()["type"];
                    // type = 1 : 선택, type = 2 : 강제
                    bool isForced = type == 1;
                    DispatcherAction(() => BackEndUIManager.instance.ShowUpdateUI(isForced));
                }
                else
                {
                    // 뒷끝 토큰 로그인 시도
                    LoginWithTheBackendToken();
                }
            }
            else
            {
                // 뒷끝 토큰 로그인 시도
                LoginWithTheBackendToken();
            }
        });
    }


    // ====================================================================================================
    #region 로그인 / 로그아웃
    /*
     *
     */
    // ##### 로그인 #####
    // 기기에 저장된 뒤끝 AccessToken으로 로그인 (페더레이션, 커스텀 회원가입 또는 로그인 이후에 시도 가능)
    public void LoginWithTheBackendToken()
    {
        Backend.BMember.LoginWithTheBackendToken(loginBro =>
        {
            if (loginBro.IsSuccess())
            {
                // 뒤끝 토큰 로그인 성공
                isSuccess = true;
                bro = loginBro;
            }
            else
            {
                // 뒤끝 토큰 로그인 실패
                // 개인정보 취급 UI 열기
                DispatcherAction(BackEndUIManager.instance.ShowPrivacyUI);
                Debug.Log("로그인 실패 - "+loginBro.ToString());
            }
        });
    }

    // 푸시 알람 설정
    public void SetPush(bool flag, bool showMsg)
    {
        if(flag) 
		{
			// 푸시설정 ON
#if UNITY_ANDROID && !UNITY_EDITOR
			Backend.Android.PutDeviceToken();
#elif UNITY_IOS && !UNITY_EDITOR
			//Backend.iOS.PutDeviceToken(isDevelopment.iosDev); // 개발자 버전에서 사용
            Backend.iOS.PutDeviceToken(isDevelopment.iosProd); // 실제 배포용
#else
#endif
            if(showMsg) 
            {
                ShowMessage("푸시 알람이 활성화 되었습니다.");
            }
		}
		else
		{
            // 푸시설정 OFF
#if UNITY_ANDROID && !UNITY_EDITOR
			Backend.Android.DeleteDeviceToken();
#elif UNITY_IOS && !UNITY_EDITOR
			Backend.iOS.DeleteDeviceToken( );
#else
#endif
            if(showMsg) 
            {
                ShowMessage("푸시 알람이 비활성화 되었습니다.");
            }
        }
    }

    // 유저 정보 초기화
    public void OnBackendAuthorized()
    {
        GetUserInfo();  // 유저정보 가져오기 후 변수에 저장
        GetRemoveAds(); // 유저 광고제거 구매정보 가져오기 후 저장
        UpdateScore2(0);// 최고점수 갱신
        BackEndChatManager.instance.EnterChatServer(); //채팅서버 접속
    }

    #endregion

    // ====================================================================================================
    #region 페더레이션 회원 가입 (GPGS)
    /* 
     *
	 */
    // GPGS 로그인 
    public void GPGSLogin()
    {
#if UNITY_ANDROID
        // 이미 로그인 된 경우
        if (Social.localUser.authenticated == true)
        {
            bro = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");
            if (bro.IsSuccess())
            {
                // 성공시
                OnBackendAuthorized();
                BackEndUIManager.instance.CloseAll();
                Debug.Log("1 GPGS 로그인 성공");
                MessagePopManager.instance.ShowPop("GPGS 로그인 성공");
            }
        }
        else
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    // 로그인 성공 -> 뒤끝 서버에 획득한 구글 토큰으로 가입요청
                    bro = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");

                    if (bro.IsSuccess())
                    {
                        // 성공시
                        OnBackendAuthorized();
                        BackEndUIManager.instance.CloseAll();
                        Debug.Log("2 GPGS 로그인 성공");
                        MessagePopManager.instance.ShowPop("GPGS 로그인 성공");
                    }
                }
                else
                {
                    // 로그인 실패
                    Debug.Log("Login failed for some reason");
                    Debug.LogError("GPGS 로그인 실패");
                    MessagePopManager.instance.ShowPop("[X]GPGS 로그인 실패", 10f);
                }
            });
        }
#endif
    }

    // 구글 토큰 받아옴
    public string GetTokens()
    {
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            // 유저 토큰 받기 첫번째 방법
            string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
            // 두번째 방법
            // string _IDtoken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
            return _IDtoken;
        }
        else
        {
            MessagePopManager.instance.ShowPop("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail", 10f);
            Debug.Log("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");     
        }
#endif

        return null;
    }
    #endregion

    // ====================================================================================================
    #region 닉네임 설정
    /*
     *
     */
    // ##### 닉네임 수정 #####
    public void UpdateNickname(string _newNickName)
    {
        Backend.BMember.UpdateNickname(_newNickName, nickNameBro =>
        {
            if (nickNameBro.IsSuccess())
            {
                nickName = _newNickName;
                ShowMessage("닉네임 변경 완료");
                DispatcherAction(BackEndUIManager.instance.CloseAll);
            }
            else
            {
                ShowMessage("[X]닉네임 변경 실패");
                Debug.Log("닉네임 변경 실패 - "+ nickNameBro);
            }

        });
    }

    public int DuplicateNickNameCheck(string _newNickName)
    {
        BackendReturnObject bro = Backend.BMember.CheckNicknameDuplication(_newNickName);

        if(bro.IsSuccess()) {
            return 0;
        }
        return int.Parse(bro.GetStatusCode());
    }
    #endregion

    // ====================================================================================================
    #region 공지사항
    /*
     *
     */
    // 공지 리스트 불러오기
    public void NoticeList()
    {
        Backend.Notice.NoticeList(noticeListBro =>
        {
            if (noticeListBro.IsSuccess())
            {
                // 공지 리스트 불러오기 성공
                JsonData rows = noticeListBro.GetReturnValuetoJSON()["rows"];
                Notice notice;
                JsonData data;
                noticeList.Clear();
                for (int i = 0; i < rows.Count; i++)
                {
                    data = rows[i];
                    notice = new Notice
                    {
                        Title = data["title"]["S"].ToString(),
                        Content = data["content"]["S"].ToString(),
                    };

                    noticeList.Add(notice);
                }
                // 리스트 생성 후 공지 리스트 UI를 띄움
                Dispatcher.Instance.Invoke(BackEndUIManager.instance.ShowNoticeListUI);
            }
            else
            {
                // 공지 불러오기 실패
                ShowMessage("[X]공지 불러오기 실패");
                Debug.Log("공지사항 불러오기 실패 - " + noticeListBro.ToString());
            }
        });

    }
    #endregion

    // ====================================================================================================

    #region 랭킹
    /* 
     *
	 */
    static string dash = "-";

    [HideInInspector]
    public List<RankItem> rankTop10DataList = new List<RankItem>();
    [HideInInspector]
    public RankItem myRankData = new RankItem();
    private int lastUpdatedRankTime = -1;
    private static DateTime today;
    private int[] updatingRankTime = { 0, 6, 12, 18 };

    public RankItem EmptyRank = new RankItem
    {
        nickname = dash,
        score = dash,
        rank = dash
    };

    // 랭킹 가져오기
    public void GetRank()
    {
        // lastUpdatedRankTime 이 초기화 되지 않은 경우 -> 랭킹 받아오기
        if (lastUpdatedRankTime < 0)
        {
            GetRankTop10ByUuid();
        }
        // lastUpdatedRankTime 이 초기화 된 경우 -> 서버시간 확인 후 랭킹 받아오기
        else
        {
            Backend.Utils.GetServerTime(serverTime =>
            {
                string time = serverTime.GetReturnValuetoJSON()["utcTime"].ToString();
                DateTime now = DateTime.Parse(time);
                if (now.Hour < lastUpdatedRankTime || (now.Hour == lastUpdatedRankTime && now.Minute < 0))
                {
                    Debug.Log("GetServerTime get rank");
                    GetRankTop10ByUuid();
                }
                else
                {
                    Debug.Log("GetServerTime show ui");
                    DispatcherAction(BackEndUIManager.instance.ShowRankUI);
                }
            });
        }



    }

    // 랭킹 불러오기 (UUID지정)
    private void GetRankTop10ByUuid()
    {
        // 랭킹 불러오기
        RankItem item;
        int rankCount = 10;
        Backend.Rank.GetRankByUuid(rankUuid, rankCount, rankListBro =>
        {
            if (rankListBro.IsSuccess())
            {
                // 내 랭킹 가져오기 
                GetMyRank();

                // lastUpdatedRankTime 업데이트 
                DateTime now = DateTime.Now;
                foreach (int date in updatingRankTime)
                {
                    if (now.Hour < date || (now.Hour == date && now.Minute < 0))
                    {
                        break;
                    }
                    lastUpdatedRankTime = date;
                }

                // rank 저장
                JsonData rankData = rankListBro.GetReturnValuetoJSON()["rows"];
                rankTop10DataList.Clear();
                for (int i = 0; i < rankCount; i++)
                {
                    if (rankData[i] != null)
                    {
                        item = new RankItem
                        {
                            nickname = rankData[i].Keys.Contains("nickname") ? rankData[i]["nickname"]["S"].ToString() : dash,
                            rank = rankData[i].Keys.Contains("rank") ? rankData[i]["rank"]["N"].ToString() : dash,
                            score = rankData[i].Keys.Contains("score") ? rankData[i]["score"]["N"].ToString() : dash
                        };
                        rankTop10DataList.Add(item);
                    }
                }

            }
            else
            {
                ShowMessage("[X]랭킹 불러오기 실패");
                Debug.Log("랭킹 불러오기 실패 - " + rankListBro.ToString());
            }

        });
    }

    // 접속한 게이머 랭킹 가져오기 (UUID지정)
    public void GetMyRank()
    {
        Backend.Rank.GetMyRank(rankUuid, myRankBro =>
        {
            if (myRankBro.IsSuccess())
            {
                JsonData rankData = myRankBro.GetReturnValuetoJSON()["rows"];
                if (rankData[0] != null)
                {
                    myRankData = new RankItem
                    {
                        nickname = rankData[0].Keys.Contains("nickname") ? rankData[0]["nickname"]["S"].ToString() : dash,
                        rank = rankData[0].Keys.Contains("rank") ? rankData[0]["rank"]["N"].ToString() : dash,
                        score = rankData[0].Keys.Contains("score") ? rankData[0]["score"]["N"].ToString() : dash
                    };
                }
                else
                {
                    myRankData = EmptyRank;
                }

            }
            else
            {
                myRankData = EmptyRank;
                Debug.Log("유저 랭킹 불러오기 실패 - " + myRankBro);
                // ShowMessage("유저 랭킹 불러오기 실패");
            }

            // 랭킹 ui 
            DispatcherAction(BackEndUIManager.instance.ShowRankUI);
        });

    }
    #endregion

    // ====================================================================================================
    #region 영수증 검증
    /* 
     * 영수증 검증은 InAppPurchaser 스크립트에 있습니다.
	 */
    #endregion

    // ====================================================================================================
    #region 유저 정보
    /* 
     *
	 */
    // ##### 유저 정보 #####
    public void GetUserInfo()
    {
        Backend.BMember.GetUserInfo(userInfoBro =>
        {
            if (userInfoBro.IsSuccess())
            {
                JsonData Userdata = userInfoBro.GetReturnValuetoJSON()["row"];
                JsonData nicknameJson = Userdata["nickname"];

                // 닉네임 여부를 확인
                if (nicknameJson != null)
                {
                    // nickName 변수에 닉네임 저장
                    nickName = nicknameJson.ToString();
                    Debug.Log("NickName is " + nickName);
                    DispatcherAction(BackEndUIManager.instance.CloseAll);
                }
                else
                {
                    Debug.Log("NickName is null");
                    DispatcherAction(() => BackEndUIManager.instance.ShowNickNameUI(false));
                }
            }
            else
            {
                ShowMessage("[X]유저 정보 가져오기 실패");
                Debug.Log("[X]유저 정보 가져오기 실패 - " + userInfoBro);
            }
        });
    }

    public string GetNickName()
    {
        return nickName;
    }
    #endregion

    // ====================================================================================================
    #region 데이터 관리
    /* 
     *
	 */
    // ##### 점수 저장 #####
    // 데이터(점수) 생성
    internal void InsertScore(int _score)
    {
        Param param = new Param();
        param.Add(ScoreColumnName, _score);

        Backend.GameInfo.Insert(ScoreTableName, param, insertScoreBro =>
        {
            Debug.Log("InsertScore - " + insertScoreBro);
			InsertLog("BestScoreUpdateSuccess", "Success : " + _score);
			userInDateScore = insertScoreBro.GetInDate();
        });
    }

    // 데이터(점수) 업데이트
    private void UpdateScore(int _score)
    {
        // UI의 HighScore 업데이트
        DispatcherAction(() => GameManager.instance.UpdateHighScore());

        // 서버로 삽입할 데이터 생성
        Param param = new Param();
        param.Add(ScoreColumnName, _score);
        Backend.GameInfo.Update(ScoreTableName, userInDateScore, param, updateBro =>
        {
            Debug.Log("UpdateScore - " + updateBro.ToString());
            if (updateBro.IsSuccess())
            {
                ShowMessage("최고점수 저장 성공");
                InsertLog("BestScoreUpdateSuccess", "Success : " + _score);
            }
            else
            {
                ShowMessage("최고점수 저장 실패\n" + updateBro.ToString(), 10f);
                InsertLog("BestScoreUpdateFailed", "Failed : " + _score);
            }
        });
    }

    public void UpdateScore2(int _score)
    {
        // 유저 스코어 조회해보지 않은 경우
        if (string.IsNullOrEmpty(userInDateScore))
        {
            Backend.GameInfo.GetPrivateContents(ScoreTableName, myScoreBro =>
            {
                Debug.Log("플레이어 점수 정보 - " + myScoreBro.ToString());
                if (myScoreBro.IsSuccess())
                {
                    JsonData userData = myScoreBro.GetReturnValuetoJSON()["rows"];
                    // 유저 스코어가 존재하는 경우
                    if (userData.Count > 0)
                    {
                        userInDateScore = userData[0]["inDate"]["S"].ToString();
                        GameManager.bestScore = int.Parse(userData[0][ScoreColumnName]["N"].ToString());
                        DispatcherAction(() => GameManager.instance.UpdateHighScore());

                        // 유저 스코어 update
                        if (GameManager.bestScore < _score)
                            UpdateScore(_score);
                    }
                    // 유저 스코어가 존재하지 않는 경우
                    else
                    {
                        // 유저 스코어 insert
                        InsertScore(_score);
                    }
                }
                else
                {
                    InsertScore(_score);
                    ShowMessage("유저 점수 가져오기 실패");
                }
            });
        }
        else
        {
            UpdateScore(_score);
        }
    }

    // ##### 광고제거 구매 저장 #####
    void InsertPurchase(bool _isPurchase)
    {
        Param param = new Param();
        param.Add(PurchaseColumnName, _isPurchase ? "Y" : "N");
        Debug.Log("InsertPurchase - " + Backend.GameInfo.Insert(PurchastTableName, param));
    }

    // 구매내역 업데이트
    void UpdatePurchase(bool _isPurchase)
    {
        Param param = new Param();
        param.Add(PurchaseColumnName, _isPurchase ? "Y" : "N");
        Backend.GameInfo.Update(PurchastTableName, userInDatePurchase, param, updateBro =>
        {
            if (updateBro.IsSuccess())
            {
                ShowMessage("광고제거 구매 저장 성공");
            }
            else
            {
                ShowMessage("[X]광고제거 구매 저장 실패");
                Debug.Log("광고제거 구매 저장 실패 - " + updateBro.ToString());
            }
        });

    }

    // 광고 제거 구매내역 관련
    public void GetRemoveAds()
    {
        Backend.GameInfo.GetPrivateContents("purchase", myRemoveAdsBro =>
        {
            if (myRemoveAdsBro.IsSuccess())
            {
                //JsonData userData = myRemoveAdsBro.GetReturnValuetoJSON()["rows"][0];
                JsonData userData = myRemoveAdsBro.GetReturnValuetoJSON()["rows"];
                if (userData.Count > 0)
                {
                    //count가 1이상이면 구매내역이 존재
                    userData = userData[0];
                    userInDatePurchase = userData["inDate"]["S"].ToString();
                    Debug.Log("Purchase state - " + userData["purchase"]["S"]);
                    if ((userData["purchase"]["S"].ToString() == "Y") ? true : false)
                    {
                        DispatcherAction(() => BackEndUIManager.instance.SetRemoveAds(true));
                        AdsManager.instance.SetRemoveAds();
                    }
                }
                else
                {
                    //구매내역이 존재하지 않는 경우
                    InsertPurchase(false);
                    DispatcherAction(() => BackEndUIManager.instance.SetRemoveAds(false));
                }
            }
            else
            {
                ShowMessage("[X]구매내역 가져오기 실패");
                Debug.Log("구매내역 불러오기 실패 - " + myRemoveAdsBro);
            }
        });

    }

    // 게임 상단 메시지 띄우기
    public void ShowMessage(string msg)
    {
        DispatcherAction(() => MessagePopManager.instance.ShowPop(msg));
    }

    private void ShowMessage(string msg, float time)
    {
        DispatcherAction(() => MessagePopManager.instance.ShowPop(msg, time));
    }

    // Dispatcer에서 action 실행 (메인스레드)
    private void DispatcherAction(Action action)
    {
        Dispatcher.Instance.Invoke(action);
    }

    // 광고제거 구매 true로 설정
    public void SetRemoveAds()
    {
        UpdatePurchase(true);
        DispatcherAction(() => BackEndUIManager.instance.SetRemoveAds(true));
    }

    // 광고제거 아이탬 구매 성공
    public void BuyRemoveAdsSuccess()
    {
        InsertLog("BuyRemoveAds", "Success");
        UpdatePurchase(true);
        DispatcherAction(() => BackEndUIManager.instance.SetRemoveAds(true));
    }
    // 광고제거 아이템 구매 실패
    public void BuyRemoveAdsFailed()
    {
        InsertLog("BuyRemoveAds", "Failed");
    }
#endregion

#region 로그기록
    /*
    *
    */

    private void InsertLog(string logType, string content)
    {
        Param param = new Param();
        param.Add("content", content);
        Backend.GameInfo.InsertLog(logType, param, bro => {
            Debug.Log("InsertLog - " + bro.ToString());
        });
    }

#endregion

}

// 랭크 클래스
public class RankItem
{
    public string nickname { get; set; } // 닉네임
    public string score { get; set; }    // 점수
    public string rank { get; set; }     // 랭크
}

// 공지사항 클래스
public class Notice
{
    internal string Title { get; set; }          // 공지사항 제목
    internal string Content { get; set; }        // 공지사항 내용
    internal string ImageKey { get; set; }       // 공지사항 이미지 (존재할 경우)
    internal string LinkButtonName { get; set; } // 공지사항 버튼 (누르면 공지사항 UI를 띄우기 위한)
    internal string LinkUrl { get; set; }        // 공지사항 URL (존재할 경우)
}
