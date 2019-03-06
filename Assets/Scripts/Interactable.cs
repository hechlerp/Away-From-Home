﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    //Collider2D[] cds;
    bool isPlayerInRange;
    System.Action action = null;
    public GameObject tooltip;
    public Vector3 middlePosition;
    bool readyToExecute;

    bool promptingBlocked;

    // The name of the parent game object, used for the interactionQueue's OrderedDictionary.
    string nameToStore;

    void Start () {
        //cds = GetComponents<Collider2D>();
        isPlayerInRange = false;
        readyToExecute = false;
        promptingBlocked = false;
        nameToStore = transform.parent.gameObject.name;

    }

    void Update () {
        if (readyToExecute) {
            if (Input.GetKeyDown("f")) {
                // I think trycatch can be used here, will refactor later if I get the chance.
                if (action != null) {
                    action();
                }
            }
        }
	}

    public void setAction (System.Action passedAction) {
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
    }

    public void unblockPrompting() {
        promptingBlocked = false;
    }

    public void setMiddlePosition(Vector3 position) {
        middlePosition = position;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player" & !promptingBlocked) {
            if (!isPlayerInRange) {
                isPlayerInRange = true;
                tooltip.GetComponent<InteractionQueue>().addToQueue(nameToStore, gameObject);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            isPlayerInRange = false;
            tooltip.GetComponent<InteractionQueue>().removeFromQueue(nameToStore, gameObject);
        }
    }
}
