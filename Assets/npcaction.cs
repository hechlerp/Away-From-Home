using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class npcaction : MonoBehaviour
{

    // Use this for initialization
    bool ismoving;
    bool moved;
    private GameObject player;
    private float[] playerpos = new float[2];

    void Start()
    {
        Interactable interactable = GetComponentInChildren<Interactable>();
        interactable.setAction(npcMove);
        ismoving = false;
        moved = false;
        player = GameObject.Find("Player");

    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(playerpos[0] - transform.position.x) < 2f)
        {
            //player at top or bottom of npc
            if (playerpos[1] > transform.position.y)
            {
                //player on top, NPC moves up
                if (transform.position.y < playerpos[1])
                {
                    //might have to incorporate npc moving animation here, so moving might have to be implemented differently from just changing transform.position
                    Debug.Log("moving up");
                }
            }
            else
            {
                //player on bottom, NPC moves down
                if (transform.position.y > playerpos[1])
                {
                    //might have to incorporate npc moving animation here, so moving might have to be implemented differently from just changing transform.position
                    Debug.Log("moving down");
                }
            }
        }

        else
        {
            //player at left right of npc
            if (playerpos[0] < transform.position.y)
            {
                //player on left, NPC moves left
                if (transform.position.x > playerpos[0])
                {
                    //might have to incorporate npc moving animation here, so moving might have to be implemented differently from just changing transform.position
                    Debug.Log("moving left");
                }
            }
            else
            {
                //player on bottom, NPC moves down
                if (transform.position.x < playerpos[0])
                {
                    //might have to incorporate npc moving animation here, so moving might have to be implemented differently from just changing transform.position
                    Debug.Log("moving right");
                }
            }
        }

    }

    void npcMove()
    {
        if (!moved)
        {
            playerpos[0] = player.transform.position.x;
            playerpos[1] = player.transform.position.y;
            ismoving = true;
            moved = true;
            Debug.Log("hey");
           
        }
    }
}
