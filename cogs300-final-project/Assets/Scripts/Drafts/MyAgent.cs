using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MyAgent : CogsAgent
{
    // ------------------MONOBEHAVIOR FUNCTIONS-------------------
    
    // Initialize values
    protected override void Start()
    {
        base.Start();
        AssignBasicRewards();
    }

    // For actual actions in the environment (e.g. movement, shoot laser)
    // that is done continuously
    protected override void FixedUpdate() {
        base.FixedUpdate();
        
        LaserControl();
        // Movement based on DirToGo and RotateDir
        if(!IsFrozen()){
            if (!IsLaserOn()){
                rBody.AddForce(dirToGo * GetMoveSpeed(), ForceMode.VelocityChange);
            }
            transform.Rotate(rotateDir, Time.deltaTime * GetTurnSpeed());
        }
    }


    
    // --------------------AGENT FUNCTIONS-------------------------

    // Get relevant information from the environment to effectively learn behavior
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent velocity in x and z axis 
        var localVelocity = transform.InverseTransformDirection(rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);

        // Time remaning
        sensor.AddObservation(timer.GetComponent<Timer>().GetTimeRemaning());  

        // Agent's current rotation
        var localRotation = transform.rotation;
        sensor.AddObservation(transform.rotation.y);

        // Agent and home base's position
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(baseLocation.localPosition);

        // for each target in the environment, add: its position, whether it is being carried,
        // and whether it is in a base
        foreach (GameObject target in targets){
            sensor.AddObservation(target.transform.localPosition);
            sensor.AddObservation(target.GetComponent<Target>().GetCarried());
            sensor.AddObservation(target.GetComponent<Target>().GetInBase());
        }
        
        // Whether the agent is frozen
        sensor.AddObservation(IsFrozen());
    }

    // What to do when an action is received (i.e. when the Brain gives the agent information about possible actions)
    public override void OnActionReceived(float[] act)
    {
        AddReward(-0.0005f);
        int forwardAxis = (int)act[0]; //NN output 0

        // Call movePlayer helper to handle the various cases based on brain output
        movePlayer(forwardAxis, (int)act[1], (int)act[2], (int)act[3], (int)act[4]);

    }

    // For manual check of controls 
    public override void Heuristic(float[] actionsOut)
    {
        // Overrides brain output with value based on keyboard input
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



    // ----------------------ONTRIGGER AND ONCOLLISION FUNCTIONS------------------------
    // Called when object collides with or trigger (similar to collide but without physics) other objects

    protected override void OnTriggerEnter(Collider collision)
    {
        base.OnTriggerEnter(collision);

        // At home base
        if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == GetTeam())
        {
            AddReward(GetCarrying() * 0.1f); 
        }
    }

    protected override void OnCollisionEnter(Collision collision) 
    {
        base.OnCollisionEnter(collision);

        // target is not in my base and is not being carried and I am not frozen
        if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != GetTeam() && collision.gameObject.GetComponent<Target>().GetCarried() == 0 && !IsFrozen())
        {
            SetReward(0.5f);
        }

        // if hit wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
    }



    //  --------------------------HELPERS---------------------------- 
    
    // Assign reward values to basic actions
    private void AssignBasicRewards() {
        rewardDict = new Dictionary<string, float>();

        rewardDict.Add("frozen", -0.1f);
        rewardDict.Add("shooting-laser", 0f);
        rewardDict.Add("hit-enemy", 0.5f);
        rewardDict.Add("dropped-one-target", 0f);
        rewardDict.Add("dropped-targets", 0f);
    }
    
    // Adjust values used for agent actions based on brain output
    private void movePlayer(int forwardAxis, int rotateAxis, int shootAxis, int goToTargetAxis, int goToBaseAxis)
    {
        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;
        SetLaser(false);

        switch (forwardAxis)
        {
            case 0: break; //do nothing
            case 1: dirToGo = transform.forward; break;
            case 2: dirToGo = -transform.forward; break;
        }
        switch (rotateAxis)
        {
            case 0: break; //do nothing
            case 1: rotateDir = -transform.up; break;
            case 2: rotateDir = transform.up; break;
        }

        switch (shootAxis)
        {
            case 0: break; //do nothing 
            case 1: SetLaser(true); break;
        }
        
         switch (goToTargetAxis)
        {
            case 0: break; //do nothing 
            case 1: goToNearestTarget(); break;
        }

         switch (goToBaseAxis)
        {
            case 0: break; //do nothing 
            case 1: goToBase(); break;
        }
    }

    // Go to home base
    private void goToBase(){
        turnAndGo(getYAngle(myBase));
    }

    // Go to the nearest target
    private void goToNearestTarget(){
        GameObject target = getNearestTarget();
        if (target != null){
            float rotation = getYAngle(target);
            turnAndGo(rotation);
        }        
    }

    // Rotate and go in specified direction
    private void turnAndGo(float rotation){

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

    // ???
    private float getYAngle(GameObject target) {
        
       Vector3 targetDir = target.transform.position - transform.position;
       Vector3 forward = transform.forward;

      float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
      return angle; 
        
    }
}
