using UnityEngine;
using System.Collections;

[System.Serializable]
public class WaveSpawner {

    public GameObject[] hazards;

    public Vector3 spawnLine;
    public int hazardCount;
    public float spawnDelay;
    public float startDelay;
    public float waveDelay;
}

[System.Serializable]
public class GameGui
{
    public GUIText scoreText;
    public GUIText restartText;
    public GUIText gameOverText;
    public string gameOverMessage;
    public string restartMessage;
}

public class GameController : MonoBehaviour {

    public GameGui gui;
    public WaveSpawner spawner;
    private int currentWave = 1;
    private int score = 0;
    private bool gameOver = false;
    private bool restart = false;

    public void Start()
    {
        UpdateScore();
        StartCoroutine (SpawnWaves());
        gui.restartText.text = string.Empty;
        gui.gameOverText.text = string.Empty;
    }

    public void Update()
    {
        if (restart == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
    }

    public IEnumerator SpawnWaves()
    {
        while(true) {
            yield return new WaitForSeconds(spawner.startDelay);
            Debug.Log(string.Format("Start wave {0}", currentWave));

            int maxHazards = (int)(spawner.hazardCount * Mathf.Sqrt(currentWave));
            for (int i = 0; i < maxHazards; i++)
            {
                float randomX = Random.Range(-spawner.spawnLine.x, spawner.spawnLine.x);
                Vector3 spawnPosition = new Vector3(randomX, spawner.spawnLine.y, spawner.spawnLine.z);
                Quaternion spawnRotation = Quaternion.identity;

                GameObject hazard = spawner.hazards[Random.Range(0, spawner.hazards.Length)];
                Instantiate(hazard, spawnPosition, spawnRotation);
                float randomDelay = Random.Range(0.5f * spawner.spawnDelay, 1.5f * spawner.spawnDelay);
                yield return new WaitForSeconds(randomDelay);
            }

            yield return new WaitForSeconds(spawner.waveDelay);
            currentWave++;

            if (gameOver == true)
            {
                gui.restartText.text = gui.restartMessage;
                restart = true;
                break;
            }
        }
    }

    private void UpdateScore()
    {
        gui.scoreText.text = string.Format("Score: {0}", score);
    }

    public void AddPointsToScore(int points)
    {
        score += points;
        UpdateScore();
    }

    public void GameOver()
    {
        gui.gameOverText.text = gui.gameOverMessage;
        gameOver = true;
    }
}
