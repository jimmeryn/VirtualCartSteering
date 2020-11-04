using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Functions
{
  public static Vector3 DirectionFromAngle(float angle, Vector3 forward) {
    return Quaternion.Euler(0, angle, 0) * forward;
  }

  public static Vector3 CalculateDirection(Transform transform, float angle, Vector3 normalizedForward, float range) {
    return transform.TransformDirection(DirectionFromAngle(angle, normalizedForward) * range);
  }

  public static float CalculateLength(Vector3 playerPosition, Vector3 targetPosition) {
    return (targetPosition - playerPosition).magnitude;
  }

  public static RaycastInfo ChooseCloserPoint(Vector3 currentPosition, RaycastInfo a, RaycastInfo b) {
    return CalculateLength(currentPosition, a.point) < CalculateLength(currentPosition, b.point) ? a : b;
  }

  public static float ChooseShortest(List<RaycastInfo> nodes) {
    return nodes.Min(node => node.distance);
  }

  // NOT WORKING: but I left it becouse I might fix and use it later
  public static Vector3 CalculateCorrectPosition(RaycastInfo p1, RaycastInfo p2, Vector3 current) {
    var a = -((p2.point.z - p1.point.z)/(p2.point.z - p1.point.z));
    var b = current.z - a*current.x;
    var r = (p2.point - p1.point).magnitude;
    var delta = Mathf.Pow(2 * a * b - 2 * a * current.z - 2 * current.x, 2) - 4 * (1 + Mathf.Pow(a, 2)) * (Mathf.Pow(current.x, 2) + Mathf.Pow(b, 2) + Mathf.Pow(current.z, 2) - 2 * b * current.z - Mathf.Pow(r, 2));
    var x1 = (-(2 * a * b - 2 * a * current.z - 2 * current.x) + Mathf.Sqrt(delta)) / (2 * (1 + Mathf.Pow(a, 2)));
    var x2 = (-(2 * a * b - 2 * a * current.z - 2 * current.x) - Mathf.Sqrt(delta)) / (2 * (1 + Mathf.Pow(a, 2)));
    var z1 = a * x1 + b;
    var z2 = a * x2 + b;
    return new Vector3(x1, current.y, z1);
  }
}
