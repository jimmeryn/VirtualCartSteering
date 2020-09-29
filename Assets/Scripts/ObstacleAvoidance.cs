using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour {
  [Range(0, 360)]
  public float viewAngle = 360;

  [Range(10, 100)]
  public float range = 50;

  [Range(1, 100)]
  public float step = 10;

  void FixedUpdate() {
    RaycastHit hit;

    for (float i = 0; i < viewAngle; i += step) {
      float angle = (-viewAngle / 2) + i;
      Vector3 direction = DirectionFromAngle(angle) * range;
      if (Physics.Raycast(transform.position, direction, out hit, range)) {
        Debug.DrawLine(transform.position, transform.position + direction, Color.red);
      } else {
        Debug.DrawLine(transform.position, transform.position + direction, Color.green);
      }
    }
  }

  Vector3 DirectionFromAngle(float angle) {
    return Quaternion.Euler(0, angle, 0) * transform.forward;
  }
}
