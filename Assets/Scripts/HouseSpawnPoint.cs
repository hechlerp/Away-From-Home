using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseSpawnPoint : MonoBehaviour {
    public GameObject enemy;
    public Vector3 spawnLocation;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void spawnParent(Vector2[] navPoints) {
        GameObject enemyParent = Instantiate(enemy);
        enemyParent.transform.position = transform.position + spawnLocation;
        EnemyNavigation en = enemyParent.GetComponent<EnemyNavigation>();
        en.speed = 3f;
        en.navPoints = navPoints;
    }
}
