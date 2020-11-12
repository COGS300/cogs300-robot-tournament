using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeBase : MonoBehaviour
{
    // Start is called before the first frame update
    public int team;
    public int numInBase;
    public GameObject player;

    public List<Vector3> positionsInBase = new List<Vector3>();
    public List<bool> openSpots = new List<bool>();

    void Start()
    {
        //team = player.GetComponent<MyAgent>().team;

        for (int i = 0; i < 9; i++){
            openSpots.Add(true);
        }

        float x = transform.localPosition.x;
        float z = transform.localPosition.z;
        float y = 0.5f;
        
        positionsInBase.Add(new Vector3(x + 3,y,z + 3));
        positionsInBase.Add(new Vector3(x + 3,y,z));
        
        positionsInBase.Add(new Vector3(x,y,z + 3));
        
        positionsInBase.Add(new Vector3(x + 3,y,z - 3));

        positionsInBase.Add(new Vector3(x,y,z));

        positionsInBase.Add(new Vector3(x - 3,y,z + 3));
        
        positionsInBase.Add(new Vector3(x,y,z - 3));
        
        positionsInBase.Add(new Vector3(x - 3,y,z));
        positionsInBase.Add(new Vector3(x - 3,y,z - 3));

        if(team == 1){
            positionsInBase.Reverse();
        }

        Material mat;
        if (team == 1) {
            mat = (Material) Resources.Load<Material>(WorldConstants.agent1ID + "/HomeBaseMat"); 
        }
        else {
            mat = (Material) Resources.Load<Material>(WorldConstants.agent2ID + "/HomeBaseMat"); 
        }
        gameObject.GetComponentInChildren<Renderer>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset(){
        numInBase = 0;
        for (int i = 0; i < 9; i++){
            openSpots[i] = true;
        }
    }

    public int addToFirstSpotInBase(){
        for (int i = 0; i < 9; i++){
                if(openSpots[i]){
                    openSpots[i] = false;
                    return i;
                }
            }
        return -1;
    }

    public Vector3 getPosition(int i){
        return positionsInBase[i];
    }

    void OnTriggerEnter(Collider collision){
        if(collision.gameObject.CompareTag("Target")){ 
            
        }
    }
    void OnTriggerExit(Collider collision){
         if(collision.gameObject.CompareTag("Target")){ 
            player.GetComponent<MyAgent>().capturedTargets.Remove(collision.gameObject);
            openSpots[collision.gameObject.GetComponent<Target>().spotInBase] = true;
            numInBase--;
             
         }
         if (numInBase < 0){
             numInBase = 0;
         }
         
    }
}