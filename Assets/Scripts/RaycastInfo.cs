using UnityEngine;

public struct RaycastInfo
{
  public bool hit;
  public Vector3 point;
  public float distance;


  public RaycastInfo(bool _hit, Vector3 _point, float _distance) {
    hit = _hit;
    point = _point;
    distance = _distance;
  }
}
