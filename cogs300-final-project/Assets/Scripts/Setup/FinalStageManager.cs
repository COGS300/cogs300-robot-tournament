using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FinalStageManager : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isTraining = false;
    public Text winnerTextbox;
    public GameObject timer;

    public GameObject agent1;
    public GameObject agent2;
    public GameObject base1;
    public GameObject base2;
    public Text base1CountTxt;
    public Text base2CountTxt;


    GameObject[] targets;
    void Start()
    {
        targets = GameObject.FindGameObjectsWithTag("Target");

        winnerTextbox.enabled = false;
    }


    // Update is called once per frame
    void FixedUpdate()
    { 
        bool timerIsRunning = timer.GetComponent<Timer>().GetTimerIsRunning();
        
        int base1Num = agent1.GetComponent<MyAgent>().GetCaptured();
        int base2Num = agent2.GetComponent<MyAgent>().GetCaptured();
        int agent1Carry = agent1.GetComponent<MyAgent>().GetCarrying();
        int agent2Carry = agent2.GetComponent<MyAgent>().GetCarrying();
        float agent1BaseDist = agent1.GetComponent<MyAgent>().DistanceToBase();
        float agent2BaseDist = agent1.GetComponent<MyAgent>().DistanceToBase();

        base1CountTxt.text = WorldConstants.agent1ID + ": " + base1Num.ToString();
        base2CountTxt.text = WorldConstants.agent2ID + ": " + base2Num.ToString();
     
        if (!timerIsRunning)
        {
            if (base1Num > base2Num)
            {
                agent1.GetComponent<MyAgent>().SetReward(1f);
                agent2.GetComponent<MyAgent>().SetReward(-1f);
                Debug.Log("Agent 1 wins by capture");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (base2Num > base1Num)
            {
                agent1.GetComponent<MyAgent>().SetReward(-1f);
                agent2.GetComponent<MyAgent>().SetReward(1f);
                Debug.Log("Agent 2 wins by capture");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            else if (agent1Carry > agent2Carry)
            {
                agent1.GetComponent<MyAgent>().SetReward(1f);
                agent2.GetComponent<MyAgent>().SetReward(-1f);
                Debug.Log("Agent 1 wins by carry");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (agent2Carry > agent1Carry)
            {
                agent1.GetComponent<MyAgent>().SetReward(-1f);
                agent2.GetComponent<MyAgent>().SetReward(1f);
                Debug.Log("Agent 2 wins by carry");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            else if (agent1BaseDist < agent2BaseDist && agent1Carry != 0)
            {
                agent1.GetComponent<MyAgent>().SetReward(1f);
                agent2.GetComponent<MyAgent>().SetReward(-1f);
                Debug.Log("Agent 1 wins by distance");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (agent2BaseDist < agent1BaseDist && agent2Carry != 0)
            {
                agent1.GetComponent<MyAgent>().SetReward(-1f);
                agent2.GetComponent<MyAgent>().SetReward(1f);
                Debug.Log("Agent 2 wins by distance");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            
            else {
                agent1.GetComponent<MyAgent>().SetReward(0f);
                agent2.GetComponent<MyAgent>().SetReward(0f);
                Debug.Log("Draw!");

                winnerTextbox.enabled = true;
                winnerTextbox.text = "Draw";
            }

            if (isTraining) {
                Reset();
            }
            else {
                StopGame();
            }
            
        }
    }

    void Reset() {
        timer.GetComponent<Timer>().Reset();
        base1.GetComponent<HomeBase>().Reset();
        foreach (GameObject target in targets)
        {
            target.GetComponent<Target>().resetGame();
        }
        
        agent1.GetComponent<MyAgent>().EndEpisode();
        agent2.GetComponent<MyAgent>().EndEpisode();
    }

    void StopGame() {
        Time.timeScale = 0;
    }
}