using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timeRemaining;
    public float timeLimit;
    public bool timerIsRunning = true;
    public Text timerTextUI;
    private float minutes, seconds;
    private string timerText = "";


    public void reset(){
        timeRemaining = timeLimit;
        timerIsRunning = true;
    }

    void Start(){
        //reset();
        timeRemaining = timeLimit;
        timerIsRunning = true;
    }

    void Update()
    {
    
        if (timeRemaining > 0) {
            timeRemaining -= Time.deltaTime;
            minutes = Mathf.FloorToInt(timeRemaining / 60);
            seconds = Mathf.FloorToInt(timeRemaining % 60);
            if (Mathf.RoundToInt(seconds) < 10) {
                timerText = minutes + ":0" + seconds;
            }
            else {
                timerText = minutes + ":" + seconds;
            }
        }
        else {
            timerText = "Time's up!";
            timeRemaining = 0;
            timerIsRunning = false;
        }

        timerTextUI.text = timerText;
        
        
    }
}
