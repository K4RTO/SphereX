using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyEasy;
    public GameObject enemyMedium;
    public GameObject enemyHard;
    public GameObject speedCube;
    public GameObject frozenCube;
    public GameObject healthCube;
    public GameObject damageCube;
    public GameObject ultimateCube;
    public GameObject player;
    public int totalEnemies;
    public float spawnRadius = 30.0f;
    public float cubeSpawnInterval = 5.0f; // Interval between cube spawns
    private List<GameObject> enemyPrefabs = new List<GameObject>(); 
    private GameObject currentCube;
    private float cubeSpawnTimer;
    private float timeSinceLastCheck;
    private float timeSinceLastEnemyIncrease; // Timer for increasing enemies over time

    private int maxEnemiesAllowed; // Maximum enemies allowed based on difficulty
    private int enemiesIncreaseRate; // Enemies increase rate based on difficulty

    // UI Elements
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button returnButton;
    public TextMeshProUGUI titleText; 
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI gameOverText;

    private bool gameStarted = false;

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ResetUI();
    }

    void ResetUI()
    {
        easyButton.gameObject.SetActive(true);
        mediumButton.gameObject.SetActive(true);
        hardButton.gameObject.SetActive(true);
        titleText.gameObject.SetActive(true);
        healthText.gameObject.SetActive(false);
        xpText.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        gameStarted = false;
    }

    void Start()
    {
        easyButton.onClick.AddListener(() => SetDifficulty(5, 1, 15));
        mediumButton.onClick.AddListener(() => SetDifficulty(8, 2, 20));
        hardButton.onClick.AddListener(() => SetDifficulty(12, 3, 25));

        SetUI();    
        gameStarted = false;

        returnButton.GetComponent<Button>().onClick.AddListener(ReturnToMainMenu);
    }

    void SpawnEnemies()
    {
        if (!gameStarted) return;

        enemyPrefabs.Add(enemyEasy);
        enemyPrefabs.Add(enemyMedium);
        enemyPrefabs.Add(enemyHard);

        // Initial spawn of enemies
        for (int i = 0; i < totalEnemies; i++)
        {
            if (i < 3)
            {
                SpawnSpecificEnemy(enemyPrefabs[i]);
            }
            else
            {
                SpawnRandomEnemy();
            }   
        }

        timeSinceLastCheck = 0f;
        cubeSpawnTimer = cubeSpawnInterval;
        timeSinceLastEnemyIncrease = 0f; // Initialize enemy increase timer
    }

    void SetUI()
    {
        easyButton.gameObject.SetActive(true);
        mediumButton.gameObject.SetActive(true);
        hardButton.gameObject.SetActive(true);
        titleText.gameObject.SetActive(true);
        healthText.gameObject.SetActive(false); 
        xpText.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
    }

    public void SetDifficulty(int initialEnemyCount, int increaseRate, int maxEnemies)
    {
        totalEnemies = initialEnemyCount;
        enemiesIncreaseRate = increaseRate;
        maxEnemiesAllowed = maxEnemies;

        easyButton.gameObject.SetActive(false);
        mediumButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        titleText.gameObject.SetActive(false);
        healthText.gameObject.SetActive(true); 
        xpText.gameObject.SetActive(true);
        levelText.gameObject.SetActive(true);

        StartCoroutine(DisplayHintText());
        gameStarted = true;
        SpawnCube();
        SpawnEnemies();
    }

    IEnumerator DisplayHintText()
    {
        hintText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        hintText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameStarted) return;

        timeSinceLastCheck += Time.deltaTime;
        cubeSpawnTimer -= Time.deltaTime;
        timeSinceLastEnemyIncrease += Time.deltaTime; // Increment enemy increase timer

        if (timeSinceLastCheck >= 0.5f)
        {
            EnsureEnemyPresence();
            timeSinceLastCheck = 0f;
        }

        if (cubeSpawnTimer <= 0f)
        {
            SpawnCube();
            cubeSpawnTimer = cubeSpawnInterval;
        }

        // Increase enemy count every minute based on difficulty
        if (timeSinceLastEnemyIncrease >= 60f)
        {
            IncreaseEnemyCount();
            timeSinceLastEnemyIncrease = 0f; // Reset timer
        }
    }

    void IncreaseEnemyCount()
    {
        // Increase the total number of enemies based on difficulty
        totalEnemies = Mathf.Min(totalEnemies + enemiesIncreaseRate, maxEnemiesAllowed);
    }

    void EnsureEnemyPresence()
    {
        if (!gameStarted) return;

        EnsureSpecificEnemyPresence(enemyEasy, "Enemy_Easy");
        EnsureSpecificEnemyPresence(enemyMedium, "Enemy_Medium");
        EnsureSpecificEnemyPresence(enemyHard, "Enemy_Hard");

        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy_Easy").Length +
                                GameObject.FindGameObjectsWithTag("Enemy_Medium").Length +
                                GameObject.FindGameObjectsWithTag("Enemy_Hard").Length;

        while (currentEnemyCount < totalEnemies)
        {
            SpawnRandomEnemy();
            currentEnemyCount++;
        }
    }

    void EnsureSpecificEnemyPresence(GameObject enemyPrefab, string enemyTag)
    {
        if (!gameStarted) return;

        if (GameObject.FindGameObjectsWithTag(enemyTag).Length == 0)
        {
            SpawnSpecificEnemy(enemyPrefab);
        }
    }

    void SpawnSpecificEnemy(GameObject enemyPrefab)
    {
        if (!gameStarted) return;

        Vector3 spawnPosition = GetRandomPosition();
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    void SpawnRandomEnemy()
    {
        if (!gameStarted || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Enemy Prefabs list is empty or game has not started! Cannot spawn enemies.");
            return;
        }

        GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Vector3 spawnPosition = GetRandomPosition();
        Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity);
    }

    void SpawnCube()
    {
        if (!gameStarted) return;

        if (currentCube != null)
        {
            Destroy(currentCube);
        }

        GameObject cubeToSpawn1 = GetRandomCube();
        Vector3 spawnPosition1 = GetRandomPosition();
        GameObject spawnedCube1 = Instantiate(cubeToSpawn1, spawnPosition1, Quaternion.identity);
        StartCoroutine(DestroyCubeAfterTime(spawnedCube1, 8f));

        GameObject cubeToSpawn2 = GetRandomCube();
        Vector3 spawnPosition2 = GetRandomPosition();
        GameObject spawnedCube2 = Instantiate(cubeToSpawn2, spawnPosition2, Quaternion.identity);
        StartCoroutine(DestroyCubeAfterTime(spawnedCube2, 8f));
    }

    IEnumerator DestroyCubeAfterTime(GameObject cube, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cube != null)
        {
            Destroy(cube);
        }
    }

    GameObject GetRandomCube()
    {
        float playerHealth = player.GetComponent<PlayerController>().GetHealth();
        float randomValue = Random.value * 100f;

        if (playerHealth <= 50)
        {
            if (randomValue < 50) return healthCube;
            else if (randomValue < 65) return speedCube;
            else if (randomValue < 80) return frozenCube;
            else if (randomValue < 95) return damageCube;
            else return ultimateCube;
        }
        else
        {
            if (randomValue < 25) return speedCube;
            else if (randomValue < 50) return frozenCube;
            else if (randomValue < 75) return damageCube;
            else if (randomValue < 90) return healthCube;
            else return ultimateCube;
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(
            player.transform.position.x + randomPoint.x, 
            player.transform.position.y, 
            player.transform.position.z + randomPoint.y
        );
        return spawnPosition;
    }
}