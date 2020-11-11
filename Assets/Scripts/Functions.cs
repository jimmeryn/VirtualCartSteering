using System;
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

  public static float VectorLength(Vector3 v1, Vector3 v2) {
    return (v2 - v1).magnitude;
  }


  public static Vector3 CalculateBestNode(Vector3 currentPosition, Vector3 targetPosition, Vector3[] nodes) {
      float[] fCostArray = new float[nodes.Length - 1];
      for (int i = 0; i < nodes.Length - 1; i++) {
      if (currentPosition != nodes[i]) {
        fCostArray[i] = VectorLength(currentPosition, nodes[i]) + VectorLength(nodes[i], targetPosition);
      } else {
        fCostArray[i] = Mathf.Infinity;
      }
    }
      return nodes[Array.IndexOf(fCostArray, fCostArray.Min())];
  }
}
