using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	public static bool isSpawning;
	[Space]
	[Header("Settings")]
	public GameObject enemyPrefab;
	public Vector2 spawnTimeRangeDefault = new Vector2 (1f, 1.6f);
	public Vector2 spawnTimeRangeLimit = new Vector2 (0.3f, 0.5f);
	public Vector2 levelUpSpawnTimeDecrease = new Vector2 (0.1f, 0.2f);


	float sideSpeed = 5.0f;
	int direction = 0;
    float spawnTimemin;
	float spawnTimemax;
	int spawnCount;


	float spawnTime = 0;

	void OnEnable () {
		GameManager.OnGameOver += OnGameOver;
		GameManager.OnGameReady += OnGameReady;
		GameManager.OnGameStart += OnGameStart;
		GameManager.OnLevelUp += LevelUpCheck;
		direction = Random.Range (0, 2);
		if (direction == 0) {
			direction = -1;
		}

		spawnTime = 3.0f;
	}

	void OnDisable () {
		GameManager.OnGameOver -= OnGameOver;
		GameManager.OnGameReady -= OnGameReady;
		GameManager.OnGameStart -= OnGameStart;
		GameManager.OnLevelUp -= LevelUpCheck;
	}
	
	void Update () {
		if (isSpawning) {
			if (transform.position.x <= -4.5) {
				direction = 1;
			} else if (transform.position.x >= 4.5) {
				direction = -1;
			}

			transform.Translate (Vector3.right * direction * sideSpeed * Time.deltaTime);

			spawnTime -= Time.deltaTime;
			if (spawnTime <= 0) {				
				SpawnEnemy ();				
				spawnTime = Random.Range (spawnTimemin, spawnTimemax);
			}
		}
	}

	// ==================================================
	void OnGameOver ()
	{
		isSpawning = false;
	}

	void OnGameReady ()
	{
		isSpawning = false;
	}

	void OnGameStart ()
	{
		spawnCount = 0;
		spawnTimemin = spawnTimeRangeDefault.x;
		spawnTimemax = spawnTimeRangeDefault.y;
		isSpawning = true;
	}
		
    private void SpawnEnemy ()
    {
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        ++spawnCount;
    }

	private void LevelUpCheck ()
	{
		spawnTimemin = Mathf.Clamp(spawnTimemin - levelUpSpawnTimeDecrease.x, spawnTimeRangeLimit.x, spawnTimeRangeDefault.x);
		spawnTimemax = Mathf.Clamp(spawnTimemax - levelUpSpawnTimeDecrease.y, spawnTimeRangeLimit.y, spawnTimeRangeDefault.y);	
	}
}
