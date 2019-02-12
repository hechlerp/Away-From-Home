using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    //Collider2D[] cds;
    bool isPlayerInRange;
    System.Action action = null;
    public GameObject tooltip;
    public Vector3 middlePosition;

	// Use this for initialization
	void Start () {
        //cds = GetComponents<Collider2D>();
        isPlayerInRange = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (isPlayerInRange) {
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

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            if (!isPlayerInRange) {
                isPlayerInRange = true;
                tooltip.transform.position = middlePosition;
                tooltip.SetActive(true);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            isPlayerInRange = false;
            tooltip.SetActive(false);
        }
    }
}
