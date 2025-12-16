using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag("Player")) return;

        Debug.Log("Speed Boost Picked Up");

        PrometeoCarController car = other.GetComponentInChildren<PrometeoCarController>();
        if (car == null)
        {
            Debug.LogWarning("No PrometeoCarController found on player!");
            return;
        }

        StartCoroutine(ApplySpeedBoost(car));

        Destroy(gameObject);
    }

    private IEnumerator ApplySpeedBoost(PrometeoCarController car)
    {
        float originalMaxSpeed = car.maxSpeed;
        float originalAcceleration = car.accelerationMultiplier;

        car.maxSpeed = (int)(car.maxSpeed * 1.5f);
        car.accelerationMultiplier = (int)(car.accelerationMultiplier * 1.5f);

        Debug.Log("Speed Boost On");

        yield return new WaitForSeconds(boostDuration);

        car.maxSpeed = (int)originalMaxSpeed;
        car.accelerationMultiplier = originalAcceleration;

        Debug.Log("Speed Boost Off");
    }
}
