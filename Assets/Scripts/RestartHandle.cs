using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartHandle : MonoBehaviour {
  public void RestartClick() {
    enabled = false;
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
}
