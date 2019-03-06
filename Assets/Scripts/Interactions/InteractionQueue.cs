using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class InteractionQueue : MonoBehaviour {
    OrderedDictionary interactionDictionary;

    // Initialize the dictionary, but disable the tooltip until it's needed
    void Awake () {
        interactionDictionary = new OrderedDictionary();
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
        if ((GameObject)interactionDictionary[0] == interactionObj) {
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
            GameObject nextObject = (GameObject)interactionDictionary[0];
            nextObject.GetComponent<Interactable>().prepareForExecution();
        }
    }
}
