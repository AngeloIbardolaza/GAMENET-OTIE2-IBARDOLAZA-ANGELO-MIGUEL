using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillFloor : MonoBehaviour
{
    private void Awake()
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("U R DEAD");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}