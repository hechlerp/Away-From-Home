using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class InteractionQueue : MonoBehaviour {
    Dictionary<string, GameObject> interactionDictionary;
    GameObject currentObject;

    // Initialize the dictionary, but disable the tooltip until it's needed
    void Awake () {
        interactionDictionary = new Dictionary<string, GameObject>();
        gameObject.SetActive(false);
	}
	
    public void addToQueue (string objName, GameObject interactionObj) {
        interactionDictionary.Add(objName, interactionObj);
        if (interactionDictionary.Count == 1) {
            prepareNextObject();
        }
    }

    public void removeFromQueue(string objName, GameObject interactionObj) {
        bool shouldPrepareNext = false;
        if (currentObject == interactionObj) {
            shouldPrepareNext = true;
        }
        interactionDictionary.Remove(objName);
        Interactable inter = interactionObj.GetComponent<Interactable>();
        inter.blockExecution();
        if (shouldPrepareNext) {
            prepareNextObject();
        }
    }

    void prepareNextObject () {
        if (interactionDictionary.Count > 0) {
            GameObject currentObject = findClosestObject();
            currentObject.GetComponent<Interactable>().prepareForExecution();
        }
    }



    GameObject findClosestObject () {
        float shortestDistance = 0;
        GameObject closestObject = null;
        float entryDistance;
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        foreach(KeyValuePair<string, GameObject> entry in interactionDictionary) {
            entryDistance = Vector3.Distance(entry.Value.transform.position, playerPos);
            if (shortestDistance == 0 | shortestDistance > entryDistance) {
                shortestDistance = entryDistance;
                closestObject = entry.Value;
            }
        }
        return closestObject;
    }
}
