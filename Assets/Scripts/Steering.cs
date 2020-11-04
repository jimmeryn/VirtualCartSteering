using System.Collections;
using UnityEngine;

public class Steering : MonoBehaviour {
  private GameObject target;
  private float speed = 50.0f;
  private float maxHitDistance;
  private float hitDistance;
  private Vector3 origin;
  private Vector3 direction;
  private Vector3 cubeSize;
  private ObstacleAvoidance obstacleAvoidance;

  public MeshFilter viewMeshFilter;
  private Mesh viewMesh;

  private Vector3 localTarget;
  private Vector3 oldTarget;


  private void Start() {
    viewMesh = new Mesh();
    viewMesh.name = "View Mesh";
    viewMeshFilter.mesh = viewMesh;
    target = GameObject.FindWithTag(Tags.Target);
    maxHitDistance = Functions.CalculateLength(transform.position, target.transform.position);
    hitDistance = maxHitDistance;
    cubeSize = transform.localScale;
    obstacleAvoidance = new ObstacleAvoidance(transform, target);

    //StartCoroutine("AlgorithmWithDelay", 0.3f);
  }

  IEnumerator AlgorithmWithDelay(float delay) {
    while (true) {
      yield return new WaitForSeconds(delay);
      Algorithm();
    }
  }

  void Algorithm() {
    origin = transform.position;
    direction = (target.transform.position - transform.position).normalized;
    maxHitDistance = (transform.position - target.transform.position).magnitude;

    obstacleAvoidance.Raycasting();
    RaycastHit hit;
    if (Physics.BoxCast(origin, cubeSize / 2, direction, out hit, transform.rotation)) {
      hitDistance = hit.distance;
      if (hit.collider.tag.Equals(Tags.Target) ||
        !obstacleAvoidance.raycastList.Exists(ray => ray.hit)) {
        transform.position = Vector3.MoveTowards(transform.position, hit.point, Time.deltaTime * speed);
      } else {
        localTarget = obstacleAvoidance.FindLocalTarget();
        if (localTarget == transform.position) {
          transform.position = Vector3.MoveTowards(transform.position, oldTarget, Time.deltaTime * speed);
        } else {
          if (localTarget.magnitude > 2) {
            transform.Translate(localTarget * Time.deltaTime * speed / 5);
          } else {
            transform.Translate(oldTarget * Time.deltaTime * speed / 5);
          }
          oldTarget = localTarget;
          transform.Translate(localTarget * Time.deltaTime * speed / 5);
        }
      }
    }
  }
  

  private void LateUpdate() {
    // DEBUG: in Prod use Coroutine for better performance
    Algorithm();
    // NOTE: Add shaders to materials for this to be visiable.
    //MeshCreator();
  }

  private void MeshCreator() {
    int vertexCount = obstacleAvoidance.raycastList.Count + 1;
    Vector3[] vertices = new Vector3[vertexCount];
    int[] triangles = new int[(vertexCount - 1) * 3];
    vertices[0] = Vector3.zero;
    for (int i = 0; i < vertexCount - 1; i++) {
      vertices[i + 1] = transform.InverseTransformPoint(obstacleAvoidance.raycastList[i].point + Vector3.forward * 0.2f);
      if (i <= vertexCount - 2) {
        if (i == vertexCount - 2) {
          // Connect to make triangles for last piece of 360
          triangles[i * 3] = 0;
          triangles[i * 3 + 1] = i + 1;
          triangles[i * 3 + 2] = 1;
        } else {
          triangles[i * 3] = 0;
          triangles[i * 3 + 1] = i + 1;
          triangles[i * 3 + 2] = i + 2;
        }
      }
    }
    viewMesh.Clear();
    viewMesh.vertices = vertices;
    viewMesh.triangles = triangles;
    viewMesh.RecalculateNormals();
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
