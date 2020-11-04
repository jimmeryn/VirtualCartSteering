using UnityEngine;

public class Steering : MonoBehaviour {
  private GameObject target;
  private float speed = 50.0f;
  private float maxHitDistance;
  private float hitDistance;
  private Vector3 origin;
  private Vector3 direction;
  private Vector3 cubeSize;

  private void Start() {
    target = GameObject.FindWithTag(Tags.Target);
    maxHitDistance = CalculateRangeToTarget(transform.position, target.transform.position);
    hitDistance = maxHitDistance;
    cubeSize = transform.localScale;
  }

  private void Update() {
    origin = transform.position;
    direction = (target.transform.position - transform.position).normalized;
    maxHitDistance = (transform.position - target.transform.position).magnitude;
  }

  private void FixedUpdate() {
    RaycastHit hit;
    if (Physics.BoxCast(origin, cubeSize / 2, direction, out hit, transform.rotation)) {
      hitDistance = hit.distance;
      if (hit.collider.tag.Equals(Tags.Target) || maxHitDistance < 5.0f) {
        transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
      }
    }
  }

  // DEBUG only
  private void OnDrawGizmos() {
    Gizmos.color = Color.blue;
    Gizmos.DrawLine(origin, origin + direction * hitDistance);
    Gizmos.DrawWireCube(origin + direction * hitDistance, cubeSize);
    if (obstacleAvoidance != null && obstacleAvoidance.raycastList != null) {
      obstacleAvoidance.OnDrawGizmos();
    }
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(localTarget, 3f);
  }
}
