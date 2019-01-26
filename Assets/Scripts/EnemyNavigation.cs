using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNavigation : MonoBehaviour {

    public Vector2[] navPoints;
    int currentPointIndex = 0;
    bool patrolling;
    Vector2 destination;
    bool stopped;
    public float speed;
    public float turnspeed;
    Quaternion targetRotation;

	void Start () {
        patrolling = true;
        stopped = false;
        destination = navPoints[0];
        speed = 1f;
        turnspeed = 5f;
        targetRotation = transform.rotation;
	}
	
	void Update () {
        if (patrolling & Vector2.Distance(destination, transform.position) < 0.01) {
            goToNextPoint();
        } else if (!stopped) {
            moveToPoint();
        }
        if (targetRotation != transform.rotation) {
            stepRotation();
        }
	}

    public void inspectLocation(Vector2 location) {
        destination = location;
        patrolling = false;
    }

    public void faceDetectedObject(GameObject detectedObj) {
        Vector2 detectedObjPos = new Vector2(detectedObj.transform.position.x, detectedObj.transform.position.y);
        facePoint(detectedObjPos);
    }

    public void stopMoving() {
        destination = new Vector2(transform.position.x, transform.position.y);
        patrolling = false;
        stopped = true;
    }

    void goToNextPoint () {
        currentPointIndex++;
        if (currentPointIndex == navPoints.Length) {
            currentPointIndex = 0;
        }
        destination = navPoints[currentPointIndex];
    }

    void moveToPoint() {
        Vector2 positionIn2D = new Vector2(transform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(positionIn2D, destination, speed * Time.deltaTime);
        facePoint(destination);
    }

    void facePoint(Vector2 point) {
        Vector2 positionIn2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = point - positionIn2D;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void stepRotation() {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnspeed);
    }


}
