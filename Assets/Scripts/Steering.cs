using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour {
  private const float speed = 10.0f;
  private const float toleranceValue = 4f;
  private const float safetyRadius = 4f;

  private float distance;
  private Vector3 targetPosition;
  private Vector3 direction;
  private Raycasting raycasting;
  public List<Vector3> trajectory = new List<Vector3>();
  private readonly List<Vector3> T = new List<Vector3>();
  //private List<Vector3> Tv = new List<Vector3>();
  private Vector3 startPosition;
  Vector3 localTarget;

  public MeshFilter viewMeshFilter;
  private Mesh viewMesh;

  // DEBUG
  private readonly List<Vector3> drawingList = new List<Vector3>();


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
      hit.collider.tag.Equals(Tags.Target)
    ) {
      Departure(hit.point);
    } else {
      if (raycasting.minRaysList.Count >= 1 &&
          Functions.VectorLength(transform.position, raycasting.minRaysList[0].point) <= safetyRadius) {
        localTarget = transform.TransformPoint(raycasting.minRaysList[0].point);
        MoveAway(raycasting.minRaysList[0].point);
      } else
      if (raycasting.minRaysList.Count >= 3) {
        // 3. Compare
        Compare(raycasting.minRaysList);
      } else {
        // If cannot Comapare - not enought min rays search for more obstacles
        distance += 5;
      }
    }
  }

  private void Compare(List<RaycastInfo> minRays) {
    // 3.1. Out of ME
    if (Functions.VectorLength(minRays[0].point, minRays[1].point) > toleranceValue) {
      Debug.Log("3.1");
      Correction(transform.position, minRays[0].point, minRays[1].point);
    } else
    // 3.3. Robot is located on a medial axis vertex - maintains max distance from 3 obstacles
    if (Functions.VectorLength(minRays[0].point, minRays[1].point) <= toleranceValue &&
        Functions.VectorLength(minRays[0].point, minRays[2].point) <= toleranceValue) {
      Debug.Log("3.3");
      if (T.Contains(transform.position)) {
        // DEBUG
        trajectory.Add(transform.position);
        localTarget = CalculateLocalTarget(transform.position, minRays[0].point, minRays[1].point, minRays[2].point);
        Projection(localTarget);
        // DEBUG END

        //LoopHandle();
      } else {
        trajectory.Add(transform.position);
        localTarget = CalculateLocalTarget(transform.position, minRays[0].point, minRays[1].point, minRays[2].point);
        Projection(localTarget);
      }
    } else
    // 3.2. Robot is located on medial axis edge - maintains max distance from 2 obstacles
    if (Functions.VectorLength(minRays[0].point, minRays[1].point) <= toleranceValue) {
      Debug.Log("3.2");
      if (Functions.VectorLength(transform.position, minRays[0].point) <= safetyRadius) {
        // 3.2.1. Robot to close to obstacle
        Debug.Log("backtracking");
        //Backtracking();
        MoveAway(minRays[0].point);
      } else {
        // ERROR HERE!!!!!!!!!
        Debug.Log("follow edge");
        trajectory.Add(transform.position);
        localTarget = CalculateLocalTarget(transform.position, minRays[0].point, minRays[1].point);
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

  private void Projection(Vector3 target) {
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, 1, target.z), Time.deltaTime * speed);
    // NOTE: use transform.position = Vector3.MoveTowards or transfrorm.translate
    //transform.Translate(target * Time.deltaTime * speed);
  }

  private void MoveAway(Vector3 obstacle) {
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(obstacle.x, 1, obstacle.z), -speed * Time.deltaTime);
  }



  private void Correction(Vector3 currentPosition, Vector3 p1, Vector3 p2) {
    var pm1 = p1 - currentPosition;
    var pm2 = p2 - currentPosition;
    var corectionStepLength = (pm2.magnitude - pm1.magnitude) / 2;
    localTarget = new Vector3(-pm1.x, 1, -pm1.y).normalized * corectionStepLength;
    MoveAway(new Vector3(-pm1.x, 1, -pm1.y).normalized * corectionStepLength);
    //T.Add(transform.position, J(x));
    T.Add(transform.position);
  }

  private void Departure(Vector3 target) {
    trajectory.Add(transform.position);
    trajectory.Add(target);
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, 1, target.z), Time.deltaTime * speed);
  }

  private Vector3 CalculateLocalTarget(Vector3 position, Vector3 p1, Vector3 p2) {
    Vector3 pm1 = p1 - position;
    Vector3 pm2 = p2 - position;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = -v1;
    drawingList.Clear();
    drawingList.Add(v1.normalized * pm1.magnitude);
    drawingList.Add(v2.normalized * pm1.magnitude);
    Vector3 node = AStar.CalculateBestNode(transform.position, new Vector3[] { v1, v2 });
    return node.normalized * pm1.magnitude;
  }

  private Vector3 CalculateLocalTarget(Vector3 position, Vector3 p1, Vector3 p2, Vector3 p3) {
    Vector3 pm1 = p1 - position;
    Vector3 pm2 = p2 - position;
    Vector3 pm3 = p3 - position;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = pm2 + pm3;
    Vector3 v3 = pm3 + pm1;
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
    // Drawing SAFETY CIRCLE
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, safetyRadius);
    
    // Drawing MAX DISTANCE CIRCLE
    //Gizmos.DrawWireSphere(transform.position, distance);
    
    // Drawing DEBUG DRAWING LIST
    Gizmos.color = Color.red;
    foreach (var d in drawingList) {
      Gizmos.DrawSphere(transform.TransformPoint(d), 1.5f);
    }

    // Drawing LOCAL TARGET
    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(transform.TransformPoint(localTarget), 1f);

    if (raycasting.minRaysList != null && raycasting.minRaysList.Count > 0) {
      // Drawing CLOSEST RAY CIRCLE
      //Gizmos.DrawWireSphere(transform.position, raycasting.minRaysList[0].distance);

      // Drawing N CLOSEST RAYS
      int N = 3;
      Gizmos.color = Color.gray;
      for (int i = 0; i < N; i++) {
        if (raycasting.minRaysList.Count > i)
          Gizmos.DrawLine(transform.position, raycasting.minRaysList[i].point);
      }
    }
  }
}
