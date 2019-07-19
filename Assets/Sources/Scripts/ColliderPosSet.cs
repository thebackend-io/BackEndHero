using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderPosSet : MonoBehaviour {

    private int width;
    private int height;

    public GameObject leftWall;
    public GameObject rightWall;
    public GameObject roof;

	void Start () {
        //왼쪽 벽 설정
        Vector3 pos = Camera.main.WorldToViewportPoint(leftWall.transform.position);
        pos.x = 0;
        leftWall.transform.position = Camera.main.ViewportToWorldPoint(pos);

        //오른쪽 벽 설정
        pos = Camera.main.WorldToViewportPoint(rightWall.transform.position);
        pos.x = 1;
        rightWall.transform.position = Camera.main.ViewportToWorldPoint(pos);

        //상단 벽 설정
        pos = Camera.main.WorldToViewportPoint(roof.transform.position);
        pos.y = 1;
        roof.transform.position = Camera.main.ViewportToWorldPoint(pos);
    }
}
