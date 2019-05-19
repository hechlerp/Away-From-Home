using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingLayers : MonoBehaviour {
    public bool useParent2DCollider;
    void LateUpdate() {
        if (useParent2DCollider) {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.parent.GetComponent<Collider2D>().bounds.center.y * 100);
        } else {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100);
        }
    }
}
