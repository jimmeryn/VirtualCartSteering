using System.Collections.Generic;
using UnityEngine;

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

  public Node() {
    location = new Vector3(0, 0, 0);
    g = 0;
    h = 0;
  }

  public Node(Vector3 _location) {
    location = _location;
    g = Mathf.Infinity;
    h = Mathf.Infinity;
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

  public Node FindNodeInList(List<Node> list) {
    var node = new Node();
    foreach (var listNode in list) {
      if (listNode.location == location) {
        node = listNode;
      }
    }
    return node;
  }
}
