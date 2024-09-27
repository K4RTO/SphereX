using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    // Enum to differentiate between different cube types
    public enum CubeType { Speed, Frozen, Health, Damage, Ultimate }
    public CubeType cubeType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            // Apply the effect based on the cube type
            switch (cubeType)
            {
                case CubeType.Speed:
                    playerController.StartCoroutine(playerController.SpeedBoost());
                    break;

                case CubeType.Frozen:
                    playerController.StartCoroutine(playerController.FrozenBoost());
                    break;

                case CubeType.Health:
                    playerController.RecoverHealth(25);
                    break;

                case CubeType.Damage:
                    playerController.StartCoroutine(playerController.DamageBoost());
                    break;

                case CubeType.Ultimate:
                    playerController.StartCoroutine(playerController.UltimateBoost());
                    break;
            }

            // Destroy the cube after the effect is applied
            Destroy(gameObject);
        }
    }
    
}
