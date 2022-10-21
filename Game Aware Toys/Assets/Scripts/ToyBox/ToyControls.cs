using UnityEngine;

public abstract class ToyControls : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public virtual void ToyGUIPanel() {
        GUILayout.Label("No controls for this toy");
    }
}
