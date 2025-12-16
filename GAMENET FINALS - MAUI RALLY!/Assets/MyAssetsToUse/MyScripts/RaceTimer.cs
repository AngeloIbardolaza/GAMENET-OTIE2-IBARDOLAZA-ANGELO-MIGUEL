using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceTimer : MonoBehaviour
{
    public int minuteCount;
    public int secondCount;
    public float milliCount;
    public string milliDisplay;

    public GameObject minuteBox;
    public GameObject secondBox;
    public GameObject milliBox;

    void Start()
    {
        minuteCount = 0;
        secondCount = 0;
        milliCount = 0;

        minuteBox.GetComponent<Text>().text = "00.";
        secondBox.GetComponent<Text>().text = "00.";
        milliBox.GetComponent<Text>().text = "0";
    }
    void Update()
    {
        milliCount += Time.deltaTime * 10;
        milliDisplay = milliCount.ToString("F0");
        milliBox.GetComponent<Text>().text = " " + milliDisplay;

        if (milliCount >= 10)
        {
            milliCount = 0;
            secondCount += 1;
        }

        if (secondCount <= 9)
        {
            secondBox.GetComponent<Text>().text = "0" + secondCount + ".";
        }
        else
        {
            secondBox.GetComponent<Text>().text = "" + secondCount + ".";
        }

        if (secondCount >= 60)
        {
            secondCount = 0;
            minuteCount += 1;
        }

        if (minuteCount <= 9)
        {
            minuteBox.GetComponent<Text>().text = "0" + minuteCount + ".";
        }
        else
        {
            minuteBox.GetComponent<Text>().text = "" + minuteCount + ".";
        }
    }

    public void ResetTimer()
    {
        minuteCount = 0;
        secondCount = 0;
        milliCount = 0f;
    }
}
