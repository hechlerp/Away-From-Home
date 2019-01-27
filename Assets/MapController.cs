using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        x = transform.GetChild((0)).gameObject;
	}
    GameObject x;
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp((KeyCode.M))){
            if (x.activeInHierarchy) { x.SetActive((false)); }

        }
        if (Input.GetKeyDown((KeyCode.M)))
        {
            if (!x.activeInHierarchy)
            {
                x.SetActive((true));
            }
        }
	}
}
