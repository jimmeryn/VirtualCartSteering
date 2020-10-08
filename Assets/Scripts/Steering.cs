using UnityEngine;

public class Steering : MonoBehaviour {
  private GameObject target;
  private Vector3 velocity = Vector3.zero;
  private Rigidbody rigidBody;
  private float speed = 50.0f;
  private float maxHitDistance;
  private float hitDistance;
  private Vector3 origin;
  private Vector3 direction;
  private Vector3 cubeSize;

  private void Start() {
    target = GameObject.FindWithTag("Target");
    rigidBody = GetComponent<Rigidbody>();
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
        //velocity = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z).normalized * speed;
        // Use transform.position ... MoveTowards here?
      } 
      //else {
        // Using ObstacleAvoidance in this case
        // velocity = Vector3.zero;
      //}
      transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
      // Dont know if using transform.position is good idea so i left rigidBody.
      // Delete this if no longer needed
      //rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
    }
  }

  private void OnDrawGizmos() {
    Gizmos.color = Color.blue;
    Gizmos.DrawLine(origin, origin + direction * hitDistance);
    Gizmos.DrawWireCube(origin + direction * hitDistance, cubeSize);
  }

  float CalculateRangeToTarget(Vector3 playerPosition, Vector3 targetPosition) {
    return (targetPosition - playerPosition).magnitude;
  }
}
