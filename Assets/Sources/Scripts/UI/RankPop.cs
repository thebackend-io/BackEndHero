using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankPop : MonoBehaviour {
	[System.Serializable]
	public struct RankRow
	{
		public Text nickNameTxt;
		public Text scoreTxt;
		public Text rankTxt;
	}
	private RankRow[] top10RankRows = new RankRow[10];
    public GameObject rankList;
	public RankRow myRankRow;
    public Text TopN;
    private Color32 topRankcolor = new Color32(179, 88, 249, 255);
    private Color32 normalRankcolor = new Color32(100, 108, 224, 255);

    void Awake()
    {
        for(int i=1; i<11; ++i) {
            GameObject tmp = rankList.transform.GetChild(i).gameObject;
            top10RankRows[i-1].nickNameTxt = tmp.transform.GetComponentsInChildren<Text>()[1];
            top10RankRows[i-1].scoreTxt = tmp.transform.GetComponentsInChildren<Text>()[2];
            top10RankRows[i-1].rankTxt = tmp.transform.GetComponentsInChildren<Text>()[0];
        }
    }

    void OnEnable()
	{
		SetRank();
	}

	public void SetRank() {

        try
        {
            RankItem myRankData = BackEndServerManager.instance.myRankData;
            myRankRow.nickNameTxt.text = myRankData.nickname;
            myRankRow.scoreTxt.text = myRankData.score;
            myRankRow.rankTxt.text = myRankData.rank;
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            List<RankItem> rankTop10DataList = BackEndServerManager.instance.rankTop10DataList;
            for (int i = 0; i < rankTop10DataList.Count; i++)
            {
                top10RankRows[i].nickNameTxt.text = rankTop10DataList[i].nickname;
                top10RankRows[i].scoreTxt.text = rankTop10DataList[i].score;
                top10RankRows[i].rankTxt.text = rankTop10DataList[i].rank;

                top10RankRows[i].rankTxt.GetComponentInParent<Image>().color = int.Parse(rankTop10DataList[i].rank) < 4 ? (Color)topRankcolor : (Color)normalRankcolor;
                if( i == rankTop10DataList.Count -1)
                {
                    TopN.text = "Top " + rankTop10DataList[i].rank;
                }
            }


        }
	}
}
