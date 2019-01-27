using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyNavigation : MonoBehaviour {
    Seeker seeker;
    public int currentWaypoint;
    float nextWaypointDistance;
    public bool hasPathed;
    public Path path;
    public bool traveling;
    public float speed;

    public Vector2[] navPoints;
    int currentPointIndex = 0;
    int searchPointIndex = 0;
    string alertState;
    Vector2 destination;
    bool stopped;
    Quaternion targetRotation;
    Vector2[] searchPoints;
    public float searchTime;
    bool searchedAreaComplete;
    bool lookingAround;
    int directionsChecked;
    Animator anim;

    void Start() {
        alertState = "patrol";
        stopped = false;
        destination = navPoints[0];
        currentWaypoint = 0;
        nextWaypointDistance = 0;
        hasPathed = false;
        seeker = GetComponent<Seeker>();
        traveling = true;
        GetComponent<AIPath>().maxSpeed = speed;
        searchedAreaComplete = false;
        lookingAround = false;
        directionsChecked = 0;
        anim = GetComponent<Animator>();
    }

    void Update() {
        if (stopped) {
            return;
        }
        if (traveling) {
            if (!hasPathed) {
                seeker.StartPath(transform.position, new Vector3(destination.x, destination.y, 0), OnPathComplete);
                hasPathed = true;
            }
            if (getCurrentPos() == destination || Vector2.Distance(getCurrentPos(), destination) < 0.1f) {
                traveling = false;
            }

        } else if (!stopped) {
            switch (alertState) {
                case "patrol":
                    goToNextPoint();
                    lookingAround = false;
                    searchedAreaComplete = false;
                    break;

                case "investigate":
                    if (searchPoints == null) {
                        targetRotation = transform.rotation;
                        createSearchPoints();
                    }
                    break;

                case "search":
                    if (!searchedAreaComplete & !lookingAround) {
                        Debug.Log("look around");
                        lookAround();
                    } else if (!lookingAround) {
                        goToNextSearchPoint();
                        searchedAreaComplete = false;
                    }
                    break;
            }
        }
        if (path == null) {
            // We have no path to follow yet, so don't do anything
            return;
        } else if (currentWaypoint > path.vectorPath.Count) {
            return;
        } else if (currentWaypoint == path.vectorPath.Count) {
            currentWaypoint++;
            return;
        }
        // Direction to the next waypoint
        if (path != null) {
            Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;

            dir *= speed;
            if (!lookingAround) {
                int lightDirection = getDirection(dir);
                LineOfSightRotator loSR = GetComponentInChildren<LineOfSightRotator>();
                if (loSR != null) {
                    loSR.setRotation(lightDirection);
                }
            }
            // The commented line is equivalent to the one below, but the one that is used
            // is slightly faster since it does not have to calculate a square root
            //if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
            if ((transform.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance) {
                currentWaypoint++;
                return;
            }

        }
    }

    Vector2 getCurrentPos() {
        Vector3 pos = transform.position;
        return new Vector2(pos.x, pos.y);
    }

    public void OnPathComplete(Path p) {
        path = p;
        currentWaypoint = 0;
    }

    public void inspectLocation(Vector2 location) {
        destination = location;
        Debug.Log(destination);
        alertState = "investigate";
        traveling = true;
        hasPathed = false;
    }

    public int getDirection(Vector3 dir) {
        float dirAngle = Mathf.Atan2(dir.x, dir.y);
        float degDirAngle = Mathf.Rad2Deg * dirAngle;
        if (degDirAngle < 45 & degDirAngle >= -45) {
            // left
            anim.SetBool("WalkFront", true);
            anim.SetBool("WalkRight", false);
            anim.SetBool("WalkBack", false);
            anim.SetBool("WalkLeft", false);
            return 180;
        } else if (degDirAngle >= 45 & degDirAngle < 135) {
            // front

            anim.SetBool("WalkLeft", true);
            anim.SetBool("WalkFront", false);
            anim.SetBool("WalkBack", false);
            anim.SetBool("WalkRight", false);

            return 90;
        } else if (degDirAngle < -45 & degDirAngle >= -135) {
            //right

            anim.SetBool("WalkBack", false);
            anim.SetBool("WalkLeft", false);
            anim.SetBool("WalkFront", false);
            anim.SetBool("WalkRight", true);

            return 270;
        } else {
            // back
            anim.SetBool("WalkRight", false);
            anim.SetBool("WalkLeft", false);
            anim.SetBool("WalkFront", false);
            anim.SetBool("WalkBack", true);

            return 0;
        }
    }

    //public void faceDetectedObject(GameObject detectedObj) {
    //    Vector2 detectedObjPos = new Vector2(detectedObj.transform.position.x, detectedObj.transform.position.y);
    //    facePoint(detectedObjPos);
    //}

    public void stopMoving() {
        destination = new Vector2(transform.position.x, transform.position.y);
        alertState = "detecting";
        stopped = true;
        traveling = false;
    }

    void goToNextPoint() {
        currentPointIndex++;
        if (currentPointIndex == navPoints.Length) {
            currentPointIndex = 0;
        }
        destination = navPoints[currentPointIndex];
        traveling = true;
        hasPathed = false;
    }

    void createSearchPoints() {
        searchPoints = new Vector2[5];
        int[] directions = new int[5] { 0, 72, 144, 216, 288 };
        float maxDist = 3;
        Vector2 position = getCurrentPos();
        int layerMask = LayerMask.GetMask("Obstacle");
        for (int i = 0; i < directions.Length; i++) {
            int direction = directions[i];
            float radDir = direction * Mathf.Deg2Rad;
            Vector2 vectorDir = new Vector2(Mathf.Cos(radDir), Mathf.Sin(radDir));
            RaycastHit2D hit = Physics2D.Raycast(position, vectorDir, maxDist, layerMask);
            if (hit.collider != null) {
                Vector3 extents = transform.GetComponent<SpriteRenderer>().sprite.bounds.extents;
                //searchPoints[i] = hit.point;
                searchPoints[i] = new Vector2(transform.position.x, transform.position.y);
            } else {
                searchPoints[i] = position - (vectorDir * maxDist);
            }
        }
        foreach(Vector2 point in searchPoints) {
            Debug.Log(point);
        }
        alertState = "search";
        StartCoroutine(returnToPatrolAfterTime(searchTime));
    }

    void OnTriggerEnter(Collider other) {
        if (alertState == "search") {
            goToNextSearchPoint();
        }
    }

    void goToNextSearchPoint() {
        searchPointIndex++;
        if (searchPointIndex == searchPoints.Length) {
            searchPointIndex = 0;
        }
        destination = searchPoints[searchPointIndex];
        traveling = true;
        hasPathed = false;
    }

    public void returnToPatrol() {
        alertState = "patrol";
        searchPoints = null;
        traveling = false;
    }



    IEnumerator returnToPatrolAfterTime(float time) {
        yield return new WaitForSeconds(time);
        returnToPatrol();
    }

    void lookAround() {
        LineOfSightRotator losR = GetComponentInChildren<LineOfSightRotator>();
        lookingAround = true;
        directionsChecked++;
        if (directionsChecked > 4) {
            searchedAreaComplete = true;
            lookingAround = false;
            directionsChecked = 0;
        } else {
            losR.setRotation(90 * directionsChecked);
            StartCoroutine(checkOtherDirections(.5f));
        }
        
    }

    IEnumerator checkOtherDirections(float time) {
        yield return new WaitForSeconds(time);
        lookAround();
    }

    //void moveToPoint() {
    //    Vector2 positionIn2D = new Vector2(transform.position.x, transform.position.y);
    //    transform.position = Vector2.MoveTowards(positionIn2D, destination, speed * Time.deltaTime);
    //    facePoint(destination);
    //}









}
