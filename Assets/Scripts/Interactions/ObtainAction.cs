using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainAction : MonoBehaviour
{
    // Start is called before the first frame update
    Interactable interactable;
    public GameObject player;
    void Start()
    {
        interactable = GetComponentInChildren<Interactable>();
        interactable.setAction(gatherItem);
        interactable.setMiddlePosition(transform.position + interactable.middlePosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void gatherItem() {
        player.GetComponent<PlayerInventory>().addToInventory(name, 1);
        gameObject.SetActive(false);
    }
}
