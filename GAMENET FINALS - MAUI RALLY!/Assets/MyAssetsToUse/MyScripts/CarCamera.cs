using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public Transform carModel;
    public float CarX;
    public float CarY;
    public float CarZ;

    void Update()
    {
        CarX = carModel.transform.eulerAngles.x;
        CarY = carModel.transform.eulerAngles.y;
        CarZ = carModel.transform.eulerAngles.z;

        transform.eulerAngles = new Vector3 (CarX - CarX, CarY - CarY, CarZ - CarZ);
    }
    void LateUpdate()
    {
        Vector3 camEuler = transform.eulerAngles;
        camEuler.y = carModel.eulerAngles.y + 90f; 
        transform.eulerAngles = camEuler;
    }

}
