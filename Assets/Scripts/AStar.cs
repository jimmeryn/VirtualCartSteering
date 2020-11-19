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
    //var nextNode = new Node(currentPosition, Mathf.Infinity, Mathf.Infinity);
    //for (int i = 0; i < openList.Count - 1; i++) {
    //  if (openList[i].f <= nextNode.f) {
    //    nextNode = openList[i];
    //  }
    //}
    //openList.Remove(nextNode);
    //closedList.Add(nextNode);

    //foreach (var node in nodes) {
    //  if (closedList.Contains(new Node(node, 0, 0)))
    //    continue;

    //  var neighbourDistance = Functions.VectorLength(currentPosition, node);
    //  var g = closedList.Count > 0 ? closedList[closedList.Count - 1].g + neighbourDistance : neighbourDistance;
    //  var newNode = new Node(node, g, Functions.VectorLength(node, target));

    //  if (openList.Contains(newNode) && newNode.g > FindNodeByLocation(openList, newNode.location).g)
    //    continue;

    //  openList.Add(newNode);
    //}
    //return nextNode.location;
    var currentNode = openList.OrderBy(n => n.f).First();
    openList.Remove(currentNode);
    closedList.Add(currentNode);
    if (currentNode.location == target) {
      return currentNode.location;
    }
    foreach(var nodeLocation in nodes) {
      if (closedList.Contains(new Node(nodeLocation, 0, 0))) {
        continue;
      }
      var g = currentNode.g + Functions.VectorLength(currentNode.location, nodeLocation);
      var h = Functions.VectorLength(nodeLocation, target);
      var node = new Node(nodeLocation, g, h);
      if (openList.Contains(new Node(node.location, 0, 0))) {
        continue;
      }
      openList.Add(node);
    }
    return currentNode.location;
  }

  private static Node FindNodeByLocation(List<Node> list, Vector3 location) {
    var node = new Node(start, 0, 0);
    foreach(var listNode in list) {
      if (listNode.location == location) {
        node = listNode;
      }
    }
    return node;
  }



  public class Node {
    public Vector3 location { get; private set; }
    public float g { get; private set; }
    public float h { get; private set; }
    public float f { get { return g + h; } }

    public Node(Vector3 _location, float _g, float _h) {
      location = _location;
      g = _g;
      h = _h;
    }

    public static bool operator ==(Node n1, Node n2) {
      return n1.location == n2.location;
    }

    public static bool operator !=(Node n1, Node n2) {
      return n1.location != n2.location;
    }

    public override bool Equals(object obj) {
      return base.Equals(obj);
    }

    public override int GetHashCode() {
      return base.GetHashCode();
    }
  }
}
