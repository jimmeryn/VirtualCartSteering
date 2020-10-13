using System.Collections.Generic;
using UnityEngine;

public class Graph {
  private List<RaycastInfo> rays;
  public List<Vector3> nodes;
  public Vector3 currentPosition;
  public Vector3 targetPosition;

  
  public Graph(List<RaycastInfo> _rays, Vector3 _currentPosition, Vector3 _targetPosition) {
    rays = _rays;
    targetPosition = _targetPosition;
    currentPosition = _currentPosition;
    CalculateNodes();
  }

  /**
   * Base on rays list calculate Voronoi Graph 
   */
  private void CalculateNodes() {
    nodes = new List<Vector3>();
  }

  /**
   * Base on Voronoi graph calculate best posiiton
   */
  public Vector3 BestPosition() {
    Vector3 bestNode = new Vector3();
    foreach(Vector3 node in nodes) {
      AStar(node, targetPosition);
    }
    return bestNode;
  }

  private float AStar(Vector3 node, Vector3 target) {
    return 0;
  }
}
