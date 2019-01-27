using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTimer : MonoBehaviour {

    void Start() {
        //StartCoroutine(spawnFirstParent(3));
        //StartCoroutine(spawnSecondParent(6));
    }

    IEnumerator spawnFirstParent(float time) {
        yield return new WaitForSeconds(time);
        Vector2[] firstNavPoints = new Vector2[4];
        firstNavPoints[0] = new Vector2(1, 0);
        firstNavPoints[1] = new Vector2(-1, 0);
        firstNavPoints[2] = new Vector2(-10, -10);
        firstNavPoints[3] = new Vector2(-1, 0);
        GameObject.Find("House").GetComponent<HouseSpawnPoint>().spawnParent(firstNavPoints);
    }

    IEnumerator spawnSecondParent(float time) {
        yield return new WaitForSeconds(time);
        Vector2[] secondNavPoints = new Vector2[2];
        secondNavPoints[0] = new Vector2(5, 0);
        secondNavPoints[1] = new Vector2(-5, 0);
        GameObject.Find("House").GetComponent<HouseSpawnPoint>().spawnParent(secondNavPoints);
    }
}
