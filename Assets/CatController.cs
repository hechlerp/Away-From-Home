using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : MonoBehaviour {
    bool idle = false;
    //state:roaming, staying in the spot
    public void Alarm(){
        if (waveexpanding) return;

        wave.gameObject.SetActive(true);
        waveexpanding = true;
        Debug.Log(("scream"));
        GetComponent<EnemyNavigation>().enabled = false;
        GetComponent<Pathfinding.AIPath>().enabled=false;
        foreach(GameObject x in InRangeParents){
            x.GetComponent<EnemyNavigation>().inspectLocation(transform.position);

        }
        //        GetComponent<Animator>().Play("Scream");

    }
    public Transform wave;
    bool waveexpanding = false;
    List<GameObject> InRangeParents = new List<GameObject>();
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!InRangeParents.Contains((collision.gameObject))){
            InRangeParents.Add((collision.gameObject));

        }

    }private void OnTriggerExit2D(Collider2D collision)
    {
        if (InRangeParents.Contains((collision.gameObject)))
        {
            InRangeParents.Remove((collision.gameObject));

        }

    }
    void Start () {
		
	}

    float elapsed = 0;
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
        elapsed += Time.deltaTime;
        if (elapsed > 2){
            elapsed = 0;
            GetComponent<EnemyNavigation>().enabled=false;
        }
	}
}
