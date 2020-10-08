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

  private void Start() {
    target = GameObject.FindWithTag("Target");
  }

  void FixedUpdate() {
    range = CalculateRangeToTarget(transform.position, target.transform.position);
    for (float i = 0; i < viewAngle; i += step) {
      float angle = (-viewAngle / 2) + i;
      Vector3 direction = DirectionFromAngle(angle) * range;
      CastRay(direction);
    }
  }

  Vector3 DirectionFromAngle(float angle) {
    return Quaternion.Euler(0, angle, 0) * transform.forward;
  }

  float CalculateRangeToTarget(Vector3 playerPosition, Vector3 targetPosition) {
    return (targetPosition - playerPosition).magnitude;
  }

  void CastRay(Vector3 direction) {
    RaycastHit hit;
    if (Physics.Raycast(transform.position, direction, out hit, range)) {
      if (hit.collider.tag.Equals(Tags.Obstacle)) {
        Debug.DrawLine(transform.position, hit.point, Color.red);
      } else if (hit.collider.tag.Equals(Tags.Wall)) {
        Debug.DrawLine(transform.position, hit.point, Color.yellow);
      } else {
        Debug.DrawLine(transform.position, hit.point, Color.green);
      }
    } else {
      Debug.DrawLine(transform.position, transform.position + direction, Color.green);
    }
  }
}
