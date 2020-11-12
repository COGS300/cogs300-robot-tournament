using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class AgentController : Agent
{
    //public GameObject timer
   
    Rigidbody rBody;

    public GameObject timer;
    public GameObject enemy;
    public GameObject MyBase;
    public float maxMoveSpeed = 1;
    public float maxTurnSpeed = 150;
    public int team;
    public Transform baseLocation;

    GameObject[] targets;
    public List<GameObject> capturedTargets = new List<GameObject>();
    public List<GameObject> carriedTargets = new List<GameObject>();

    public GameObject myLaser;
    bool m_Shoot;
    public float m_LaserLength = 10;

    private bool frozen;
    float frozenTime;

    Vector3 dirToGo = Vector3.zero;
    Vector3 rotateDir = Vector3.zero;

    float moveSpeed;
    float turnSpeed;


    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        targets = GameObject.FindGameObjectsWithTag("Target");
    }


    public int getCaptured()
    {
        return capturedTargets.Count;
    }

    public int getCarrying()
    {
        return carriedTargets.Count;
    }

    public void dropCarrying(){
        for (int i = carriedTargets.Count - 1; i > -1; i--)
            {
                GameObject currentTarget = carriedTargets[i];
                currentTarget.GetComponent<Target>().Carry(0);
                currentTarget.GetComponent<Target>().setMass(1);
                currentTarget.GetComponent<Target>().explode();
                carriedTargets.Remove(currentTarget);

            }
    }
    
    public void freeze(){
        AddReward(-0.1f);
        frozen = true;
        frozenTime = Time.time;
    }

    public float distanceToBase(){
        return Vector3.Distance(MyBase.transform.localPosition, transform.localPosition);
    }



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
        capturedTargets.Clear();
        carriedTargets.Clear();
        frozen = false;

        transform.localPosition = new Vector3(baseLocation.localPosition.x, 0.5f, baseLocation.localPosition.z);
        foreach (GameObject target in targets)
        {
            target.GetComponent<Target>().resetGame();
        }
        MyBase.GetComponent<HomeBase>().reset();
        timer.GetComponent<Timer>().reset();

    }

    // Get relevant information from the environment to effectively learn behavior
    public override void CollectObservations(VectorSensor sensor)
    {
        //Target and Agent positions
        var localVelocity = transform.InverseTransformDirection(rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);

        sensor.AddObservation(timer.GetComponent<Timer>().timeRemaining);

        var localRotation = transform.rotation;
        sensor.AddObservation(transform.rotation.y);

        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(baseLocation.localPosition);

        foreach (GameObject target in targets){
            sensor.AddObservation(target.transform.localPosition);
            sensor.AddObservation(target.GetComponent<Target>().carried);
            sensor.AddObservation(target.GetComponent<Target>().inBase);
        }

        sensor.AddObservation(enemy.transform.localPosition);
        sensor.AddObservation(enemy.transform.rotation.y);
        sensor.AddObservation(enemy.GetComponent<AgentController>().frozen);

        sensor.AddObservation(frozen);
    }

    // What to do when an action is received (i.e. when the Brain gives the agent information about possible actions)
    public override void OnActionReceived(float[] act)
    {
        AddReward(-0.0005f);
        if (Time.time > frozenTime + 4f && frozen)
        {
            frozen = false;
        }

            int numCarry = 0;
            float xCo = this.transform.localPosition.x;
            float zCo = this.transform.localPosition.z;
            foreach (GameObject target in carriedTargets)
            {
                numCarry++;
                target.transform.localPosition = new Vector3(xCo, numCarry * 1.2f, zCo);
            }

            movePlayer((int)act[0], (int)act[1], (int)act[2], (int)act[3], (int)act[4]);

    }

    void movePlayer(int forwardAxis, int rotateAxis, int shootAxis, int goToTargetAxis, int goToBaseAxis)
    {
        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;

        moveSpeed = maxMoveSpeed - (0.05f * carriedTargets.Count);
        turnSpeed = maxTurnSpeed - (10f * carriedTargets.Count);


        var shootCommand = false;
         m_Shoot = false;

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward;
                break;
            case 2:
                dirToGo = -transform.forward;
                break;
        }
        switch (rotateAxis)
        {
            case 1:
                rotateDir = -transform.up;
                break;
            case 2:
                rotateDir = transform.up;
                break;
        }


        switch (shootAxis)
        {
            case 1:
                shootCommand = true;
                break;
        }
        if (shootCommand)
        {
            m_Shoot = true;
        }
         switch (goToTargetAxis)
        {
            case 1:
                goToNearestTarget(moveSpeed,turnSpeed);
            break;
        }

         switch (goToBaseAxis)
        {
            case 1:
                goToBase(moveSpeed,turnSpeed);
            break;
        }

        if (m_Shoot && !frozen)
        {
            //AddReward(-0.05f);
            var myTransform = transform;
            myLaser.transform.localPosition = new Vector3(0f,0f,(m_LaserLength / 2f) + 0.5f);
            myLaser.transform.localScale = new Vector3(1f, 1f, m_LaserLength);
            var rayDir = 25.0f * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 2f, rayDir, out hit, m_LaserLength))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    AddReward(0.5f);
                    hit.collider.gameObject.GetComponent<AgentController>().dropCarrying();
                    hit.collider.gameObject.GetComponent<AgentController>().freeze();
                }
            }
        }
        else
        {
            myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
            myLaser.transform.localPosition = new Vector3(0f,0f,0f);

        }

        if(!frozen){
            if ( shootCommand == false){
                rBody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            }
            transform.Rotate(rotateDir, Time.deltaTime * turnSpeed);
        }
        if (rBody.velocity.sqrMagnitude > 25f) // slow it down
        {
            float maxVelocity = 0.95f - 0.03f * carriedTargets.Count;
            if (maxVelocity <= 0.6f) { maxVelocity = 0.6f; }
            rBody.velocity *= maxVelocity;

        }
    }

    GameObject getNearestTarget(){
        float distance = 200;
        GameObject nearestTarget = null;
        foreach (var target in targets)
        {
            float currentDistance = Vector3.Distance(target.transform.localPosition, transform.localPosition);
            if (currentDistance < distance && target.GetComponent<Target>().carried == 0 && target.GetComponent<Target>().inBase != team){
                distance = currentDistance;
                nearestTarget = target;
            }
        }
        return nearestTarget;
    }

    void goToBase(float moveSpeed, float turnSpeed){
        turnAndGo(moveSpeed, turnSpeed, getYAngle(MyBase));
    }

    void goToNearestTarget(float moveSpeed, float turnSpeed){
        GameObject target = getNearestTarget();
        if (target != null){
            float rotation = getYAngle(target);
            turnAndGo(moveSpeed, turnSpeed, rotation);
        }        
    }

    void turnAndGo(float moveSpeed, float turnSpeed, float rotation){

        if(rotation < -5f){
            rotateDir = transform.up;
        }
        else if (rotation > 5f){
            rotateDir = -transform.up;
        }
        else {
            dirToGo = transform.forward;
        }
    }

    private float getYAngle(GameObject target) {
        
       Vector3 targetDir = target.transform.position - transform.position;
       Vector3 forward = transform.forward;

      float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
      return angle; 
        
    }

    void OnTriggerEnter(Collider collision)
    {
        //If I collide with a target which is not in my own base
        
        if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == team)
        {
            for (int i = carriedTargets.Count - 1; i > -1; i--)
            {
                GameObject currentTarget = carriedTargets[i];
                AddReward(0.1f);
                capturedTargets.Add(currentTarget);
                int spot = MyBase.GetComponent<HomeBase>().addToFirstSpotInBase();
                Vector3 position = MyBase.GetComponent<HomeBase>().getPosition(spot);
                currentTarget.GetComponent<Target>().addToBase(spot, team, position);
                currentTarget.GetComponent<Target>().zeroRotation();
                carriedTargets.Remove(currentTarget);

            }
            collision.gameObject.GetComponent<HomeBase>().numInBase = capturedTargets.Count;
        }
    }

    //check collisions for walls and add a small negative reward
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().inBase != team && collision.gameObject.GetComponent<Target>().carried == 0 && !frozen)
        {
            SetReward(0.5f);
            //carrying++;
            collision.gameObject.GetComponent<Target>().setMass(0);
            collision.gameObject.GetComponent<Target>().Carry(team);
            collision.gameObject.GetComponent<Target>().zeroRotation();

            
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
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
    }

    // For manual check of controls 
    public override void Heuristic(float[] actionsOut)
    {
        var discreteActionsOut = actionsOut;
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }

        discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        discreteActionsOut[3] = Input.GetKey(KeyCode.A) ? 1:0;

        discreteActionsOut[4] = Input.GetKey(KeyCode.S) ? 1:0;
     }
}