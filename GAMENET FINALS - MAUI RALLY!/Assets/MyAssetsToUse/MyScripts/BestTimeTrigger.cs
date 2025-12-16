using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestTimeTrigger : MonoBehaviour
{
    //public GameObject FinishLine;

    public GameObject bestMinuteBox;
    public GameObject bestSecondBox;
    public GameObject bestMilliBox;

    private int bestMinutes = int.MaxValue;
    private int bestSeconds = int.MaxValue;
    private float bestMillis = float.MaxValue;

    public int totalLaps = 2;   
    private int currentLap = 1; 
    private bool lapFinished = false;
    private bool touchedStartingLine = false;

    private RaceTimer raceTimer;
    private PrometeoCarController pcc;

    void Start()
    {
        raceTimer = GetComponent<RaceTimer>();

    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Triggered pero hindi by player");

        PrometeoCarController car = other.GetComponentInChildren<PrometeoCarController>();

        if (!touchedStartingLine)
        {

            if (lapFinished) return;

            Debug.Log("Finish line triggered by: " + other.name + " on lap " + currentLap);

            float currentTime = raceTimer.minuteCount * 60 + raceTimer.secondCount + raceTimer.milliCount / 10f;
            float bestTime = bestMinutes * 60 + bestSeconds + bestMillis / 10f;

            if (currentTime < bestTime)
            {
                bestMinutes = raceTimer.minuteCount;
                bestSeconds = raceTimer.secondCount;
                bestMillis = raceTimer.milliCount;

                bestMinuteBox.GetComponent<Text>().text = bestMinutes.ToString("00") + ".";
                bestSecondBox.GetComponent<Text>().text = bestSeconds.ToString("00") + ".";
                bestMilliBox.GetComponent<Text>().text = Mathf.FloorToInt(bestMillis).ToString("0");

                Debug.Log("New best time: " + bestMinutes + ":" + bestSeconds + "." + Mathf.FloorToInt(bestMillis));
            }

            raceTimer.ResetTimer();
            lapFinished = true;
            touchedStartingLine = false;

            if (currentLap < totalLaps)
            {
                currentLap++;
                lapFinished = false;
                Debug.Log("Lap " + (currentLap - 1) + " finished. Next lap: " + currentLap);
            }
            else
            {
                Debug.Log("Race finished!");
            }

        }
    }
}