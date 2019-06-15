using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSortingLayer : MonoBehaviour {
    void LateUpdate() {
        int sortingLayerMidpoint = -(int)(GetComponent<Collider2D>().bounds.center.y * 100);
        transform.parent.GetComponentInChildren<SpriteRenderer>().sortingOrder = sortingLayerMidpoint;
    }
}
