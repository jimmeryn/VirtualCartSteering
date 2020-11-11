using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour {
  private const float speed = 5.0f;
  private GameObject target;
  private Vector3 direction;
  private Raycasting raycasting;
  public List<Vector3> trajectory = new List<Vector3>();
  public List<Vector3> T = new List<Vector3>();
  public List<Vector3> Tv = new List<Vector3>();
  const float toleranceValue = 5f;
  const float safetyRadius = 6f;
  const float defaultDistance = 30f;
  float distance;
  Vector3 localTarget;

  public MeshFilter viewMeshFilter;
  private Mesh viewMesh;
  // DEBUG
  public List<Vector3> drawingList = new List<Vector3>();


  private void Start() {
    viewMesh = new Mesh();
    viewMesh.name = "View Mesh";
    viewMeshFilter.mesh = viewMesh;

    target = GameObject.FindWithTag(Tags.Target);
    raycasting = new Raycasting(transform);
    distance = defaultDistance;
    trajectory.Add(transform.position);
    //StartCoroutine("AlgorithmWithDelay", 0.3f);
  }

  IEnumerator AlgorithmWithDelay(float delay) {
    while (true) {
      yield return new WaitForSeconds(delay);
      // TODO: optimalization... multithreading?
      Algorithm();
    }
  }

  private void Update() {
    // DEBUG: in Prod use Coroutine for better performance
    Algorithm();
    // NOTE: Add shaders to materials for this to be visiable.
    //MeshCreator();
  }

  void Algorithm() {
    // 2.1. Distance check
    raycasting.DistanceCheck(distance);
    //2.2.Check if goal is visible
  RaycastHit hit;
    direction = (target.transform.position - transform.position).normalized;
    if (
      Physics.BoxCast(transform.position, transform.localScale / 2, direction, out hit, transform.rotation) &&
      hit.collider.tag.Equals(Tags.Target)
    ) {
      Departure(hit.point);
    } else {
      if (raycasting.minRaysList.Count >= 1 &&
          Functions.VectorLength(transform.position, raycasting.minRaysList[0].point) <= safetyRadius) {
        Debug.Log("Safety");
        localTarget = transform.TransformPoint(raycasting.minRaysList[0].point);
        MoveAway(raycasting.minRaysList[0].point);
      }  else
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
      Correction(transform.position, minRays[0].point, minRays[1].point);
      Debug.Log("3.1");
    } else
    // 3.3. Robot is located on a medial axis vertex - maintains max distance from 3 obstacles
    if (Functions.VectorLength(minRays[0].point, minRays[1].point) <= toleranceValue &&
        Functions.VectorLength(minRays[0].point, minRays[2].point) <= toleranceValue) {
      Debug.Log("3.3");
      if (T.Contains(transform.position)) {
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
    distance = defaultDistance;
  }

  private void MoveAway(Vector3 obstacle) {
    transform.position = Vector3.MoveTowards(transform.position, new Vector3(obstacle.x, 1, obstacle.z), -speed * Time.deltaTime);
    distance = defaultDistance;
  }



  private void Correction(Vector3 currentPosition, Vector3 p1, Vector3 p2) {
    // TODO/DEBUG: Jakimś sposobem correction uznaje czasem, że aby najlepiej uniknąć przeszkody będzie w nią przywalić...
    var pm1 = p1 - currentPosition;
    var pm2 = p2 - currentPosition;
    var corectionStepLength = (pm2.magnitude - pm1.magnitude) / 2;
    localTarget = new Vector3(pm1.x, 1, pm1.y).normalized * corectionStepLength;
    MoveAway(new Vector3(pm1.x, 1, pm1.y).normalized * corectionStepLength);
    //T.Add(currentPosition, J(x));
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
    //Debug.LogWarning(pm1 + " " + pm2 + " " + v1 + " " + v2);
    drawingList.Clear();
    drawingList.Add(v1.normalized * pm1.magnitude);
    drawingList.Add(v2.normalized * pm1.magnitude);
    Vector3 node = Functions.CalculateBestNode(transform.position, target.transform.position, new Vector3[] { v1, v2 });
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
    Vector3 node = Functions.CalculateBestNode(transform.position, target.transform.position, new Vector3[] { v1, v2, v3 });
    return node.normalized * pm1.magnitude;
  }

  private void MeshCreator() {
    int vertexCount = raycasting.raysList.Count + 1;
    Vector3[] vertices = new Vector3[vertexCount];
    int[] triangles = new int[(vertexCount - 1) * 3];
    vertices[0] = Vector3.zero;
    for (int i = 0; i < vertexCount - 1; i++) {
      vertices[i + 1] = transform.InverseTransformPoint(raycasting.raysList[i].point + Vector3.forward * 0.2f);
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
    Gizmos.color = Color.white;
    Gizmos.DrawWireSphere(transform.position, safetyRadius);
    Gizmos.DrawWireSphere(transform.position, distance);
    if (raycasting.minRaysList != null && raycasting.minRaysList.Count > 0) {
      Gizmos.DrawWireSphere(transform.position, raycasting.minRaysList[0].distance);

      Gizmos.color = Color.gray;
      if (raycasting.minRaysList.Count > 0)
        Gizmos.DrawLine(transform.position, raycasting.minRaysList[0].point);
      if (raycasting.minRaysList.Count > 1)
        Gizmos.DrawLine(transform.position, raycasting.minRaysList[1].point);
      if (raycasting.minRaysList.Count > 2)
        Gizmos.DrawLine(transform.position, raycasting.minRaysList[2].point);
    }


    Gizmos.color = Color.red;
    foreach (var d in drawingList) {
      Gizmos.DrawSphere(transform.TransformPoint(d), 1.5f);
    }

    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(transform.TransformPoint(localTarget), 1f);
    Gizmos.color = Color.green;
    if (trajectory.Count > 1) {
      for (var i = 1; i < trajectory.Count - 1; i++) {
        Gizmos.DrawLine(trajectory[i - 1], trajectory[i]);
      }
    }
  }
}
