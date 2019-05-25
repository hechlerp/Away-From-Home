using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    //Collider2D[] cds;
    bool isPlayerInRange;
    System.Action<Dictionary<string, object>> action = null;
    public GameObject tooltip;
    public Vector3 middlePosition;
    bool readyToExecute;
    bool shouldActivateTooltip;
    bool promptingBlocked;

    // The name of the parent game object, used for the interactionQueue's OrderedDictionary.
    string nameToStore;
    Dictionary<string, bool> occupiedColliders;

    void Start () {
        //cds = GetComponents<Collider2D>();
        isPlayerInRange = false;
        readyToExecute = false;
        promptingBlocked = false;
        nameToStore = transform.parent.gameObject.name;
        shouldActivateTooltip = true;
        occupiedColliders = new Dictionary<string, bool>();
        foreach (Transform child in transform) {
            if (child.name == "InteractableCollider") {
                occupiedColliders.Add(child.GetComponent<InteractableCollider>().colliderName, false);
            }
        }
    }

    void Update () {
        if (readyToExecute) {
            if (Input.GetKeyDown("f")) {
                // I think trycatch can be used here, will refactor later if I get the chance.
                if (action != null) {
                    action(new Dictionary<string, object>() { { "colliders", occupiedColliders } });
                }
            }
        }
	}

    public void setAction (System.Action<Dictionary<string, object>> passedAction) {
        action = passedAction;
    }

    public void prepareForExecution() {
        readyToExecute = true;
        tooltip.transform.position = middlePosition;
        tooltip.SetActive(true);
    }

    public void blockExecution() {
        readyToExecute = false;
        tooltip.SetActive(false);
    }

    public void blockPrompting() {
        promptingBlocked = true;
        tooltip.SetActive(false);
    }

    public void unblockPrompting() {
        promptingBlocked = false;
        shouldActivateTooltip = true;
    }

    public void setMiddlePosition(Vector3 position) {
        middlePosition = position;
    }

    public void handleCollision(Collider2D collision, string colliderName) {
        if (collision.gameObject.tag == "Player" &!promptingBlocked) {
            occupiedColliders[colliderName] = true;
            if (!isPlayerInRange) {
                isPlayerInRange = true;
                tooltip.GetComponent<InteractionQueue>().addToQueue(nameToStore, gameObject);
            } else if (shouldActivateTooltip) {
                shouldActivateTooltip = false;
                prepareForExecution();
            }
        }

    }

    public void handleCollisionExit(Collider2D collision, string colliderName) {
        if (collision.gameObject.tag == "Player") {
            occupiedColliders[colliderName] = false;
            isPlayerInRange = false;
            foreach (string key in occupiedColliders.Keys) {
                if (occupiedColliders[key]) {
                    isPlayerInRange = true;
                }
            }
            if (!isPlayerInRange) {
                tooltip.GetComponent<InteractionQueue>().removeFromQueue(nameToStore, gameObject);
            }
        }
        
    }
}
