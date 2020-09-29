using UnityEngine;

public class Steering : MonoBehaviour {
  private GameObject target;
  private Vector3 velocity = Vector3.zero;
  private Rigidbody rigidBody;
  private float speed = 5.0f;

  private void Start() {
    target = GameObject.FindWithTag("Target");
    rigidBody = GetComponent<Rigidbody>();
  }
  void FixedUpdate() {
    RaycastHit hit;
    if (Physics.Raycast(transform.position, target.transform.position, out hit)) {
      Debug.DrawLine(transform.position, target.transform.position);
    }
    rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
  }

  void Update() {
    RaycastHit hit;

    if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out hit)) {
      Debug.DrawLine(transform.position, target.transform.position);
      if (hit.collider.tag == "Target") {
        velocity = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z).normalized * speed;
      } else {
        velocity = Vector3.zero;
      }
    }
  }
}
