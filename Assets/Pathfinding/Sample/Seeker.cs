using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AggregatGames.AI.Pathfinding;

[AddComponentMenu ("AggregatGames/Pathfinding/Seeker")]
[RequireComponent (typeof(AggregatGames.AI.Pathfinding.Pathfinder))]
[RequireComponent (typeof(CharacterController))]
/**
 *You are allowed to change this script in order to get the best out of your Pathfinder
 **/
public class Seeker2 : MonoBehaviour {
	public Transform target;
	public float rotationSpeed = 20f;
	public float walkingSpeed = 2f;
	public float reachedKnot = 1f;
	private Pathfinder pathfinder;
	private CharacterController characterController;
	private List<PathKnot> knots = new List<PathKnot>();
	private int knotIndex = 1;
	private int state = -1;

	private Path path;

	void Start () {
		pathfinder = gameObject.GetComponent<AggregatGames.AI.Pathfinding.Pathfinder>();
		characterController =  gameObject.GetComponent<CharacterController>();
	}

	void Update () {
		if (knots.Count == 0 && state != -2 || target.position != pathfinder.target) {
			if (knots.Count == 0) pathfinder.findPath(transform.position, target.position, foundPath);
			else pathfinder.findPath(knots[knotIndex].position, target.position, foundPath);
		}
		if (knots.Count != 0 && knotIndex < knots.Count) {
			Vector3 lookPos = knots[knotIndex].position - transform.position;
			lookPos.y = 0;
			Quaternion rotation = Quaternion.LookRotation(lookPos);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

			Vector3 walkdir = transform.root.forward;
			walkdir.y = (knots[knotIndex].position - transform.position).y;

			if (knotIndex+1 >= knots.Count) {
				return;
			}

			if (path.blockedByDynamicObstacle(knots[knotIndex], knots[knotIndex+1])) {
				return;
			}

			characterController.SimpleMove(walkdir*walkingSpeed);
			if (Vector3.Distance(transform.position, knots[knotIndex].position) <= reachedKnot) {
				RaycastHit hit;
				bool isHitting = Physics.Raycast(transform.position, (knots[knotIndex].position-transform.position).normalized, out hit, Vector3.Distance(transform.position, knots[knotIndex].position));
				if (isHitting && hit.collider.gameObject.tag != pathfinder.obstacleTag) {
					Obstacle obstacle = hit.collider.gameObject.GetComponent<Obstacle>();
					if (obstacle == null) {
						if (knotIndex < knots.Count-1) knotIndex++;
					} else if (!obstacle.isObstacle(pathfinder)) if (knotIndex < knots.Count-1) knotIndex++;
				} else if (!isHitting) {
					if (knotIndex < knots.Count-1) knotIndex++;
					else knots = new List<PathKnot>();//DONE
				}
			}
		}
	}

	public void foundPath(Pathinfo info) {
		if (info.foundPath) {
			path = info.path;
			if (knots.Count == 0) {
				knotIndex = 1;
				knots = info.path.getPathList();
			} else {
				knots.RemoveRange(knotIndex, knots.Count-knotIndex);
				knots.AddRange(info.path.getPathList());
			}
		} else {
			Debug.Log(info.comment);
			state = -2;
		}
	}
}
