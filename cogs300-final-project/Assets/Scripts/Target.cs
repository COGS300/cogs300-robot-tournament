using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int carried;
    public int inBase;

    Vector3 base1co = new Vector3 (25f, 0f, 25f);
    Vector3 base2co = new Vector3 (-25f, 0f, -25f);
    Vector3 startingCo;

    Vector3 positionInBase = Vector3.zero;

    public int spotInBase;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        setMass(0);
        startingCo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(transform.localPosition.y < 0.5f){
            transform.localPosition = new Vector3(transform.localPosition.x, 0.5f, transform.localPosition.z);
        }
        
        if (carried != 0 || inBase != 0){
            rb.useGravity = false;
        }
        else {
            rb.useGravity = true;
        }

        if(carried == 0 && inBase != 0){
            this.transform.localPosition = positionInBase;
        }
        // if(inBase == 1 && carried == 0){
        //     this.transform.localPosition = new Vector3 (base1co.x, (spotInBase * 1.1f) - 0.5f, base1co.z);
        // }
        // else if (inBase == 2 && carried == 0){
        //     this.transform.localPosition = new Vector3 (base2co.x, (spotInBase * 1.1f) - 0.5f, base2co.z);
        // }

    }

    public void setMass(float mass){
        rb.mass = mass;
    }

    public void zeroRotation(){
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void explode(){
        setMass(1);
        rb.velocity = new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f));
        rb.angularVelocity = Vector3.zero;
    }
    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Player")){
            zeroRotation();
            setMass(0);
        }
    }

     void OnTriggerEnter(Collider collision){

     }
     void OnTriggerExit(Collider collision){
         if(collision.gameObject.CompareTag("HomeBase")){ 
             inBase = 0;
         }
         
     }

    public void addToBase(int spot, int playerTeam, Vector3 position){
        inBase = playerTeam;
        carried = 0;
        positionInBase = position;
        spotInBase = spot;
        //spotInBase = amountInBase;
        // if(playerTeam == 1){
        //     this.transform.localPosition = new Vector3 (base1co.x, (spotInBase * 1.1f) - 0.5f, base1co.z);
        // }
        // else{
        //     this.transform.localPosition = new Vector3 (base2co.x, (spotInBase * 1.1f) - 0.5f, base2co.z);
        // }

     }

     public void addToSpotInbase(int spot, Vector3 position){
         positionInBase = position;
         spotInBase = spot;
         Debug.Log(positionInBase);
         Debug.Log(spotInBase);
     }

     public void resetGame(){
        transform.localPosition = startingCo;
        zeroRotation();
        carried = 0;
        inBase = 0;
        spotInBase = 0;

     }

     public void Carry(int team){
         carried = team;
     }

     public void adjustSpotInBase(){
        //  spotInBase--;
        //  if(inBase == 1){
        //     this.transform.localPosition = new Vector3 (base1co.x, (spotInBase * 1.1f) - 0.5f, base1co.z);
        // }
        // else{
        //     this.transform.localPosition = new Vector3 (base1co.z, (spotInBase * 1.1f) - 0.5f, base1co.z);
        // }
     }
}