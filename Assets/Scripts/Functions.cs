using System.Collections.Generic;
using UnityEngine;

public static class Functions
{
  public static Vector3 DirectionFromAngle(float angle, Vector3 forward) {
    return Quaternion.Euler(0, angle, 0) * forward;
  }

  public static Vector3 CalculateDirection(Transform transform, float angle, Vector3 normalizedForward, float range) {
    return transform.TransformDirection(DirectionFromAngle(angle, normalizedForward) * range);
  }

  public static float VectorLength(Vector3 v1, Vector3 v2) {
    return (v2 - v1).magnitude;
  }

  public static float CalculateError(float v1, float v2) {
    return Mathf.Abs(v1 - v2);
  }

  public static bool ListContainsNodeLocation(List<Node> list, Vector3 location) {
    foreach (var node in list) {
      if (node.location == location) {
        return true;
      }
    }
    return false;
  }

  public static Node GetNodeFromListByLocation(List<Node> list, Vector3 location) {
    foreach (var node in list) {
      if (node.location == location) {
        return node;
      }
    }
    return new Node();
  }
}
