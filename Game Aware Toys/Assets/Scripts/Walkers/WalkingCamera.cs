using System.ComponentModel.Composition;
using UnityEngine;
using UnityEngine.UIElements;

public class WalkingCamera : MonoBehaviour {

    public enum CameraMovement {
        Pan,
        Rotate,
        Zoom
    }

    [Header("Cycle Settings")]
    public float cylceTime;
    public CameraMovement movement;

    [Header("Pan Settings")]
    public float maxDistance;
    public float numRotations;

    [Header("Rotate Settings")]
    public float maxAngle;

    [Header("Zoom Settings")]
    public float minZoom;
    public float maxZoom;

    private int rotationAxis = 0;
    private float lastSwitch;
    private bool goingOut = true;




    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        switch (movement) {
            case CameraMovement.Pan:
                transform.rotation = Quaternion.identity;
                Camera.main.orthographicSize = 5;
                PanUpdate();
                break;
            case CameraMovement.Rotate:
                transform.position = new Vector3(0, 0, -10);
                Camera.main.orthographicSize = 5;
                RotateUpdate();
                break;
            case CameraMovement.Zoom:
                transform.position = new Vector3(0, 0, -10);
                transform.rotation = Quaternion.identity;
                ZoomUpdate();
                break;
        }
    }

    void PanUpdate() {
        float t = (Time.time - lastSwitch) / cylceTime;
        float angle;
        float distance;
        if (goingOut) {
            angle = Mathf.Lerp(0, 360 * numRotations, t);
            distance = Mathf.Lerp(0, maxDistance, t);
        }
        else {

            angle = Mathf.Lerp(360 * numRotations, 0, t);
            distance = Mathf.Lerp(maxDistance, 0, t);
        }

        Vector3 pos = Quaternion.Euler(0, 0, angle) * Vector3.right * distance;
        pos.z = transform.position.z;
        transform.position = pos;

        if (Time.time - lastSwitch > cylceTime) {
            goingOut = !goingOut;
            lastSwitch = Time.time;
        }
    }

    void RotateUpdate() {

        float t = (Time.time - lastSwitch) / cylceTime;
        float angle = maxAngle * Mathf.Sin(t * 2 * Mathf.PI);

        Quaternion rot = rotationAxis switch {
            0 => Quaternion.Euler(angle, 0, 0),
            1 => Quaternion.Euler(0, angle, 0),
            2 => Quaternion.Euler(0, 0, angle),
            _ => Quaternion.Euler(angle, 0, 0)
        };

        transform.position = rot * Vector3.back * 10;
        transform.LookAt(Vector3.zero);
        if (rotationAxis == 2) {
            transform.rotation = rot;
        }

        if (Time.time - lastSwitch > cylceTime) {
            transform.rotation = Quaternion.identity;
            rotationAxis++;
            rotationAxis %= 3;
            lastSwitch = Time.time;
        }
    }

    void ZoomUpdate() {
        float t = (Time.time - lastSwitch) / cylceTime;
        t = (Mathf.Sin(t * 2 * Mathf.PI) + 1) / 2;
        if (goingOut) {
            Camera.main.orthographicSize = Mathf.Lerp(minZoom, maxZoom, t);
        }
        else {
            Camera.main.orthographicSize = Mathf.Lerp(maxZoom, minZoom, t);
        }
        
        if (Time.time - lastSwitch > cylceTime) {
            goingOut = !goingOut;
            lastSwitch = Time.time;
        }
    }
}
