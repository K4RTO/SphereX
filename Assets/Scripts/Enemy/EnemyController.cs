using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    private float health;
    private float moveSpeed;
    private GameObject player;
    private Rigidbody rigidbody;
    private bool isFrozen = false;
    private float freezeTime = 0f;
    private float enemyXp;
    private bool canMove = false; // New variable to control movement
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        rigidbody = GetComponent<Rigidbody>();
        EnemySet();
        rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        // Start the coroutine to delay the enemy movement
        StartCoroutine(MovementDelay(0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        if (isFrozen)
        {
            freezeTime -= Time.deltaTime;
            if (freezeTime < 0f)
            {
                UnFreeze(); 
            }
        }
        else if (canMove) // Check if the enemy is allowed to move
        {
            EnemyMove();
            if (Vector3.Distance(transform.position, player.transform.position) > 25.0f)
            {
                Die();
            } 
        }
        
    }
    // Coroutine to delay the movement
    private IEnumerator MovementDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canMove = true; // Allow the enemy to move after the delay
    }

    void EnemySet(){
        // Set different enemy
        if(gameObject.CompareTag("Enemy_Easy")){
            moveSpeed = 2.5f;
            health = 50.0f;
            enemyXp = 3f;
        }else if(gameObject.CompareTag("Enemy_Medium")){
            moveSpeed = 3.0f;
            health = 100.0f;
            enemyXp = 8f;
        }else if(gameObject.CompareTag("Enemy_Hard")){
            moveSpeed = 4.0f;
            health = 150.0f;
            enemyXp = 12f;
        }
    }
    void EnemyMove(){
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.Translate(moveSpeed * direction * Time.deltaTime );
    }

    public void TakeDamage(float damage){
        health -= damage;
        if(health <= 0){
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.GainXP(enemyXp);  // Player gains XP when the enemy dies
            Die();
        }
    }
    public void Freeze(float duration){
        if(!isFrozen){
            isFrozen = true;
            freezeTime = duration;
        }
    }
    public void UnFreeze(){
        isFrozen = false;
        freezeTime = 0f;
    }

    private void Die(){
        Destroy(gameObject);
    }
}
