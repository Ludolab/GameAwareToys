using UnityEngine;

public class WalkingCamera : MonoBehaviour {

    public float maxDistance;
    public float numRotations;
    public float cylceTime;
    private float lastSwitch;
    private bool goingOut = true;


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
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
}
