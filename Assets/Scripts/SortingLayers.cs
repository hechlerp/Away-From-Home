using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingLayers : MonoBehaviour {
    public bool useParent2DCollider;
    void LateUpdate() {
        float positionToMeasure;
        if (useParent2DCollider) {
            positionToMeasure = transform.parent.GetComponent<Collider2D>().bounds.center.y;
        } else {
            positionToMeasure = transform.position.y;
        }
        GetComponent<SpriteRenderer>().sortingOrder = -(int)(positionToMeasure * 100);
    }
}
