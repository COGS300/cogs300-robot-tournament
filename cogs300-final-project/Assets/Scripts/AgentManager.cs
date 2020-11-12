﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject agent1;
    public GameObject agent2;

    //Timer for the game in seconds. At end, winner is chosen, game resets.
    public float maxTime = 120f;
    public float timer;
    GameObject[] targets;
    void Start()
    {
        targets = GameObject.FindGameObjectsWithTag("Target");
        timer = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        // captured = MyBase.GetComponent<HomeBase>().numInBase;
        // int enemyCaptured = enemy.GetComponent<AgentController>().getCaptured();
        int agent1Captured = agent1.GetComponent<AgentController>().getCaptured();
        int agent1Carrying = agent1.GetComponent<AgentController>().getCarrying();
        int agent2Captured = agent2.GetComponent<AgentController>().getCaptured();
        int agent2Carrying = agent2.GetComponent<AgentController>().getCarrying();

        if (timer > maxTime)
        {
            timer = 0f;
            if (agent1Captured > agent2Captured)
            {
                agent1.GetComponent<AgentController>().SetReward(1f);
                agent2.GetComponent<AgentController>().SetReward(-1f);
                agent1.GetComponent<AgentController>().EndEpisode();
                agent2.GetComponent<AgentController>().EndEpisode();
                Debug.Log("Agent 1 wins by capture");
            }
            else if (agent2Captured > agent1Captured)
            {
                agent1.GetComponent<AgentController>().SetReward(-1f);
                agent2.GetComponent<AgentController>().SetReward(1f);
                agent1.GetComponent<AgentController>().EndEpisode();
                agent2.GetComponent<AgentController>().EndEpisode();
                Debug.Log("Agent 2 wins by capture");
            }
            else if (agent1Carrying > agent2Carrying)
            {
                agent1.GetComponent<AgentController>().SetReward(1f);
                agent2.GetComponent<AgentController>().SetReward(-1f);
                agent1.GetComponent<AgentController>().EndEpisode();
                agent2.GetComponent<AgentController>().EndEpisode();
                Debug.Log("Agent 1 wins by carry");
            }
            else if (agent2Carrying > agent1Carrying)
            {
                agent1.GetComponent<AgentController>().SetReward(-1f);
                agent2.GetComponent<AgentController>().SetReward(1f);
                agent1.GetComponent<AgentController>().EndEpisode();
                agent2.GetComponent<AgentController>().EndEpisode();
                Debug.Log("Agent 2 wins by carry");
            }
            else if (agent1Captured == agent2Captured){
                agent1.GetComponent<AgentController>().SetReward(0f);
                agent2.GetComponent<AgentController>().SetReward(0f);
                agent1.GetComponent<AgentController>().EndEpisode();
                agent2.GetComponent<AgentController>().EndEpisode();
                Debug.Log("Draw!");
            }
        }
    }
}
