using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Raycasting {
  private const float step = 1;
  private float maxRange = 30;
  private Transform transform;
  public List<RaycastInfo> raysList = new List<RaycastInfo>();
  public List<RaycastInfo> minRaysList = new List<RaycastInfo>();

  public Raycasting(Transform _transform) {
    transform = _transform;
  }

  public void DistanceCheck(float _maxRange) {
    raysList.Clear();
    maxRange = _maxRange;
    for (float angle = 0; angle < 360; angle += step) {
      Vector3 direction = Functions.CalculateDirection(transform, angle, transform.forward.normalized, maxRange);
      RaycastInfo currentInfo = CastRay(transform.position, direction, maxRange);
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
        newList.Add(new RaycastInfo(current.hit, current.point, current.distance));
      }
    }
    minRaysList = newList.OrderBy(ray => ray.distance).ToList();
  }

  private RaycastInfo CastRay(Vector3 origin, Vector3 direction, float range) {
    RaycastHit hit;
    if (Physics.Raycast(origin, direction, out hit, range)) {
      return new RaycastInfo(true, hit.point, hit.distance);
    } else {
      return new RaycastInfo(false, origin + direction, range);
    }
  }
}
