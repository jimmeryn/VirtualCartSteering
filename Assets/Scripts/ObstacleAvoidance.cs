using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour {
  [Range(0, 360)]
  public float viewAngle = 360;

  [Range(1, 100)]
  public float step = 1;

  private GameObject target;

  private float range = 10;

  private List<RaycastInfo> raycastList = new List<RaycastInfo>();
  
  private Graph graph;

  private void Start() {
    target = GameObject.FindWithTag(Tags.Target);
  }

  void FixedUpdate() {
    range = CalculateRangeToTarget(transform.position, target.transform.position);
    CastAllRays();
    graph = new Graph(raycastList, transform.position, target.transform.position);
    DrawDebugLines();
  }

  void CastAllRays() {
    raycastList.Clear();
    for (float i = 0; i < viewAngle; i += step) {
      float angle = (-viewAngle / 2) + i;
      Vector3 direction = DirectionFromAngle(angle) * range;
      CastRay(direction, angle);
    }
  }

  void CastRay(Vector3 direction, float angle) {
    RaycastHit hit;
    if (Physics.Raycast(transform.position, direction, out hit, range)) {
      if (hit.collider.tag.Equals(Tags.Obstacle)) {
        raycastList.Add(new RaycastInfo(true, hit.point, hit.distance, angle));
      } else if (hit.collider.tag.Equals(Tags.Wall)) {
        raycastList.Add(new RaycastInfo(true, hit.point, hit.distance, angle));
      } else {
        raycastList.Add(new RaycastInfo(false, hit.point, hit.distance, angle));
      }
    } else {
      raycastList.Add(new RaycastInfo(false, transform.position + direction, range, angle));
    }
  }

  void DrawDebugLines() {
    foreach(RaycastInfo rayInfo in raycastList) {
      if (rayInfo.hit) {
        Debug.DrawLine(transform.position, rayInfo.point, Color.red);
      } else {
        Debug.DrawLine(transform.position, rayInfo.point, Color.green);
      }
    }
  }

  Vector3 DirectionFromAngle(float angle) {
    return Quaternion.Euler(0, angle, 0) * transform.forward;
  }

  float CalculateRangeToTarget(Vector3 playerPosition, Vector3 targetPosition) {
    return (targetPosition - playerPosition).magnitude;
  }
}
