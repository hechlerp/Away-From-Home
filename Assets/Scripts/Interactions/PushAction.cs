using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PushAction : MonoBehaviour {

    // Use this for initialization
    bool isPushing;
    public int weight;
    int baseSpeed;
    public GameObject player;
    bool moving;
    public float pushMag;
    Vector2 destination;
    public Vector3 tooltipOffset;
    Interactable interactable;

    void Start() {
        interactable = GetComponentInChildren<Interactable>();
        interactable.setAction(pushObject);
        interactable.setMiddlePosition(transform.position + tooltipOffset);
        isPushing = false;
        moving = false;
        baseSpeed = 20;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp("f") & isPushing) {
            isPushing = false;
        } else if (isPushing & !moving) {
            attemptPush();
        } else if (moving) {
            moveTowardDestination();
        }
    }

    void pushObject() {
        isPushing = true;
    }
    // Get the direction that the player is facing.
    Vector2 determineDirection(Vector2 pos) {

        Vector3 playerPos = player.transform.position;
        Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.y);
        Vector2 direction = (pos - playerPos2D).normalized;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            if (direction.x > 0) {
                return Vector2.right;
            } else {
                return Vector2.left;
            }
        } else {
            if (direction.y > 0) {
                return Vector2.up;
            } else {
                return Vector2.down;
            }
        }
    }

    void attemptPush() {
        Vector3 pos = transform.position;
        Vector2 pos2D = new Vector2(pos.x, pos.y);
        Vector2 direction = determineDirection(pos2D);
        destination = pos2D + direction * pushMag;
        bool isClear = false;
        int layerMask = LayerMask.GetMask("Obstacle");
        RaycastHit2D[] results = Physics2D.LinecastAll(transform.position, destination, layerMask);
        if (results.Length < 2) {
            isClear = true;
        }
        // Push the block if it can be pushed, you're facing the right way, and trying to interact.
        if (isClear & direction == Vector2.right & (Input.GetKey("d") | Input.GetKey("right"))) {
            startMoving();
        } else if (isClear & direction == Vector2.down & (Input.GetKey("s") | Input.GetKey("down"))) {
            startMoving();
        } else if (isClear & direction == Vector2.left & (Input.GetKey("a") | Input.GetKey("left"))) {
            startMoving();
        } else if (isClear & direction == Vector2.up & (Input.GetKey("w") | Input.GetKey("up"))) {
            startMoving();
        }
    }

    void startMoving() {
        moving = true;
        interactable.blockPrompting();
    }

    void moveTowardDestination() {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        // if you're close enough, stop moving.
        if (Vector2.Distance(pos2D, destination) < 0.05) {
            moving = false;
            Interactable interactableComponent = GetComponentInChildren<Interactable>();
            interactableComponent.unblockPrompting();
            interactableComponent.setMiddlePosition(transform.position + tooltipOffset);
        // otherwise, lerp toward the destination.
        } else {
            transform.position = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, 0), Time.deltaTime * baseSpeed / weight);
            // As the block moves, update the blocked area.
            blockArea();
        }
    }

    void blockArea() {
        Bounds bounds = GetComponent<Collider2D>().bounds;
        AstarPath.active.UpdateGraphs(bounds);
    }
}
