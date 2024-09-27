using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private float health = 100.0f;
    private float moveSpeed = 6.0f;
    public GameObject Bullet;
    public float fireRate = 1f;
    public float shootingRadius = 10f;
    private float fireCooldown = 0f;
    private Rigidbody rigidbody;
    private Vector3 spawnPosition = new Vector3(0, 0, 0);
    private bool isFrozenBoostActive = false;  // Track if frozen boost is active
    // XP and level
    private float playerXP = 0f;
    private float playerLevel = 0f;

    // Upgrade limits
    private float maxSpeed = 8.0f;
    private float maxDamage = 50.0f;
    private float maxHealthIncrease = 50.0f;
    private float baseHealth = 100.0f;
    private float minFireRate = 0.5f; // Fire rate cannot be lower than this

    public TextMeshProUGUI healthText;  // Public reference to health text
    public TextMeshProUGUI xpText;  // Public reference to xp text
    public TextMeshProUGUI levelText;  // Public reference to level text
    public GameObject gameOverText;  // Public reference to Game Over text
    public GameObject returnButton;  // Public reference to Return Button

    // Upgrade Buttons
    public Button upgradeSpeedButton;
    public Button upgradeFireRateButton;
    public Button upgradeDamageButton;
    public Button upgradeHealthButton;

    private Queue<TextMeshProUGUI> cubeTextQueue = new Queue<TextMeshProUGUI>(); // Queue to manage cube texts
    private bool isDisplayingText = false; 

    // Text components for cubes
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI healthCubeText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI bulletText;
    public TextMeshProUGUI ultimateText;

    private Coroutine currentTextCoroutine = null;

    // Cube message display logic
    private IEnumerator DisplayCubeMessage(TextMeshProUGUI textComponent)
    {
        textComponent.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        textComponent.gameObject.SetActive(false);
    }

    private void HideAllCubeTexts()
    {
        speedText.gameObject.SetActive(false);
        healthCubeText.gameObject.SetActive(false);
        damageText.gameObject.SetActive(false);
        bulletText.gameObject.SetActive(false);
        ultimateText.gameObject.SetActive(false);
    }

    private void ShowCubeText(TextMeshProUGUI textComponent)
    {
        cubeTextQueue.Enqueue(textComponent);

        if (!isDisplayingText)
        {
            StartCoroutine(DisplayNextCubeText());
        }
    }

    void Start(){
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        // Hide Game Over UI elements at the start
        gameOverText.SetActive(false);
        returnButton.SetActive(false);

        // Hide upgrade buttons at the start
        HideUpgradeButtons();

        // Hide cube text elements at the start
        HideAllCubeTexts();


        UpdateHealthText();  // Update the health text at the start
        UpdateXPText();
        UpdateLevelText();  // Ensure level displays 0 at the start
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            ShootAtNearestEnemy();
            fireCooldown = 1f / fireRate;
        }
    }
    public void SetHealthText(TMPro.TextMeshProUGUI healthText)  // Add this method to set the health text
    {
        this.healthText = healthText;
        UpdateHealthText();  // Update the text immediately when it's set
    }

    // Player Movement
    void MovePlayer()
    {
        // Get input from the player for horizontal (A/D) and vertical (W/S) movement
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // Create a direction vector based on the input
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }

    public float GetHealth(){
        return health;
    }
    // Gain XP and Level Up
    public void GainXP(float xp)
    {
        playerXP += xp;
        Debug.Log("Player XP: " + playerXP);
        UpdateXPText();  // Update the XP text

        if (playerXP >= 50f)
        {
            LevelUp();
            playerXP = 0;
            UpdateXPText();  // Reset XP text after leveling up
        }
    }
    void ResetCubeEffects()
    {
        // Reset Speed Boost
        moveSpeed = 6.0f;  // Set back to default speed

        // Reset Frozen Boost
        isFrozenBoostActive = false;
        BulletController[] bullets = FindObjectsOfType<BulletController>();
        foreach (BulletController bullet in bullets)
        {
            bullet.isFreeze = false;
        }

        // Other resets as needed
    }

    private void LevelUp()
    {
        playerXP = 0f;  // Reset XP after leveling up
        playerLevel++;
        Debug.Log("Player Level: " + playerLevel);

        UpdateLevelText();  // Update the level text

        // Pause the game for 2 seconds
        StartCoroutine(LevelUpPause());
    }

    // In LevelUpPause method, updated logic to not pause if no buttons are active
    private IEnumerator LevelUpPause()
    {
        ResetCubeEffects();  // Remove all cube effects
        UpdateUpgradeButtons();  // Update and show the upgrade buttons

        if (AnyUpgradeButtonActive())  // Only pause if there are upgrade options available
        {
            Time.timeScale = 0f;  // Pause the game
            yield return new WaitUntil(() => !AnyUpgradeButtonActive());  // Wait until an upgrade is chosen
            Time.timeScale = 1f;  // Resume the game
        }
    }
    // Updated upgrade button logic to hide buttons when max values are reached
    private void UpdateUpgradeButtons()
    {
        // Show or hide upgrade buttons based on the current state
        if (moveSpeed < maxSpeed)
            upgradeSpeedButton.gameObject.SetActive(true);
        else
           upgradeSpeedButton.gameObject.SetActive(false);

        if (fireRate > minFireRate)
           upgradeFireRateButton.gameObject.SetActive(true);
        else
           upgradeFireRateButton.gameObject.SetActive(false);

        if (AnyBulletDamageBelowMax())
           upgradeDamageButton.gameObject.SetActive(true);
        else
           upgradeDamageButton.gameObject.SetActive(false);

        if (baseHealth < 100f + maxHealthIncrease)
          upgradeHealthButton.gameObject.SetActive(true);
        else
          upgradeHealthButton.gameObject.SetActive(false);

        // Add listeners to the buttons
        upgradeSpeedButton.onClick.AddListener(UpgradeSpeed);
        upgradeFireRateButton.onClick.AddListener(UpgradeFireRate);
        upgradeDamageButton.onClick.AddListener(UpgradeDamage);
        upgradeHealthButton.onClick.AddListener(UpgradeHealth);
    }

    // Check if any bullets have damage below max
    private bool AnyBulletDamageBelowMax()
    {
        BulletController[] bullets = FindObjectsOfType<BulletController>();
        foreach (BulletController bullet in bullets)
        {
            if (bullet.damage < maxDamage)
            {
                return true;
            }
        }
        return false;
    }

    private bool AnyUpgradeButtonActive()
    {
        return upgradeSpeedButton.gameObject.activeSelf ||
               upgradeFireRateButton.gameObject.activeSelf ||
               upgradeDamageButton.gameObject.activeSelf ||
               upgradeHealthButton.gameObject.activeSelf;
    }

    private void HideUpgradeButtons()
    {
        upgradeSpeedButton.gameObject.SetActive(false);
        upgradeFireRateButton.gameObject.SetActive(false);
        upgradeDamageButton.gameObject.SetActive(false);
        upgradeHealthButton.gameObject.SetActive(false);
    }

    // Upgrade options
    private void UpgradeSpeed()
    {
        if (moveSpeed < maxSpeed)
        {
            moveSpeed += 0.05f;
            Debug.Log("Speed increased to: " + moveSpeed);
        }
        HideUpgradeButtons();
    }

    private void UpgradeFireRate()
    {
        if (fireRate > 0.5f)
        {
            fireRate -= 0.05f;
         Debug.Log("Fire rate decreased to: " + fireRate);
        }
        HideUpgradeButtons();
    }

    private void UpgradeDamage()
    {
        BulletController[] bullets = FindObjectsOfType<BulletController>();
        foreach (BulletController bullet in bullets)
        {
            if (bullet.damage < maxDamage)
            {
                bullet.damage += 5f;
                Debug.Log("Damage increased to: " + bullet.damage);
            }
        }
        HideUpgradeButtons();
    }

    public void UpgradeHealth()
    {
        if (baseHealth < 100f + maxHealthIncrease)
        {
            baseHealth += 5f;
            health = baseHealth;  // Set current health to the new max
            Debug.Log("Health increased to: " + baseHealth);
            UpdateHealthText();
        }
        HideUpgradeButtons();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SpeedCube"))
        {
            ShowCubeText(speedText);
            StartCoroutine(SpeedBoost());
        }
        else if (other.gameObject.CompareTag("FrozenCube"))
        {
            ShowCubeText(bulletText);
            StartCoroutine(FrozenBoost());
        }
        else if (other.gameObject.CompareTag("HealthCube"))
        {
            ShowCubeText(healthCubeText);
            RecoverHealth(25);
        }
        else if (other.gameObject.CompareTag("DamageCube"))
        {
            ShowCubeText(damageText);
            StartCoroutine(DamageBoost());
        }
        else if (other.gameObject.CompareTag("UltimateCube"))
        {
            ShowCubeText(ultimateText);
            StartCoroutine(UltimateBoost());
        }

        Destroy(other.gameObject); // Destroy the cube after it's consumed
        DisplayNextCubeText(); // Display the next cube text if possible
    }

    private IEnumerator DisplayNextCubeText()
    {
        while (cubeTextQueue.Count > 0)
        {
            isDisplayingText = true;
            TextMeshProUGUI textComponent = cubeTextQueue.Dequeue();
            textComponent.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);  // Display each text for 2 seconds
            textComponent.gameObject.SetActive(false);
        }

        isDisplayingText = false;
    }

    public IEnumerator SpeedBoost()
    {
        moveSpeed += 0.5f;
        yield return new WaitForSeconds(5f);
        moveSpeed -= 0.5f;
    }

    public IEnumerator FrozenBoost()
    {
        isFrozenBoostActive = true;  // Activate frozen boost

        BulletController[] bullets = FindObjectsOfType<BulletController>();
        foreach (BulletController bullet in bullets)
        {
            bullet.isFreeze = true;
        }

        yield return new WaitForSeconds(8f);

        isFrozenBoostActive = false;  // Deactivate frozen boost

        foreach (BulletController bullet in bullets)
        {
            bullet.isFreeze = false;
        }
    }

    public void RecoverHealth(float amount)
    {
        health += amount;
        if (health > baseHealth) health = baseHealth; // Ensure health doesn't exceed the current maximum health
        Debug.Log("Health Recovered to " + health);
        UpdateHealthText();  // Update the health text
    }

    public IEnumerator DamageBoost()
    {
        BulletController[] bullets = FindObjectsOfType<BulletController>();
        foreach (BulletController bullet in bullets)
        {
            bullet.damage += 50f;
            Debug.Log("Damage increased to " + bullet.damage);
        }
        yield return new WaitForSeconds(8f);
        foreach (BulletController bullet in bullets)
        {
            bullet.damage -= 50f;
        }
    }

    public IEnumerator UltimateBoost()
    {
        moveSpeed += 1.0f;
        BulletController[] bullets = FindObjectsOfType<BulletController>();
        foreach (BulletController bullet in bullets)
        {
            bullet.isFreeze = true;
            bullet.damage += 50f;
        }
        yield return new WaitForSeconds(10f);
        moveSpeed -= 1.0f;
        foreach (BulletController bullet in bullets)
        {
            bullet.isFreeze = false;
            bullet.damage -= 50f;
        }
    }

    void ShootAtNearestEnemy()
    {
        // Find the nearest enemy within the specified radius
        GameObject nearestEnemy = FindNearestEnemyInRadius();
        if (nearestEnemy == null) return;

        // Instantiate the bullet at the player's position
        GameObject bullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();

        // Set the target of the bullet to the nearest enemy
        bulletController.SetTarget(nearestEnemy.transform);

        if (isFrozenBoostActive)
        {
            bulletController.isFreeze = true;  // Apply frozen boost to the new bullet
        }
    }

    GameObject FindNearestEnemyInRadius()
    {
        // Find all enemies with the relevant tags
        GameObject[] easyEnemies = GameObject.FindGameObjectsWithTag("Enemy_Easy");
        GameObject[] mediumEnemies = GameObject.FindGameObjectsWithTag("Enemy_Medium");
        GameObject[] hardEnemies = GameObject.FindGameObjectsWithTag("Enemy_Hard");

        // Combine all enemies into one list
        List<GameObject> allEnemies = new List<GameObject>();
        allEnemies.AddRange(easyEnemies);
        allEnemies.AddRange(mediumEnemies);
        allEnemies.AddRange(hardEnemies);

        // Initialize variables to find the nearest enemy
        GameObject nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        // Loop through all enemies to find the nearest one within the shooting radius
        foreach (GameObject enemy in allEnemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= shootingRadius)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy_Easy"))
        {
            TakeDamage(10.0f);
        }
        else if (collision.gameObject.CompareTag("Enemy_Medium"))
        {
            TakeDamage(20.0f);
        }
        else if (collision.gameObject.CompareTag("Enemy_Hard"))
        {
            TakeDamage(30.0f);
        }
    }

    void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0; // Ensure health doesn't go below 0
            UpdateHealthText();  // Update the health text to show 0
            PlayerDie();  // Handle player death
        }
        else
        {
            UpdateHealthText();  // Update the health text with current health
        }
    }

    void UpdateHealthText()  // Add this method to update the health text
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health;
        }
    }

    void UpdateXPText()
    {
        if (xpText != null)
        {
            xpText.text = "XP: " + playerXP;
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + playerLevel;
        }
    }
    void PlayerDie()
    {
        Debug.Log("Player Die");
        // Transport the player to the default position
        transform.position = new Vector3(0, 0, 0);  // Default position
        Time.timeScale = 0f;  // Pause the game

        // Destroy all enemies and cubes
        DestroyAllEnemiesAndCubes();

        // Show Game Over UI elements
        gameOverText.SetActive(true);
        returnButton.SetActive(true);
    }

    void DestroyAllEnemiesAndCubes()
    {
        // Destroy all enemies with the relevant tags
        string[] enemyTags = { "Enemy_Easy", "Enemy_Medium", "Enemy_Hard" };
        foreach (string tag in enemyTags)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

        // Destroy all cubes with the specific tags
    string[] cubeTags = { "SpeedCube", "FrozenCube", "HealthCube", "DamageCube", "UltimateCube" };
    foreach (string tag in cubeTags)
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject cube in cubes)
        {
            Destroy(cube);
        }
    }
    }
}