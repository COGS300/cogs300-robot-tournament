using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class CogsAgent : Agent
{
    // ------------------------VARIABLES----------------------------
    // public: direct access available for all classes
    // protected: direct access restricted to CogsAgent and MyAgent
    // private: direct access restricted to CogsAgent only;
    //          indirect access via Getters and Setters if they are provided

    protected Rigidbody rBody;
    protected GameObject timer, enemy, myBase;
    protected Transform baseLocation;  
    
    protected GameObject[] targets;
    //public List<GameObject> capturedTargets;
    protected List<GameObject> carriedTargets;

    protected Vector3 dirToGo, rotateDir;

    private const float maxMoveSpeed = 1;
    private const float maxTurnSpeed = 150;
    private int team;

    private const float m_LaserLength = 20;
    private GameObject myLaser;
    private bool m_Shoot, frozen, invincible;
    private float frozenTime,invinceTime, moveSpeed, turnSpeed;

    private static float frozenDuration = 3f;
    private static float invinceDuration = frozenDuration + 1f;

    protected Dictionary<string, float> rewardDict;



    // ----------------AGENT FUNCTIONS-----------------
   
    public override void OnEpisodeBegin()
    {
        rBody.velocity = Vector3.zero;

        if (team == 1)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, -125f, 0f));
        }
        else if (team == 2)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, 50f, 0f));
        }
        //capturedTargets.Clear();
        carriedTargets.Clear();
        frozen = false;
        invincible = false;

        transform.localPosition = new Vector3(baseLocation.localPosition.x, 0.5f, baseLocation.localPosition.z);
        // foreach (GameObject target in targets)
        // {
        //     target.GetComponent<Target>().resetGame();
        // }
        //myBase.GetComponent<HomeBase>().Reset();

    }



    // -----------------BASIC SETUP--------------------
    
    private void Freeze() {
        if (rewardDict.ContainsKey("frozen")) AddReward(rewardDict["frozen"]);
        if (!invincible){
            frozen = true;
            invincible = true;
            frozenTime = Time.time;
            invinceTime = Time.time;
        }
    }
    private void DropCarrying() {
        for (int i = carriedTargets.Count - 1; i > -1; i--)
            {
                GameObject currentTarget = carriedTargets[i];
                currentTarget.GetComponent<Target>().Explode();
                carriedTargets.Remove(currentTarget);

            }
    }



    // ---------------VIRTUAL FUNCTIONS-----------------
    // Used for setup as well as in MyAgent
    
    protected virtual void Start()
    {
        rBody = GetComponent<Rigidbody>();
        targets = GameObject.FindGameObjectsWithTag("Target");

        timer = GameObject.FindGameObjectWithTag("Timer");
        
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject agent in agents) {
            if (agent != gameObject) {
                enemy = agent;
            }
        }
        
        if (transform.parent.name == "Agent 1") {
            team = 1;
        }
        else team = 2;

        myBase = GameObject.Find("Base " + team);
        baseLocation = myBase.transform;

        myLaser = GameObject.Find("Laser " + team);

        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;
        //capturedTargets = new List<GameObject>();
        carriedTargets = new List<GameObject>();

        // Material mat;
        // mat = (Material) Resources.Load<Material>("AgentMat"); 
    
    }
    
    protected virtual void FixedUpdate() {
        moveSpeed = maxMoveSpeed - (0.05f * carriedTargets.Count);
        turnSpeed = maxTurnSpeed - (10f * carriedTargets.Count);


        if (rBody.velocity.sqrMagnitude > 25f) // slow it down
        {
            float maxVelocity = 0.95f - 0.03f * carriedTargets.Count;
            if (maxVelocity <= 0.6f) { maxVelocity = 0.6f; }
            rBody.velocity *= maxVelocity;

        }

        if (Time.time > frozenTime + frozenDuration && frozen)
        {
            frozen = false;
        }
        if (Time.time > invinceTime + invinceDuration && invincible){
            invincible = false;
        }

        int numCarry = 0;
        float xCo = this.transform.localPosition.x;
        float zCo = this.transform.localPosition.z;
        foreach (GameObject target in carriedTargets)
        {
            numCarry++;
            //target.GetComponent<Target>().SetYPos(numCarry * 1.2f);
            target.transform.localPosition = new Vector3(xCo, numCarry * 1.2f, zCo);
        }
    }
    
    protected virtual void OnTriggerEnter(Collider collision)
    {
        //If I collide with a target which is not in my own base
        
        // if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == team)
        // {
        //     for (int i = carriedTargets.Count - 1; i > -1; i--)
        //     {
        //         GameObject currentTarget = carriedTargets[i];
        //         //AddReward(0.1f);
        //         capturedTargets.Add(currentTarget);
        //         int spot = myBase.GetComponent<HomeBase>().addToFirstSpotInBase();
        //         Vector3 position = myBase.GetComponent<HomeBase>().getPosition(spot);
        //         currentTarget.GetComponent<Target>().addToBase(spot, team, position);
        //         currentTarget.GetComponent<Target>().zeroRotation();
        //         carriedTargets.Remove(currentTarget);

        //     }
        //     collision.gameObject.GetComponent<HomeBase>().numInBase = capturedTargets.Count;
        // }
    }

    // check collisions for walls and add a small negative reward
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != team && collision.gameObject.GetComponent<Target>().GetCarried() == 0 && !frozen)
        {
            //SetReward(0.5f);
            //carrying++;
            collision.gameObject.GetComponent<Target>().Carry(team);

            
            carriedTargets.Add(collision.gameObject);

            //if it is in my enemies base (not my base, and in a base)
            // if (collision.gameObject.GetComponent<Target>().inBase != 0)
            // {
            //     foreach (GameObject target in targets)
            //     {
            //         if (target.GetComponent<Target>().inBase != 0 && target.GetComponent<Target>().inBase != team)
            //         {
            //             target.GetComponent<Target>().adjustSpotInBase();
            //         }
            //     }
            // }
        }
        
    }


    
    // ---------------USEFUL HELPERS------------------
    // can be used in MyAgent
    
    // return true if laser succesfully activated, false otherwise
    protected bool LaserControl() {
        if (IsLaserOn() && !IsFrozen())
        {
            //AddReward(-0.05f);
            if (rewardDict.ContainsKey("shooting-laser")) AddReward(rewardDict["shooting-laser"]);
            var myTransform = transform;
            var rayDir = m_LaserLength * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            float laserHitDistance = m_LaserLength;
            if (Physics.SphereCast(transform.position, 0.25f, rayDir, out hit, m_LaserLength,1, QueryTriggerInteraction.Ignore))
            {
                laserHitDistance = hit.distance;
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    if (rewardDict.ContainsKey("hit-enemy")) AddReward(rewardDict["hit-enemy"]);
                    hit.collider.gameObject.GetComponent<CogsAgent>().DropCarrying();
                    hit.collider.gameObject.GetComponent<CogsAgent>().Freeze();
                }
            }
            myLaser.transform.localPosition = new Vector3(0f,0f,(laserHitDistance / 2f) + 0.5f);
            myLaser.transform.localScale = new Vector3(1f, 1f, laserHitDistance);
            return true;
        }
        else
        {
            myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
            myLaser.transform.localPosition = new Vector3(0f,0f,0f);

            return false;
        }
    }

    protected GameObject getNearestTarget(){
        float distance = 200;
        GameObject nearestTarget = null;
        foreach (var target in targets)
        {
            float currentDistance = Vector3.Distance(target.transform.localPosition, transform.localPosition);
            if (currentDistance < distance && target.GetComponent<Target>().GetCarried() == 0 && target.GetComponent<Target>().GetInBase() != team){
                distance = currentDistance;
                nearestTarget = target;
            }
        }
        return nearestTarget;
    }



    // --------------GETTERS----------------
    public int GetTeam() {return team;}     
    protected float GetMoveSpeed() {return moveSpeed;}
    protected float GetTurnSpeed() {return turnSpeed;}
    public bool IsLaserOn() {return m_Shoot;}
    public bool IsFrozen() {return frozen;}
    public float GetFrozenTime() {return frozenTime;}
    //public int GetCaptured() {return capturedTargets.Count;}
    public int GetCarrying() {return carriedTargets.Count;}
    public float DistanceToBase(){return Vector3.Distance(myBase.transform.localPosition, transform.localPosition);}

    public GameObject GetCarry(int i){
        return carriedTargets[i];
    }

    // --------------SETTERS----------------
    protected void SetLaser(bool on) {m_Shoot = on;}

    public void RemoveCarry(GameObject target){
        carriedTargets.Remove(target);
    }
    
}