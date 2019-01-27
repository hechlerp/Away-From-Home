using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour {
    bool idle = false;
    Transform folder;

    //state:roaming, staying in the spot
    public void Alarm(){
        if (waveexpanding) return;

        wave.gameObject.SetActive(true);
        waveexpanding = true;
        GetComponent<EnemyNavigation>().enabled = false;
        GetComponent<Pathfinding.AIPath>().enabled=false;
        //  calling = true;

        //        GetComponent<Animator>().Play("Scream");
        for (int i = 0; i < folder.childCount; i++)
        {
            //            Debug.Log(Vector3.Distance(transform.position, folder.GetChild(i).position));
            if (Vector3.Distance(transform.position, folder.GetChild(i).position) < 15)
            {
                //Alarm();
                folder.GetChild(i).GetComponent<EnemyNavigation>().inspectLocation(new Vector2(transform.position.x,transform.position.y));

            }
        }
    }

//    bool calling = false;
    public Transform wave;
    bool waveexpanding = false;
    public List<GameObject> InRangeParents = new List<GameObject>{};

    /*


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!InRangeParents.Contains((collision.gameObject))){
            InRangeParents.Add((collision.gameObject));

        }

    }

    private void OnTriggerExit2D(Collision2D collision)
    {
        if (InRangeParents.Contains((collision.gameObject)))
        {
            InRangeParents.Remove((collision.gameObject));

        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!InRangeParents.Contains((collision.gameObject)))
        {
            InRangeParents.Add((collision.gameObject));

        }

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!InRangeParents.Contains((collision.gameObject)))
        {
            InRangeParents.Add((collision.gameObject));

        }

    }
    private void OnCollisionExit2D(Collider2D collision)
    {
        if (InRangeParents.Contains((collision.gameObject)))
        {
            InRangeParents.Remove((collision.gameObject));

        }

    }*/

    void Start () {
        folder = GameObject.Find("EnemyHolder").transform;
	}

    float elapsed = 0;
	// Update is called once per frame
	void Update () {
        /*
        for (int i = 0; i < folder.childCount; i++)
        {
//            Debug.Log(Vector3.Distance(transform.position, folder.GetChild(i).position));
            if (Vector3.Distance(transform.position, folder.GetChild(i).position) < 50)
            {
                //Alarm();
                folder.GetChild(i).GetComponent<EnemyNavigation>().inspectLocation(transform.position);

            }
        }*/

        if (waveexpanding){
            wave.localScale *= 1.1f;
            if (wave.localScale.x>=50){
                wave.localScale = new Vector3(1, 1, 1);
                wave.gameObject.SetActive((false));
                waveexpanding = false;

            }
        }
        elapsed += Time.deltaTime;
        if (elapsed > 2){
            elapsed = 0;
            GetComponent<EnemyNavigation>().enabled=false;
        }
	}
}
