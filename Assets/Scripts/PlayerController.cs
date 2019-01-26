using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	public GameObject Winning;
	void OnTriggerEnter2D(Collider2D col){
		if (col.name == "EndPoint") {
			Winning.SetActive (true);
			Debug.Log ("Win");
		} else if (col.name == "ParentRange") {
			Debug.Log ("Parent Triggered");

		} else if (col.name == "Parent") {//it's a trigger of parent trigger area
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		MoveControlByTranslate ();
	}
	public float m_speed = 5f;
	//Translate移动控制函数
	void MoveControlByTranslate()
	{
		if (Input.GetKey(KeyCode.W)|Input.GetKey(KeyCode.UpArrow)) //前
		{
			this.transform.Translate(Vector3.up*m_speed*Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow)) //后
		{
			this.transform.Translate(Vector3.up *- m_speed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.A) | Input.GetKey(KeyCode.LeftArrow)) //左
		{
			this.transform.Translate(Vector3.right *-m_speed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow)) //右
		{
			this.transform.Translate(Vector3.right * m_speed * Time.deltaTime);
		}
	}
}
