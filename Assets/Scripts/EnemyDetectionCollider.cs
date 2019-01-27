using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionCollider : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D objectCollider) {
        if (objectCollider.gameObject.tag == "Player") {
            GetComponentInParent<EnemyDetection>().detectObject(objectCollider.gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D objectCollider) {
        if (objectCollider.gameObject.tag == "Player") {
            GetComponentInParent<EnemyDetection>().stopCheckingDetection();
        }
    }
}
