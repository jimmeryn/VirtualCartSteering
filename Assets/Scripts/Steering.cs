using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour {
  private const float speed = 10;
  private const float toleranceValue = 1f;
  private const float safetyRadius = 5f;

  private float distance;
  private float minDistance = 30;
  private Vector3 targetPosition;
  private Vector3 direction;
  private Raycasting raycasting;
  public List<Vector3> trajectory = new List<Vector3>();
  private readonly List<Vector3> T = new List<Vector3>();
  //private List<Vector3> Tv = new List<Vector3>();
  private Vector3 startPosition;
  Vector3 localTarget;
  private float angleTolerance = 10;

  public MeshFilter viewMeshFilter;
  private Mesh viewMesh;


  // DEBUG
  private readonly List<Vector3> drawingList = new List<Vector3>();
  bool DEBUG = true;


  private void Start() {
    viewMesh = new Mesh {
      name = "View Mesh"
    };
    viewMeshFilter.mesh = viewMesh;

    targetPosition = GameObject.FindWithTag(Tags.Target).transform.position;
    startPosition = transform.position;
    raycasting = new Raycasting(transform);
    distance = Functions.VectorLength(startPosition, targetPosition);
    trajectory.Add(startPosition);
    AStar.Init(startPosition, targetPosition);
  }

  private void LateUpdate() {
    Algorithm();
    MeshCreator();
  }

  void Algorithm() {
    trajectory.Add(transform.position);
    float d = Functions.VectorLength(transform.position, targetPosition);
    distance = d > minDistance ? d : minDistance;
    // 2.1. Distance check
    raycasting.DistanceCheck(distance);
    //2.2.Check if goal is visible
    direction = (targetPosition - transform.position).normalized;
    if (
      (raycasting.minRaysList.Count <= 0 || !(raycasting.minRaysList[0].distance <= safetyRadius)) &&
      Physics.Raycast(transform.position, direction, out RaycastHit hit) &&
      hit.collider.CompareTag(Tags.Target)) {
      Projection(targetPosition);
    } else {
      if (raycasting.minRaysList.Count >= 1 && raycasting.minRaysList[0].distance <= safetyRadius) {
        MoveAway(raycasting.minRaysList[0].point);
      } else {
        // 3. Compare
        Compare(raycasting.minRaysList.ToArray());
      }
    }
  }

  private void Compare(RaycastInfo[] minRays) {
    // 3.1. Out of ME
    if (minRays.Length >= 2 && Functions.CalculateError(minRays[0].distance, minRays[1].distance) > toleranceValue) {
      Debug.Log("OUT OF ME");
      Correction(transform.position, minRays[0].point, minRays[1].point);
    } else
    if (minRays.Length >= 3 &&
      Functions.CalculateError(minRays[0].distance, minRays[1].distance) <= toleranceValue &&
      Functions.CalculateError(minRays[0].distance, minRays[2].distance) <= toleranceValue) {
      // 3.3. Robot is located on a medial axis vertex - maintains max distance from 3 obstacles
      if (T.Contains(transform.position)) {
        Debug.Log("ME VERTEX LOOP");
        LoopHandle();
      } else {
        Debug.Log("ME VERTEX");
        CalculateLocalTarget(transform.position, minRays[0], minRays[1], minRays[2]);
        localTarget = AStar.GetBestNode();
        Projection(localTarget);
      }
    } else if (minRays.Length >= 3 && Functions.CalculateError(minRays[0].distance, minRays[1].distance) <= toleranceValue) {
      // 3.2. Robot is located on medial axis edge - maintains max distance from 2 obstacles
      Debug.Log("MA EDGE");
      CalculateLocalTarget(transform.position, minRays[0], minRays[1]);
      localTarget = AStar.GetBestNode();
      Projection(localTarget);
    } else {
      Debug.Log("ELSE");
      Projection(targetPosition);
    }
  }

  private void Correction(Vector3 currentPosition, Vector3 p1, Vector3 p2) {
    var pm1 = p1 - currentPosition;
    var pm2 = p2 - currentPosition;
    var corectionStepLength = (pm2.magnitude - pm1.magnitude) / 2;
    localTarget = transform.TransformPoint(-pm1.normalized * corectionStepLength);
    Projection(localTarget);
    T.Add(transform.position);
  }

  private void CalculateLocalTarget(Vector3 currentPosition, RaycastInfo p1, RaycastInfo p2) {
    Vector3 pm1 = p1.point - currentPosition;
    Vector3 pm2 = p2.point - currentPosition;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = -v1;

    if ((p1.angle + p2.angle) % 180 <= angleTolerance) {
      v1 = new Vector3(-pm1.z, 1, pm1.x);
      v2 = -v1;
    }

    drawingList.Clear();
    drawingList.Add(v1.normalized * pm1.magnitude);
    drawingList.Add(v2.normalized * pm1.magnitude);

    AStar.AddToOpenList(transform.position, v1.normalized * pm1.magnitude);
    AStar.AddToOpenList(transform.position, v2.normalized * pm1.magnitude);
  }

  private void CalculateLocalTarget(Vector3 currentPosition, RaycastInfo p1, RaycastInfo p2, RaycastInfo p3) {
    Vector3 pm1 = p1.point - currentPosition;
    Vector3 pm2 = p2.point - currentPosition;
    Vector3 pm3 = p3.point - currentPosition;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = pm2 + pm3;
    Vector3 v3 = pm3 + pm1;

    if ((p1.angle + p2.angle) % 180 <= angleTolerance) {
      v1 = new Vector3(-pm1.z, 1, pm1.x);
    } else if ((p2.angle + p3.angle) % 180 <= angleTolerance) {
      v2 = new Vector3(-pm2.z, 1, pm2.x);
    } else if ((p1.angle + p3.angle) % 180 <= angleTolerance) {
      v3 = new Vector3(-pm1.z, 1, pm1.x);
    }


    drawingList.Clear();
    drawingList.Add(v1.normalized * pm1.magnitude);
    drawingList.Add(v2.normalized * pm1.magnitude);
    drawingList.Add(v3.normalized * pm1.magnitude);

    AStar.AddToOpenList(transform.position, v1.normalized * pm1.magnitude);
    AStar.AddToOpenList(transform.position, v2.normalized * pm1.magnitude);
    AStar.AddToOpenList(transform.position, v3.normalized * pm1.magnitude);
  }


  private void LoopHandle() {
    // DEBUG - TODO implement serious loop handle
    Projection(targetPosition);
  }

  private void Projection(Vector3 worldPointLocation) {
    trajectory.Add(transform.position);
    transform.position = Vector3.MoveTowards(
      transform.position,
      new Vector3(worldPointLocation.x, 1, worldPointLocation.z),
      Time.deltaTime * speed);
  }

  private void MoveAway(Vector3 obstacle) {
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(obstacle.x, 1, obstacle.z), -speed * Time.deltaTime);
  }

  private void MeshCreator() {
    int vertexCount = raycasting.raysMeshList.Count + 1;
    Vector3[] vertices = new Vector3[vertexCount];
    int[] triangles = new int[(vertexCount - 1) * 3];
    vertices[0] = Vector3.zero;
    for (int i = 0; i < vertexCount - 1; i++) {
      vertices[i + 1] = transform.InverseTransformPoint(raycasting.raysMeshList[i]);
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

  private void OnDrawGizmos() {
    if (DEBUG) {
      //Drawing DEBUG DRAWING LIST
      Gizmos.color = Color.red;

      foreach (var d in drawingList) {
        Gizmos.DrawSphere(transform.TransformPoint(d), 2f);
        //Gizmos.DrawLine(transform.position, d);
      }

      //Drawing LOCAL TARGET
      Gizmos.color = Color.yellow;
      Gizmos.DrawSphere(transform.TransformPoint(localTarget), 1f);

      //Drawing SAFETY RADIUS
      Gizmos.color = Color.grey;
      Gizmos.DrawWireSphere(transform.position, safetyRadius);

      Gizmos.color = Color.red;
      DrawRay(0);
      DrawRay(1);
      Gizmos.color = Color.yellow;
      DrawRay(2);
    }
  }
  private void DrawRay(int index) {
    if (raycasting.minRaysList != null && raycasting.minRaysList.Count > index) {
      Gizmos.DrawLine(transform.position, raycasting.minRaysList[index].point);
    }
  }
}
