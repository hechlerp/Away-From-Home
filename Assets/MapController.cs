using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour {
    public List<Sprite> maps;
    public Image img;
    public List<float> bar;//2
	// Use this for initialization
	void Start () {
        x = transform.GetChild((0)).gameObject;
        ply = GameObject.Find("Player");
	}
    GameObject ply;
    GameObject x;
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp((KeyCode.M))){
            
            if (x.activeInHierarchy) { 


                x.SetActive((false)); }

        }
        if (Input.GetKeyDown((KeyCode.M)))
        {
            if (ply.transform.position.x<bar[0])
            {
                img.overrideSprite = maps[0];
            }else if (ply.transform.position.x<bar[1]){
                img.overrideSprite = maps[1];

            }else{
                img.overrideSprite = maps[2];

            }

            if (!x.activeInHierarchy)
            {
                x.SetActive((true));
            }
        }
	}
}
