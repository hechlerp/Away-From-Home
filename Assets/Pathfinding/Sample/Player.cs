using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour {
	private CharacterController c;
	public float speed = 10f;

	void Start () {
		c = gameObject.GetComponent<CharacterController>();
		transform.rotation = new Quaternion(0,0,0, 0);
	}

	void Update () {
		if (Input.GetKey(KeyCode.W)) c.SimpleMove(Vector3.forward*speed);
		if (Input.GetKey(KeyCode.S)) c.SimpleMove(-Vector3.forward*speed);
		if (Input.GetKey(KeyCode.D)) c.SimpleMove(Vector3.right*speed);
		if (Input.GetKey(KeyCode.A)) c.SimpleMove(-Vector3.right*speed);
	}
}
