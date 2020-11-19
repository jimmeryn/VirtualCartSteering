using UnityEngine;

public class ChangeCameraHandle : MonoBehaviour
{
  private Camera cameraVehicle;
  private Camera cameraPlane;
  private bool vehicleCamera = true;

  public void CameraSwitch() {
    cameraVehicle = GameObject.FindGameObjectWithTag(Tags.Player).GetComponentInChildren<Camera>();
    cameraPlane = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    if (vehicleCamera) {
      cameraVehicle.enabled = false;
      cameraPlane.enabled = true;
      vehicleCamera = false;
    } else {
      cameraPlane.enabled = false;
      cameraVehicle.enabled = true;
      vehicleCamera = true;
    }
  }
}
