using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BarrelAction : MonoBehaviour {

    // Use this for initialization
    bool barrelsDropped;
	void Start () {
        Interactable interactable = GetComponentInChildren<Interactable>();
        interactable.setAction(dropBarrels);
        barrelsDropped = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void dropBarrels() {
        if (!barrelsDropped) {
            barrelsDropped = true;
            PolygonCollider2D pcd = GetComponent<PolygonCollider2D>();
            Vector2[] pcdPoints = pcd.points;
            pcdPoints[1] = new Vector2(.7604f, .0958f);
            pcdPoints[2] = new Vector2(.3384f, -.2725f);
            pcd.points = pcdPoints;
            Bounds pcdBounds = pcd.bounds;
            AstarPath.active.UpdateGraphs(pcdBounds);
        }
    }
}
