using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticePop : MonoBehaviour {

    public GameObject noticeUI;
    public Text Title;
    public Text Content;

    private static NoticePop noticePop;

    public static NoticePop Instance()
    {
        if (!noticePop)
        {
            noticePop = FindObjectOfType(typeof(NoticePop)) as NoticePop;
            if (!noticePop)
                Debug.LogWarning("There needs to be one active NoticePop script on a GameObject in your scene.");
        }

        return noticePop;
    }


    public void SetNotice(Notice notice) 
    {
        noticeUI.SetActive(true);
        Title.text = notice.Title;
        Content.text = notice.Content;
		//if (txt != null) {
		//	txt.text = "# 작성자\n" + BackEndServerManager.instance.nAuthor
		//			+ "\n\n# 게시 일시\n" + BackEndServerManager.instance.nPostingDate
		//			+ "\n\n# 제목\n" + BackEndServerManager.instance.nTitle
		//			+ "\n\n# 내용\n" + BackEndServerManager.instance.nContent
		//			+ "\n\n# 이미지 링크\n" + BackEndServerManager.instance.nImageKey
		//			+ "\n\n# 외부 링크\n" + BackEndServerManager.instance.nLinkUrl
		//			+ "\n\n# 외부 링크 버튼 이름\n" + BackEndServerManager.instance.nLinkButtonName;
		//}
	}
}
