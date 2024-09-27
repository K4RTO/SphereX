using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
    public float damage;
    public bool isFreeze = false;
    private Transform target;
    public void SetTarget(Transform enemyTarget){
        target = enemyTarget;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null){
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if(Vector3.Distance(transform.position,target.position) <0.2f ){
            HitTarget();
        }
    }

    void HitTarget(){
        EnemyController enemy = target.GetComponent<EnemyController>();
        if(enemy != null){
            enemy.TakeDamage(damage);
                if(isFreeze){
                    enemy.Freeze(2f);
                }
            
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision){
        if (collision.gameObject == target.gameObject)
        {
            HitTarget();
        }
    }
}
