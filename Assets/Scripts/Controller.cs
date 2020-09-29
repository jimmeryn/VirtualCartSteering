using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour {
  public float speed = 5;
  Rigidbody rigidBody;
  Vector3 velocity;

  void Start() {
    rigidBody = GetComponent<Rigidbody>();
  }

  void Update() {
    velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;
  }

  void OnCollisionEnter(Collision collision) {
    if (collision.collider.tag == "Obstacle" || collision.collider.tag == "Target") {
      enabled = false;
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
  }

  void FixedUpdate() {
    rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
  }
}
