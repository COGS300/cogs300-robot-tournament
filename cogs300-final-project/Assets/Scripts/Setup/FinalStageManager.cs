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
    GameObject[] players;

    CogsAgent agent1Script;
    CogsAgent agent2Script;

    void Start()
    {
        targets = GameObject.FindGameObjectsWithTag("Target");
        players = GameObject.FindGameObjectsWithTag("Player"); 

        agent1 = players[0];
        agent2 = players[1];


        winnerTextbox.enabled = false;
        agent1Script = agent1.GetComponent(agent1.name) as CogsAgent;
        agent2Script = agent2.GetComponent(agent2.name) as CogsAgent;
    }


    // Update is called once per frame
    void FixedUpdate()
    { 
        bool timerIsRunning = timer.GetComponent<Timer>().GetTimerIsRunning();
        
        //int agent2Carry = agent2.GetComponent<MyAgent>().GetCarrying(); -> cannot access this way anymore!!!

        int base1Num = base1.GetComponent<HomeBase>().GetCaptured();
        int base2Num = base2.GetComponent<HomeBase>().GetCaptured();
        int agent1Carry = agent1Script.GetCarrying();
        int agent2Carry = agent2Script.GetCarrying();

        float agent1BaseDist = agent1Script.DistanceToBase();
        float agent2BaseDist = agent2Script.DistanceToBase();

        base1CountTxt.text = WorldConstants.agent1ID + ": " + base1Num.ToString();
        base2CountTxt.text = WorldConstants.agent2ID + ": " + base2Num.ToString();
     
        if (!timerIsRunning)
        {
            if (base1Num > base2Num)
            {
                agent1Script.SetReward(1f);
                agent2Script.SetReward(-1f);
                Debug.Log("Agent 1 wins by capture");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (base2Num > base1Num)
            {
                agent1Script.SetReward(-1f);
                agent2Script.SetReward(1f);
                Debug.Log("Agent 2 wins by capture");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            else if (agent1Carry > agent2Carry)
            {
                agent1Script.SetReward(1f);
                agent2Script.SetReward(-1f);
                Debug.Log("Agent 1 wins by carry");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (agent2Carry > agent1Carry)
            {
                agent1Script.SetReward(-1f);
                agent2Script.SetReward(1f);
                Debug.Log("Agent 2 wins by carry");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            else if (agent1BaseDist < agent2BaseDist && agent1Carry != 0)
            {
                agent1Script.SetReward(1f);
                agent2Script.SetReward(-1f);
                Debug.Log("Agent 1 wins by distance");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (agent2BaseDist < agent1BaseDist && agent2Carry != 0)
            {
                agent1Script.SetReward(-1f);
                agent2Script.SetReward(1f);
                Debug.Log("Agent 2 wins by distance");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            
            else {
                agent1Script.SetReward(0f);
                agent2Script.SetReward(0f);
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
        base2.GetComponent<HomeBase>().Reset();
        foreach (GameObject target in targets)
        {
            target.GetComponent<Target>().ResetGame();
        }
        
        agent1Script.EndEpisode();
        agent2Script.EndEpisode();
    }

    void StopGame() {
        Time.timeScale = 0;
    }
}