using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleAvoidance {
  [Range(1, 100)]
  private const float step = 5;
  private Transform transform;
  private GameObject target;
  private float maxRange = 30;
  public List<RaycastInfo> raycastList = new List<RaycastInfo>();
  public List<Vector3> trajectory = new List<Vector3>();
  public List<Vector3> directionList = new List<Vector3>();
  float circleR;
  const float toleranceValue = 3f;
  Vector3[] vArray = new Vector3[2];

  public ObstacleAvoidance(Transform _transform, GameObject _target) {
    target = _target;
    transform = _transform;
  }

  public Vector3 FindLocalTarget() {
    maxRange = (target.transform.position - transform.position).magnitude;
    Raycasting();
    ManageRaycastList();
    if (raycastList.Count > 0) {
      circleR = raycastList[0].distance;
    }
    return MedialAxis();
  }

  public void Raycasting() {
    raycastList.Clear();
    for (float angle = 0; angle < 360; angle += step) {
      Vector3 direction = Functions.CalculateDirection(transform, angle, transform.forward.normalized, maxRange);
      RaycastInfo currentInfo = CastRay(transform.position, direction, angle, maxRange);
      if (currentInfo.hit) {
        raycastList.Add(currentInfo);
      }
    }
  }

  private void ManageRaycastList() {
    List<RaycastInfo> newList = new List<RaycastInfo>();
    for (int i = 0; i < raycastList.Count - 1; i++) {
      var current = raycastList[i];
      var next = i != raycastList.Count - 1 ? raycastList[i + 1] : raycastList[0];
      var last = i != 0 ? raycastList[i - 1] : raycastList[raycastList.Count - 1];

      if (last.distance > current.distance && next.distance > current.distance) {
        newList.Add(new RaycastInfo(current.hit, current.point, current.distance, current.angle));
      }
    }
    raycastList = newList.OrderBy(ray => ray.distance).ToList();
  }

  private Vector3 MedialAxis() {
    trajectory.Add(transform.position);
    // Robot is located very close to an obstacle
    if (raycastList.Count >= 2 && Mathf.Abs(raycastList[0].distance - raycastList[1].distance) > toleranceValue) {
      var node = OutMA(transform.position, raycastList[0].point, raycastList[1].point);
      directionList.Add(transform.TransformPoint(node));
      return node;
    }
    // Robot is located on a medial axis vertex - maintains max distance from 3 obstacles
    else if (
      raycastList.Count >= 3 && (raycastList[0].point - raycastList[1].point).magnitude <= toleranceValue &&
     (raycastList[0].point - raycastList[2].point).magnitude <= toleranceValue
    ) {
      var node = OnMA(transform.position, raycastList[0].point, raycastList[1].point, raycastList[2].point);
      directionList.Add(transform.TransformPoint(node));
      return node;
    }
    // Robot is located on medial axis - maintains max distance from 2 obstacles
    else if (raycastList.Count >= 2 && (raycastList[0].point - raycastList[1].point).magnitude <= toleranceValue) {
      var node = OnMA(transform.position, raycastList[0].point, raycastList[1].point);
      directionList.Add(transform.TransformPoint(node));
      return node;
    } else {
      return transform.position;
    }
  }

  private Vector3 OutMA(Vector3 position, Vector3 p1, Vector3 p2) {
    var pm1 = p1 - position;
    var pm2 = p2 - position;
    var pm1neg = -pm1;
    var corectionStepLength = (pm2.magnitude - pm1.magnitude) / 2;
    return pm1neg.normalized * corectionStepLength;
  }

  private Vector3 OnMA(Vector3 position, Vector3 p1, Vector3 p2) {
    Vector3 pm1 = p1 - position;
    Vector3 pm2 = p2 - position;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = -v1;
    var corectionStepLength = (pm1.magnitude + pm2.magnitude);
    vArray = new Vector3[] { v1, v2 };
    Vector3 node = CalculateHeuristicallyBestNode(transform.position, target.transform.position, new Vector3[] { v1, v2 });
    Debug.Log("Edge" + node + "" + corectionStepLength);
    return node.normalized * corectionStepLength;
  }

  private Vector3 OnMA(Vector3 position, Vector3 p1, Vector3 p2, Vector3 p3) {
    Vector3 pm1 = p1 - position;
    Vector3 pm2 = p2 - position;
    Vector3 pm3 = p3 - position;
    Vector3 v1 = pm1 + pm2;
    Vector3 v2 = pm2 + pm3;
    Vector3 v3 = pm3 + pm1;
    var corectionStepLength = (pm1.magnitude + pm2.magnitude);
    Vector3 node = CalculateHeuristicallyBestNode(transform.position, target.transform.position, new Vector3[] { v1, v2, v3 });
    Debug.Log("Vertex" + node + "" + corectionStepLength);
    return node.normalized * corectionStepLength;
  }

  private RaycastInfo CastRay(Vector3 origin, Vector3 direction, float angle, float range) {
    RaycastHit hit;
    if (Physics.Raycast(origin, direction, out hit, range)) {
      return new RaycastInfo(true, hit.point, hit.distance, angle);
    } else {
      return new RaycastInfo(false, origin + direction, range, angle);
    }
  }

  private Vector3 CalculateHeuristicallyBestNode(Vector3 currentPosition, Vector3 targetPosition, Vector3[] nodes) {
    foreach(var node in nodes) {
      if (trajectory.Contains(node)) {
        continue;
      }
    }
    if (trajectory.Intersect(nodes).Count() == trajectory.Count()) {
      float[] fCostArray = new float[nodes.Length - 1];
      for (int i = 0; i < nodes.Length - 1; i++) {
        fCostArray[i] = Functions.CalculateLength(currentPosition, nodes[i]) + Functions.CalculateLength(nodes[i], targetPosition);
      }
      return nodes[Array.IndexOf(fCostArray, fCostArray.Min())];
    } else {
      foreach (var node in nodes) {
        if (!trajectory.Contains(node)) {
          return node;
        }
      }
      return currentPosition;
    }
  }

  public void OnDrawGizmos() {
    Gizmos.color = Color.white;
    Gizmos.DrawWireSphere(transform.position, circleR);
    Gizmos.color = Color.red;
    foreach (var ray in raycastList) {
      Gizmos.DrawLine(transform.position, ray.point);
    }
    Gizmos.color = Color.black;
    foreach (var v in vArray) {
      Gizmos.DrawSphere(transform.TransformPoint(v), .5f);
    }
    Gizmos.color = Color.black;
    foreach (var t in trajectory) {
      Gizmos.DrawSphere(t, .5f);
    }
  }
}
