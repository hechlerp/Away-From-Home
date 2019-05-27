using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public AudioSource AS;

    public static PlayerController instance;
	public GameObject Winning;
    Animator anim;
    Rigidbody2D rb;
    bool lockedMovement;

    void setAnimation(string animationName) {
        string[] names = new string[8];
        names[0] = "WalkDown";
        names[1] = "WalkDownRight";
        names[2] = "WalkRight";
        names[3] = "WalkUpRight";
        names[4] = "WalkUp";
        names[5] = "WalkUpLeft";
        names[6] = "WalkLeft";
        names[7] = "WalkDownLeft";
        foreach (string animation in names) {
            if (animation == animationName) {
                anim.SetBool(animation, true);
            } else {
                anim.SetBool(animation, false);
            }
            
        }
    }

    void OnTriggerEnter2D(Collider2D col){
		if (col.name == "EndPoint") {

            GameManager.instance.Win();
//			Winning.SetActive (true);
			Debug.Log ("Win");
        }else if (col.name == "CatRange"){
            col.transform.parent.GetComponent<CatController>().Alarm();

        } else if (col.name == "Line of Sight") {
            //	Debug.Log ("Parent Triggered");

        } else if (col.name == "Parent") {//it's a trigger of parent trigger area
        }
        if (col.tag == "Wood")
        {
            if (onGrass) return;
            //playGrass();
            onGrass = true;
        }
        else if (col.tag == "Path")
        {
            if (!onGrass) return;
            //playWood();
            onGrass = false;
        }
        else {  }

	}
    public bool onGrass = true;
	// Use this for initialization
	void Start () {
        AS = GetComponent<AudioSource>();
        instance = this;
        WoodSound = Resources.Load("Wood", typeof(AudioClip)) as AudioClip;
        GrassSound = Resources.Load("Grass", typeof(AudioClip)) as AudioClip;
        // The 0th child is the sprite object
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        lockedMovement = false;
    }
    AudioClip WoodSound;
    AudioClip GrassSound;
    void playWood(){
        //trigger enter
        AS.clip = WoodSound;
        AS.volume = 1f;
        AS.Play();

    }
    void playGrass(){ AS.clip = GrassSound;AS.Play();
        AS.volume = 0.35f;}
    void stop(){
     
        AS.Stop();

    }

    public bool isActive = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isActive) { return; }

        MoveControlByRigidBody();
    }

    public void lockPlayerMovement () {
        lockedMovement = true;
    }

    public void unlockPlayerMovement () {
        lockedMovement = false;
    }

    public float m_speed = 5f;
	//Translate移动控制函数
	void MoveControlByRigidBody()
	{

        float TranslateAmount = m_speed;

        if (Input.GetKey(KeyCode.W)|| Input.GetKey((KeyCode.S))){
            if (Input.GetKey((KeyCode.A))|| Input.GetKey((KeyCode.D))){
                TranslateAmount /= 1.41f;

            }
        }
        if (!lockedMovement) {

            string animationName = "";
            Vector3 nextPoint;
            float xMod = 0;
            float yMod = 0;
		    if (Input.GetKey(KeyCode.W)|Input.GetKey(KeyCode.UpArrow)) //前
		    {
                nextPoint = Vector3.up * TranslateAmount;
                yMod += nextPoint.y;
                animationName = "WalkUp";
		    }
		    if (Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow)) //后
		    {
                nextPoint = Vector3.up * -TranslateAmount;
                yMod += nextPoint.y;
                animationName = "WalkDown";
		    }
		    if (Input.GetKey(KeyCode.A) | Input.GetKey(KeyCode.LeftArrow)) //左
		    {
                nextPoint = Vector3.right * -TranslateAmount;
                xMod += nextPoint.x;
                animationName = "WalkLeft";
                if (yMod > 0) {
                    animationName = "WalkUpLeft";
                } else if (yMod < 0) {
                    animationName = "WalkDownLeft";
                }
		    }
            if (Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow)) //右
            {
                nextPoint = Vector3.right * TranslateAmount;
                xMod += nextPoint.x;
                animationName = "WalkRight";
                if (yMod > 0) {
                    animationName = "WalkUpRight";
                } else if (yMod < 0) {
                    animationName = "WalkDownRight";
                }
            }
            // Use the modifiers to generate a velocity for the RigidBody.
            rb.velocity = new Vector2(xMod, yMod) * 50f * Time.deltaTime;
            setAnimation(animationName);
        }
    }
}
