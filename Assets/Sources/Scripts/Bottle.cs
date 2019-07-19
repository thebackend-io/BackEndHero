using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Bottle : MonoBehaviour {

	public bool isDied = false;

	public Sprite bottleLiveSpr;
	public Sprite bottleDieSpr;
	public SpriteRenderer bottleSprRen;
	public Rigidbody bottleRgb;
	public Collider bottleCol;
	public Rigidbody corkRgb;
	public AudioSource myAudio;


	Vector2 dropSpeedRange = new Vector2 (0.5f, 2.5f);
	Vector2 sideSpeedRange = new Vector2 (0, 3.1f);
	
	float dropSpeed;
	float sideSpeed;
	int direction = 0;

	void Start () {
		isDied = false;
		bottleRgb.isKinematic = true;
		bottleCol.isTrigger = true;
		corkRgb.isKinematic = true;
	}

	void Update () {
		if (!isDied) {
			if (transform.position.x <= -5) {
				direction = 1;
			} else if (transform.position.x >= 5) {
				direction = -1;
			}
			
			transform.Translate (Vector3.down * dropSpeed * Time.deltaTime);
			transform.Translate (Vector3.right * direction * sideSpeed * Time.deltaTime);


            if (transform.position.y < 1f && Physics.Raycast(transform.position, Vector3.down, 1f, 1 << LayerMask.NameToLayer("Ground")))
            {
                GameManager.instance.GameOver ();
            }

        }
	}

	void OnEnable () {
		GameManager.OnGameOver += OnGameOver;
		dropSpeed = Random.Range (dropSpeedRange.x, dropSpeedRange.y) * GameManager.instance.levelSpeed;
		sideSpeed = Random.Range (sideSpeedRange.x, sideSpeedRange.y) * GameManager.instance.levelSpeed;
		direction = Random.Range (0, 2);
		if (direction == 0) {
			direction = -1;
		}
		isDied = false;
		bottleSprRen.sprite = bottleLiveSpr;
		bottleRgb.isKinematic = true;
		bottleCol.isTrigger = true;		
		bottleRgb.velocity = Vector3.zero;
		bottleRgb.angularVelocity = Vector3.zero;
		bottleRgb.transform.localRotation = Quaternion.identity;
		corkRgb.isKinematic = true;
		corkRgb.velocity = Vector3.zero;
		corkRgb.angularVelocity = Vector3.zero;
		corkRgb.transform.localRotation = Quaternion.identity;
        corkRgb.gameObject.SetActive(false);

    }

	void OnDisable () {
		GameManager.OnGameOver -= OnGameOver;
	}

	void OnTriggerEnter (Collider _col) {
		if (!isDied) {
			if (_col.CompareTag ("Player")) {
				Die ();
				myAudio.Play ();
				GameManager.instance.AddScore ();
                CameraShaker.Instance.ShakeByPower (1f);
			}
		}
	}
	
	// ==================================================
	void OnGameOver ()
	{
		Die ();
	}

	void Die () 
	{
		isDied = true;
		bottleSprRen.sprite = bottleDieSpr;
		bottleRgb.isKinematic = false;
        corkRgb.gameObject.SetActive(true);
        corkRgb.transform.parent = null;
		corkRgb.isKinematic = false;
		corkRgb.AddForce (new Vector3 (Random.Range(-5f, 5.1f), Random.Range(10f, 20f), 0), ForceMode.Impulse);
		corkRgb.AddTorque (new Vector3 (0, 0, Random.Range(-10f, 10.1f)), ForceMode.Impulse);
		bottleRgb.AddForce (new Vector3 (Random.Range(-5f, 5.1f), Random.Range(10f, 20f), 0), ForceMode.Impulse);
		bottleRgb.AddTorque (new Vector3 (0, 0, Random.Range(-10f, 10.1f)), ForceMode.Impulse);

        // 스마트폰 진동
#if UNITY_IOS || UNITY_ANDROID
		if(GameManager.instance.isVibrateOn)
		{
			Handheld.Vibrate();
		}
#endif

        Destroy(corkRgb.gameObject, 2f);
		Destroy(gameObject, 2f);
	}
}
