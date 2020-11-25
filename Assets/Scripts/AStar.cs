using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class AStar
{
  public static Vector3 start { get; private set; }

  public static Vector3 target { get; private set; }

  public static List<Node> openList { get; private set; } = new List<Node>();

  public static List<Node> closedList { get; private set; } = new List<Node>();

  public static void Init(Vector3 _start, Vector3 _target) {
    start = _start;
    target = _target;
    var startNode = new Node(start, 0, 0);
    openList.Add(startNode);
  }

  public static Vector3 CalculateBestNode(Vector3 currentPosition, Vector3[] nodes) {
    Node currentPositionNode = new Node();
    if(Functions.ListContainsNodeLocation(openList, currentPosition)) {
      currentPositionNode = Functions.GetNodeFromListByLocation(openList, currentPosition);
    } else {
      currentPositionNode = Functions.GetNodeFromListByLocation(closedList, currentPosition);
    }

    foreach (var nodeLocation in nodes) {
      if (closedList.Contains(new Node(nodeLocation, 0, 0))) {
        continue;
      }
      
      var g = currentPositionNode.g + Functions.VectorLength(currentPosition, nodeLocation);
      var h = Functions.VectorLength(nodeLocation, target);
      var node = new Node(nodeLocation, g, h);

      if (Functions.ListContainsNodeLocation(openList, node.location)) {
        continue;
      }

      openList.Add(node);
    }
    var currentNode = openList.OrderBy(n => n.f).First();
    openList.Remove(currentNode);
    closedList.Add(currentNode);
    if (currentNode.location == target) {
      return currentNode.location;
    }
    return currentNode.location;
  }
}
