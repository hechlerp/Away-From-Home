using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour {

    public GameObject objectToPursue;
    bool checkDetection;
	
	void Start () {
        objectToPursue = null;
        checkDetection = false;
	}
	
	void Update () {
		if (checkDetection & objectToPursue) {
            int layerMask = LayerMask.GetMask("Obstacle");
            //RaycastHit2D hit;
            if (!Physics2D.Linecast(transform.position, objectToPursue.transform.position, layerMask)) {
                GameManager.instance.Fail();
                GetComponent<EnemyNavigation>().enabled = false;
                GetComponent<Pathfinding.AIPath>().enabled = false;
            }
        }
	}

    public void pursueObject(GameObject detectedObj) {
        objectToPursue = detectedObj;
        GetComponent<EnemyNavigation>().inspectLocation(new Vector2(objectToPursue.transform.position.x, objectToPursue.transform.position.y));
    }

    public void detectObject(GameObject detectedObject) {
        objectToPursue = detectedObject;
        startCheckingDetection();

    }

    void startCheckingDetection() {
        checkDetection = true;
    }

    public void stopCheckingDetection() {
        checkDetection = false;
        objectToPursue = null;
    }
}
