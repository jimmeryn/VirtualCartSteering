using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour {
  private LineRenderer lr;
  private List<Vector3> points;
 
  private void Start() {
    lr = GetComponent<LineRenderer>();
    StartCoroutine("DrawLineEnumerator", 0.5f);
  }

  IEnumerator DrawLineEnumerator(float delay) {
    while (true) {
      yield return new WaitForSeconds(delay);
      DrawLine();
    }
  }

  public void SetUpLine(List<Vector3> points) {
    lr.positionCount = points.Count - 1;
    this.points = points;
  }

  private void DrawLine() {
    List<Vector3> pointsList = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Steering>().trajectory;
    if (pointsList != null && pointsList.Count > 1) {
    SetUpLine(pointsList);
      for (int i = 0; i < points.Count - 1; i++) {
        lr.SetPosition(i, points[i]);
      }
    }
  }
}
