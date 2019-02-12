using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightRotator : MonoBehaviour {
    bool rotating;
    Quaternion targetRotation;
    float smooth;

    void Start() {
        smooth = 100f;  
    }

    void Update() {
        if (rotating == true) {
            //targetRotation *= Quaternion.AngleAxis(60, Vector3.forward);
            transform.Rotate(new Vector3(0, 0, smooth * Time.deltaTime));
        }

    }

    public void setRotation(int deg) {
        //Vector2 positionIn2D = new Vector2(transform.position.x, transform.position.y);
        //Vector2 dir = point - positionIn2D;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.AngleAxis(deg, Vector3.forward);
        if (transform.rotation.eulerAngles.z == 270) {
            transform.GetChild(0).transform.localPosition = new Vector3(0.3f, 2.1f, 0f);
        } else {
            transform.GetChild(0).transform.localPosition = new Vector3(0f, 2.1f, 0f);
        }
    }

    public void startRotatation() {
        rotating = true;
    }
}
