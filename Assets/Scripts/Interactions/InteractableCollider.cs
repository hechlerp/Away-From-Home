using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCollider : MonoBehaviour
{
    public string colliderName;

    private void OnTriggerEnter2D(Collider2D collision) {
        transform.parent.GetComponent<Interactable>().handleCollision(collision, colliderName);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        transform.parent.GetComponent<Interactable>().handleCollisionExit(collision, colliderName);
    }
}
