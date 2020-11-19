using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Raycasting {
  private const float step = 10;
  private float maxRange = 30;
  private readonly float edgeDistanceThreshold = 3;
  private readonly float edgeResolveIterations = 3;
  private readonly Transform transform;
  public List<RaycastInfo> raysList = new List<RaycastInfo>();
  public List<Vector3> raysMeshList = new List<Vector3>();
  public List<RaycastInfo> minRaysList = new List<RaycastInfo>();

  public Raycasting(Transform _transform) {
    transform = _transform;
  }

  public void DistanceCheck(float _maxRange) {
    raysList.Clear();
    raysMeshList.Clear();
    maxRange = _maxRange;
    RaycastInfo oldViewCast = new RaycastInfo();
    for (float angle = 0; angle < 360; angle += step) {
      Vector3 direction = Functions.CalculateDirection(transform, angle, transform.forward.normalized, maxRange);
      RaycastInfo currentInfo = CastRay(transform.position, direction, maxRange, angle);

      if (angle > 0) {
        bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.distance - currentInfo.distance) > edgeDistanceThreshold;
        if (oldViewCast.hit != currentInfo.hit || (oldViewCast.hit && currentInfo.hit && edgeDstThresholdExceeded)) {
          EdgeInfo edge = FindEdge(oldViewCast, currentInfo);
          if (edge.pointOn != Vector3.zero) {
            raysMeshList.Add(edge.pointOn);
          }
          if (edge.pointOut != Vector3.zero) {
            raysMeshList.Add(edge.pointOut);
          }
        }
      }
      raysMeshList.Add(currentInfo.point);
      oldViewCast = currentInfo;

      if (currentInfo.hit) {
        raysList.Add(currentInfo);
      }
    }
    ManageRaycastList();
  }

  private void ManageRaycastList() {
    List<RaycastInfo> newList = new List<RaycastInfo>();
    for (int i = 0; i < raysList.Count - 1; i++) {
      var current = raysList[i];
      var next = i != raysList.Count - 1 ? raysList[i + 1] : raysList[0];
      var last = i != 0 ? raysList[i - 1] : raysList[raysList.Count - 1];

      if (last.distance > current.distance && next.distance > current.distance) {
        newList.Add(current);
      }
    }
    minRaysList = newList.OrderBy(ray => ray.distance).ToList();
  }

  private RaycastInfo CastRay(Vector3 origin, Vector3 direction, float range, float angle) {
    RaycastHit hit;
    if (Physics.Raycast(origin, direction, out hit, range)) {
      return new RaycastInfo(true, hit.point, hit.distance, angle);
    } else {
      return new RaycastInfo(false, origin + direction, range, angle);
    }
  }


  EdgeInfo FindEdge(RaycastInfo minViewCast, RaycastInfo maxViewCast) {
    float minAngle = minViewCast.angle;
    float maxAngle = maxViewCast.angle;
    Vector3 minPoint = Vector3.zero;
    Vector3 maxPoint = Vector3.zero;

    for (int i = 0; i < edgeResolveIterations; i++) {
      float angle = (minAngle + maxAngle) / 2;
      Vector3 direction = Functions.CalculateDirection(transform, angle, transform.forward.normalized, maxRange);
      RaycastInfo newViewCast = CastRay(transform.position, direction, maxRange, angle);

      bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
      if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
        minAngle = angle;
        minPoint = newViewCast.point;
      } else {
        maxAngle = angle;
        maxPoint = newViewCast.point;
      }
    }

    return new EdgeInfo(minPoint, maxPoint);
  }
}
