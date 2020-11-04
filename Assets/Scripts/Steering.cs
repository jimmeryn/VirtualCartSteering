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
