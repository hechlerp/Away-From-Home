using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionCollider : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D objectCollider) {
        GetComponentInParent<EnemyDetection>().detectObject(objectCollider.gameObject);
    }
}
