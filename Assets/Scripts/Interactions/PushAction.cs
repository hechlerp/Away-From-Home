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
    public List<string> pushableDirs;
    Dictionary<string, bool> playerDirs;
    Interactable interactable;
    Dictionary<string, string> inputDirDict;

    void Start() {
        interactable = GetComponentInChildren<Interactable>();
        interactable.setAction(pushObject);
        interactable.setMiddlePosition(transform.position + tooltipOffset);
        playerDirs = new Dictionary<string, bool>();
        foreach (string dir in pushableDirs) {
            playerDirs.Add(dir, false);
        }
        inputDirDict = new Dictionary<string, string>();
        inputDirDict.Add("w", "up");
        inputDirDict.Add("s", "down");
        inputDirDict.Add("a", "left");
        inputDirDict.Add("d", "right");
        isPushing = false;
        moving = false;
        baseSpeed = 20;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp("f") & isPushing) {
            isPushing = false;
            List<string> playerDirKeys = new List<string>(playerDirs.Keys);
            foreach (string key in playerDirKeys) {
                playerDirs[key] = false;
            }
        } else if (isPushing & !moving) {
            attemptPush();
        } else if (moving) {
            moveTowardDestination();
        }
    }

    void pushObject(Dictionary<string, object> args) {
        Dictionary<string, bool> colliders = (Dictionary<string, bool>)args["colliders"];
        List<string> playerDirKeys = new List<string>(playerDirs.Keys);
        foreach (string key in playerDirKeys) {
            playerDirs[key] = false;
        }
        foreach (string key in colliders.Keys) {
            if (playerDirs.ContainsKey(key)) {
                playerDirs[key] = colliders[key];
            }
        }
        isPushing = true;
    }
    // Get the direction that the player is facing.
    List<float> determineDirection() {
        List<float> pushDirection = new List<float>();

        // in each case, we want to check if the player is pushing AGAINST the given side
        string inputDirection = "";
        if (Input.GetKey("w") | Input.GetKey("up")) {
            inputDirection += "up";
        }
        if (Input.GetKey("s") | Input.GetKey("down")) {
            inputDirection += "down";
        }
        if (Input.GetKey("a") | Input.GetKey("left")) {
            inputDirection += "left";
        }
        if (Input.GetKey("d") | Input.GetKey("right")) {
            inputDirection += "right";
        }
        if (pushableDirs.Contains(inputDirection)) {
            if (playerDirs.ContainsKey("right") & playerDirs["right"] & inputDirection.Contains("left")) {
                pushDirection.Add(-1);
            } else if (playerDirs.ContainsKey("left") & playerDirs["left"] & inputDirection.Contains("right")) {
                pushDirection.Add(1);
            } else {
                pushDirection.Add(0);
            }
            if (playerDirs.ContainsKey("up") & playerDirs["up"] & inputDirection.Contains("down")) {
                pushDirection.Add(-1);
            } else if (playerDirs.ContainsKey("down") & playerDirs["down"] & inputDirection.Contains("up")) {
                pushDirection.Add(1);
            } else {
                pushDirection.Add(0);
            }
        } else {
            pushDirection.Add(0);
            pushDirection.Add(0);
        }
        return pushDirection;
    }

    void attemptPush() {
        Vector3 pos = transform.position;
        Vector2 pos2D = new Vector2(pos.x, pos.y);
        List<float> direction = determineDirection();
        if (direction[0] != 0f && direction[1] != 0f) {
            direction[0] /= 1.41f;
            direction[1] /= 1.41f;
        }
        Vector2 dirVector = new Vector2(direction[0], direction[1]);
        destination = pos2D + dirVector * pushMag;
        bool isClear = false;
        int layerMask = LayerMask.GetMask("Obstacle");
        RaycastHit2D[] results = Physics2D.LinecastAll(transform.position, destination, layerMask);
        if (results.Length < 2) {
            isClear = true;
        }
        Debug.Log(dirVector);
        // Push the block if it can be pushed, you're facing the right way, and trying to interact.
        if (isClear && (dirVector.x != 0 | dirVector.y != 0)) {
            startMoving();
        }
    }

    void startMoving() {
        moving = true;
        player.GetComponent<PlayerController>().lockPlayerMovement();
        interactable.blockPrompting();
    }

    void moveTowardDestination() {
        Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
        // if you're close enough, stop moving.
        if (Vector2.Distance(pos2D, destination) < 0.05) {
            moving = false;
            player.GetComponent<PlayerController>().unlockPlayerMovement();
            Interactable interactableComponent = GetComponentInChildren<Interactable>();
            interactableComponent.unblockPrompting();
            interactableComponent.setMiddlePosition(transform.position + tooltipOffset);
        // otherwise, lerp toward the destination.
        } else {
            Vector3 lastPosition = transform.position;
            transform.position = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, 0), Time.deltaTime * baseSpeed / weight);
            Vector3 positionDiff = transform.position - lastPosition;
            player.transform.position = player.transform.position + positionDiff;
            // As the block moves, update the blocked area.
            blockArea();
        }
    }

    void blockArea() {
        Bounds bounds = GetComponent<Collider2D>().bounds;
        AstarPath.active.UpdateGraphs(bounds);
    }
}
