using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRotate : MonoBehaviour {
    float z_rotation = 0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        z_rotation += 2f;
        Vector3 z_position = new Vector3(0, 0, z_rotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(z_position), Time.deltaTime);
    }
}
