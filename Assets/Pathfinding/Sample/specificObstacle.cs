using UnityEngine;
using System.Collections;
using AggregatGames.AI.Pathfinding;

public class specificObstacle : Obstacle {
	public string forPathfinder;

	public override bool isObstacle(Pathfinder pathfinder) {
		return (pathfinder.identification == forPathfinder);
	}
}
