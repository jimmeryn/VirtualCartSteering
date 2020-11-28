using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class AStar
{
  public static Vector3 start { get; private set; }

  public static Vector3 target { get; private set; }

  public static List<Node> openList { get; private set; }

  public static List<Node> closedList { get; private set; }

  public static void Init(Vector3 _start, Vector3 _target) {
    openList = new List<Node>();
    closedList = new List<Node>();
    start = _start;
    target = _target;
    var startNode = new Node(start, 0, 0);
    openList.Add(startNode);
  }

  public static void AddToOpenList(Vector3 currentPosition, Vector3 nodeLocation) {
    Node currentPositionNode = new Node();
    if (Functions.ListContainsNodeLocation(openList, currentPosition)) {
      currentPositionNode = Functions.GetNodeFromListByLocation(openList, currentPosition);
    } else {
      currentPositionNode = Functions.GetNodeFromListByLocation(closedList, currentPosition);
    }

    var g = currentPositionNode.g + Functions.VectorLength(currentPosition, nodeLocation);
    var h = Functions.VectorLength(nodeLocation, target);
    var node = new Node(nodeLocation, g, h);

    bool openListContainsBetterNode = false;
    foreach (var nodeInOpenList in openList) {
      if (nodeInOpenList.location == nodeLocation && nodeInOpenList.f < node.f) {
        openListContainsBetterNode = true;
      }
    }

    if (!openListContainsBetterNode && !Functions.ListContainsNodeLocation(closedList, nodeLocation)) {
      openList.Add(node);
    }
  }

  public static Vector3 CalculateBestNode(Vector3 currentPosition, Vector3[] nodes) {
    Node currentPositionNode = new Node();
    if(Functions.ListContainsNodeLocation(openList, currentPosition)) {
      currentPositionNode = Functions.GetNodeFromListByLocation(openList, currentPosition);
    } else {
      currentPositionNode = Functions.GetNodeFromListByLocation(closedList, currentPosition);
    }

    foreach (var nodeLocation in nodes) {
      if (Functions.ListContainsNodeLocation(closedList, nodeLocation) || Functions.ListContainsNodeLocation(openList, nodeLocation)) {
        continue;
      }

      var g = currentPositionNode.g + Functions.VectorLength(currentPosition, nodeLocation);
      var h = Functions.VectorLength(nodeLocation, target);
      var node = new Node(nodeLocation, g, h);

      openList.Add(node);
    }

    return GetBestNode();
  }

    public static Vector3 GetBestNode() {
    var currentNode = openList.OrderBy(n => n.f).First();
    openList.Remove(currentNode);
    closedList.Add(currentNode);
    return currentNode.location;
  }
}
