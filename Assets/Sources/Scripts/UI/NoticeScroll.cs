using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeScroll : MonoBehaviour
{
    public GameObject prefab; 

    public RectTransform noticeContent;

    private static NoticeScroll noticeScroll;

    public static NoticeScroll Instance()
    {
        if (!noticeScroll)
        {
            noticeScroll = FindObjectOfType(typeof(NoticeScroll)) as NoticeScroll;
            if (!noticeScroll)
                Debug.LogWarning("There needs to be one active NoticeScroll script on a GameObject in your scene.");
        }

        return noticeScroll;
    }


    void OnEnable()
    {
        PopulateList();
    }

    internal void PopulateList()
    {
        //존재하던 리스트를 삭제 (reset)
        RemoveAllListViewItem();

        foreach (Notice notice in BackEndServerManager.instance.noticeList)
        {
            Populate(notice);
        }
    }

    GameObject newObj;
    void Populate(Notice notice)
    {
        newObj = (GameObject)Instantiate(prefab, noticeContent.transform);

        newObj.name = notice.GetHashCode().ToString();

        // Notice Title 출력
        //var texts = newObj.GetComponents<Text>();
        //var text = newObj.GetComponent<Text>();
        //text.text = notice.Title;

        // 접속 버튼
        Button button = newObj.GetComponentInChildren<Button>();
        button.GetComponentInChildren<Text>().text = notice.Title;
        button.onClick.AddListener(delegate { NoticePop.Instance().SetNotice(notice); });
    }

    private void RemoveAllListViewItem()
    {
        foreach (Transform child in noticeContent.transform)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
    }
}
