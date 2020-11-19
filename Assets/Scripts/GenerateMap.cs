using UnityEngine;

public class GenerateMap : MonoBehaviour {
  public GameObject groundPlane;
  private int width = 1;
  private int height = 1;

  [Range(0, 1)]
  public float openSpaceValue = 0.7f;

  [Range(0.4f, 1.5f)]
  public float obstacleSizeMultiplayer = 1;

  public GameObject obstacle;

  public GameObject player;
  Vector3 playerPosition = new Vector3(-40f, 1f, -40f);

  public GameObject target;
  Vector3 targetPosition = new Vector3(40f, 0.1f, 40f);

  [HideInInspector]
  public bool created = false;

  void Start() {
    width = (int)groundPlane.transform.localScale.x * 10;
    height = (int)groundPlane.transform.localScale.z * 10;
    GenerateLevel();
  }

  void GenerateLevel() {
    Instantiate(player, playerPosition, Quaternion.identity);
    Instantiate(target, targetPosition, Quaternion.identity);

    Vector3 pos;
    for (int x = 10; x < width; x += 10) {
      for (int y = 10; y < height; y += 10) {
        if (Random.value > openSpaceValue) {
          pos = new Vector3(x - width / 2f, 2f, y - height / 2f);
          if (pos.x != targetPosition.x && pos.y != targetPosition.y && pos.x != playerPosition.x && pos.y != playerPosition.y) {
            Instantiate(obstacle, pos, Quaternion.identity, transform);
          }
        }
      }
    }
    if (obstacleSizeMultiplayer != 1) {
      var Obstacles = GameObject.FindGameObjectsWithTag(Tags.Obstacle);
      foreach (var obstacle in Obstacles) {
        obstacle.transform.localScale *= obstacleSizeMultiplayer;
      }
    }
  }
}
