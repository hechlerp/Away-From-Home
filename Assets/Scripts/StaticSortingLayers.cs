using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticSortingLayers : MonoBehaviour {
    public int sortOrderOverride = 1000000000;
	void Start () {
        // check to see whether a sort order was passed in (the base number is unreachable).
        int sortOrder = sortOrderOverride == 1000000000 ? -(int)(transform.position.y * 100) : sortOrderOverride;
        GetComponent<SpriteRenderer>().sortingOrder = sortOrder;
    }
}
