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
}
