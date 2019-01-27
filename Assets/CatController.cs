using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour {
    bool idle = false;
    Transform folder;

    //state:roaming, staying in the spot
    public void Alarm(){
        if (waveexpanding) return;
        waiting = true;
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
                folder.GetChild(i).GetComponent<EnemyNavigation>().inspectLocation(transform.position);

            }
        }
    }

//    bool calling = false;
    public Transform wave;
    bool waveexpanding = false;
    public List<GameObject> InRangeParents = new List<GameObject>{};


    void Start () {
        folder = GameObject.Find("EnemyHolder").transform;
	}

    float elapsed = 0;
    bool waiting = false;
	// Update is called once per frame
	void Update () {
        if (waveexpanding){
            wave.localScale *= 1.1f;
            if (wave.localScale.x>=50){
                wave.localScale = new Vector3(1, 1, 1);
                wave.gameObject.SetActive((false));
                waveexpanding = false;

            }
        }
        if (waiting)
        {
            elapsed += Time.deltaTime;
            if (elapsed > 2)
            {
                elapsed = 0;
                waiting = false;
                GetComponent<Pathfinding.AIPath>().enabled = true;
                GetComponent<EnemyNavigation>().enabled = true;
            }
        }
	}
}
