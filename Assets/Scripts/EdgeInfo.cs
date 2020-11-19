using UnityEngine;

public struct EdgeInfo {
  public Vector3 pointOn;
  public Vector3 pointOut;

  public EdgeInfo(Vector3 _pointOn, Vector3 _pointOut) {
    pointOn = _pointOn;
    pointOut = _pointOut;
  }
}