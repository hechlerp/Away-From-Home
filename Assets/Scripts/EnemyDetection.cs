using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour {

    public GameObject objectToPursue;
	
	void Start () {
        objectToPursue = null;
	}
	
	void Update () {
		
	}

    public void pursueObject(GameObject detectedObj) {
        objectToPursue = detectedObj;
        GetComponent<EnemyNavigation>().inspectLocation(new Vector2(objectToPursue.transform.position.x, objectToPursue.transform.position.y));
    }

    public void detectObject(GameObject detectedObject) {
        objectToPursue = detectedObject;
        EnemyNavigation en = GetComponent<EnemyNavigation>();
        en.stopMoving();

    }
}
