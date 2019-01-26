using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightRotator : MonoBehaviour {
    public void setRotation(int deg) {
        //Vector2 positionIn2D = new Vector2(transform.position.x, transform.position.y);
        //Vector2 dir = point - positionIn2D;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.AngleAxis(deg, Vector3.forward);
    }
}
