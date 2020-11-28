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
    distance = Functions.VectorLength(transform.position, targetPosition);
    // 2.1. Distance check
    raycasting.DistanceCheck(distance);
    //2.2.Check if goal is visible
    direction = (targetPosition - transform.position).normalized;
    if (
      Physics.BoxCast(transform.position, transform.localScale / 2, direction, out RaycastHit hit, transform.rotation) &&
      hit.collider.CompareTag(Tags.Target)) {
      Departure(hit.point);
    } else {
      if (raycasting.minRaysList.Count >= 1 && raycasting.minRaysList[0].distance <= safetyRadius) {
        MoveAway(raycasting.minRaysList[0].point);
      } else {
        // 3. Compare
        Compare(raycasting.minRaysList);
      }
    }
  }

  private void Compare(List<RaycastInfo> minRays) {
    var e1 = Functions.CalculateError(minRays[0].distance, minRays[1].distance);
    // 3.1. Out of ME
    if (minRays.Count >= 2 && e1 > toleranceValue) {
      Correction(transform.position, minRays[0].point, minRays[1].point);
    } else if (minRays.Count >= 3 && e1 <= toleranceValue && 
      Functions.CalculateError(minRays[0].distance, minRays[2].distance) <= toleranceValue) {
      // 3.3. Robot is located on a medial axis vertex - maintains max distance from 3 obstacles
      Debug.Log("3.3");
      if (T.Contains(transform.position)) {
        // DEBUG
        trajectory.Add(transform.position);
        localTarget = CalculateLocalTarget(transform.position, minRays[0], minRays[1], minRays[2]);
        Projection(localTarget);
        // DEBUG END

        //LoopHandle();
      } else {
        trajectory.Add(transform.position);
        localTarget = CalculateLocalTarget(transform.position, minRays[0], minRays[1], minRays[2]);
        Projection(localTarget);
      }
    } else if (minRays.Count >= 2 && e1 <= toleranceValue) {
      // 3.2. Robot is located on medial axis edge - maintains max distance from 2 obstacles
      Debug.Log("3.2");
      if (minRays[0].distance <= safetyRadius) {
        // 3.2.1. Robot to close to obstacle
        Debug.Log("backtracking");
        //Backtracking();
        MoveAway(minRays[0].point);
      } else {
        Debug.Log("follow edge");
        trajectory.Add(transform.position);
        T.Add(transform.position);
        localTarget = CalculateLocalTarget(transform.position, minRays[0], minRays[1]);
        Projection(localTarget);
      }
    }
  }

  private void Backtracking() {
    // remove all points until last visited vertex
    // trajectory.Remove(all points until last visited vertex)
    // DE.Add(all points until last visited vertex) -> newTraj = oldTraj \ DE
    // mark removed edge as dead end -> not to be explored again
    // go back to last viisted vertex
  }

  private void LoopHandle() {
    //Tv.Add(transform.position);
    //if (unexploredEdgeExists) {
    //  Projection(unexploredEdge);
    //} else {
    //  Projection(lastVisitedEdge);
    //}
  }

  private void Projection(Vector3 localPointLocation) {
    var worldPointLocation = localPointLocation;
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(worldPointLocation.x, 1, worldPointLocation.z), Time.deltaTime * speed);
    // NOTE: use transform.position = Vector3.MoveTowards or transfrorm.translate
    //transform.Translate(target * Time.deltaTime * speed);
  }

  private void Correction(Vector3 currentPosition, Vector3 p1, Vector3 p2) {
    var pm1 = p1 - currentPosition;
    var pm2 = p2 - currentPosition;
    var corectionStepLength = (pm2.magnitude - pm1.magnitude) / 2;
    MoveAway(p1.normalized * corectionStepLength);
    //T.Add(transform.position, J(x));
    T.Add(transform.position);
  }

  private void MoveAway(Vector3 obstacle) {
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(obstacle.x, 1, obstacle.z), -speed * Time.deltaTime);
  }

  private void Departure(Vector3 target) {
    trajectory.Add(transform.position);
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, 1, target.z), speed * Time.deltaTime);
  }

  private Vector3 CalculateLocalTarget(Vector3 position, RaycastInfo p1, RaycastInfo p2) {
    drawingList.Clear();
    drawingList.Add(p1.point);
    drawingList.Add(p2.point);
    Vector3 pm1 = p1.point - position;
    Vector3 pm2 = p2.point - position;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = -v1;
    if ((p1.angle + p2.angle) % 180 <= angleTolerance) {
      v1 = new Vector3(-pm1.z, 1, pm1.x);
      v2 = -v1;
    }
    drawingList.Clear();
    drawingList.Add(v1.normalized * pm1.magnitude);
    drawingList.Add(v2.normalized * pm1.magnitude);
    Vector3 node = AStar.CalculateBestNode(transform.position, new Vector3[] { v1, v2 });
    return node.normalized * pm1.magnitude;
  }

  private Vector3 CalculateLocalTarget(Vector3 position, RaycastInfo p1, RaycastInfo p2, RaycastInfo p3) {
    drawingList.Clear();
    drawingList.Add(p1.point);
    drawingList.Add(p2.point);
    drawingList.Add(p3.point);
    Vector3 pm1 = p1.point - position;
    Vector3 pm2 = p2.point - position;
    Vector3 pm3 = p3.point - position;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = pm2 + pm3;
    Vector3 v3 = pm3 + pm1;

    if ((p1.angle + p2.angle) % 180 <= angleTolerance) {
      v1 = new Vector3(-pm1.z, 1, pm1.x);
      return AStar.CalculateBestNode(transform.position, new Vector3[] { v1, v2, v3 }).normalized * pm1.magnitude;
    } else if ((p2.angle + p3.angle) % 180 <= angleTolerance) {
      v2 = new Vector3(-pm2.z, 1, pm2.x);
      return AStar.CalculateBestNode(transform.position, new Vector3[] { v1, v2, v3 }).normalized * pm1.magnitude;
    } else if ((p1.angle + p3.angle) % 180 <= angleTolerance) {
      v3 = new Vector3(-pm1.z, 1, pm1.x);
      return AStar.CalculateBestNode(transform.position, new Vector3[] { v1, v2, v3 }).normalized * pm1.magnitude;
    }

    drawingList.Clear();
    drawingList.Add(v1.normalized * pm1.magnitude);
    drawingList.Add(v2.normalized * pm1.magnitude);
    drawingList.Add(v3.normalized * pm1.magnitude);
    Vector3 node = AStar.CalculateBestNode(transform.position, new Vector3[] { v1, v2, v3 });
    return node.normalized * pm1.magnitude;
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
        Gizmos.DrawSphere(transform.TransformPoint(d), 5f);
        //Gizmos.DrawLine(transform.position, d);
      }

      //Drawing LOCAL TARGET
      Gizmos.color = Color.yellow;
      Gizmos.DrawSphere(transform.TransformPoint(localTarget), 3f);

      Gizmos.color = Color.grey;
      Gizmos.DrawWireSphere(transform.position, safetyRadius);

      int N = 2;
      if (raycasting.minRaysList != null && raycasting.minRaysList.Count >= N) {
        // Drawing CLOSEST RAY CIRCLE
        //Gizmos.DrawWireSphere(transform.position, raycasting.minRaysList[0].distance);

        // Drawing N CLOSEST RAYS
        Gizmos.color = Color.red;
        for (int i = 0; i < N; i++) {
          if (raycasting.minRaysList.Count >= i) {
            Gizmos.DrawLine(transform.position, raycasting.minRaysList[i].point);
          }
        }
      }
    }
  }
}
