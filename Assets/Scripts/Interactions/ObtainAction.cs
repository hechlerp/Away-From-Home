using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainAction : MonoBehaviour
{
    // Start is called before the first frame update
    Interactable interactable;
    public GameObject player;
    public bool shouldDestroy;
    public string itemName;
    void Start()
    {
        interactable = GetComponentInChildren<Interactable>();
        interactable.setAction(gatherItem);
        interactable.setMiddlePosition(transform.position + interactable.middlePosition);
    }

    void gatherItem() {
        player.GetComponent<PlayerInventory>().addToInventory(itemName, 1);
        if (shouldDestroy) {
            // Destroy self upon being picked up.
            Destroy(gameObject);
        } else {
            gameObject.SetActive(false);
        }
    }
}
