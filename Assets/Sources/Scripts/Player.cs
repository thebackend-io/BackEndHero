using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public bool isGrounded;
    public bool isLive;
    public Transform spriteObj;

    public Rigidbody myRgb;
    public Collider myCol;
    public AudioSource myAudio;

    public static event System.Action OnThrow = delegate {};

    private GameObject[] heroSprites = new GameObject[2];
    enum ImgIndex {live = 0, die = 1, max};

    void Awake()
    {
        if(this.transform.childCount < 2) {
            Debug.Log("Fail To Load Player Img");
        }
        for(ImgIndex index=ImgIndex.live; index<ImgIndex.max; ++index) {
            heroSprites[(int)index] = this.transform.GetChild((int)index).gameObject;
        }
    }
    void Update () {
        if (isLive)
        {
            if (transform.position.y < 1f && Physics.Raycast(transform.position, Vector3.down, 1f, 1 << LayerMask.NameToLayer("Ground")))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            if(transform.position.y < -10f) 
            {
                isLive = false;
            }
        }
        else
        {
            // 비정상적으로 플레이어가 맵 밖으로 나가는 경우
            // 바로 게임 종료
            if (transform.position.y < -20f)
            {
                myRgb.velocity = Vector3.zero;
                myRgb.isKinematic = true;
                GameManager.instance.GameOver ();
                transform.position = new Vector3(0, -10f, 0);
            }
        }

        if (isGrounded) {
            spriteObj.localRotation = Quaternion.identity;
        }
        else if(isLive){
            spriteObj.Rotate (0, 0, transform.localRotation.z + 720.0f * Time.deltaTime);
        }
    }
    
    void OnEnable () {
        GameManager.OnGameOver += OnGameOver;
        GameManager.OnGameReady += OnGameReady;
        GameManager.OnGameStart += OnGameStart;
    }

    void OnDisable () {
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnGameReady -= OnGameReady;
        GameManager.OnGameStart -= OnGameStart;
    }

    // ==================================================
    public void Throw (Vector3 _velocity) {
        if (isLive) {
            OnThrow ();
            myRgb.velocity = Vector3.zero;
            myRgb.AddForce (_velocity, ForceMode.Impulse);
            myAudio.Play ();
        }
    }

    public void Die () {
        myCol.isTrigger = true;
        Throw (new Vector3 (Random.Range(-1f, 1.1f), Random.Range(10f, 20f), 0));
        isLive = false;
        isGrounded = false;

        heroSprites[(int)ImgIndex.live].SetActive(false);
        heroSprites[(int)ImgIndex.die].SetActive(true);
    }

    void OnGameOver () {
        Die ();
    }

    void OnGameReady () {
        transform.position = Vector3.zero;
        myCol.isTrigger = false;
        myRgb.isKinematic = false;
        spriteObj.localRotation = Quaternion.identity;

        heroSprites[(int)ImgIndex.live].SetActive(true);
        heroSprites[(int)ImgIndex.die].SetActive(false);
    }

    void OnGameStart () {
        isLive = true;
    }

    public Vector3 GetPlayerPos()
    {
        return transform.position;
    }
}
