using GameAware;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToyManager : MonoBehaviour {

    public static ToyManager Instance { get; private set; } = null;

    public bool forceRunInBackground = true;

    private bool menuOpen = false;
    private Rect windowRect = new Rect(20, 20, 300, 500);
    private Vector2 scrollPos = new Vector2(0, 0);
    private ToyControls currentToy;

    public string[] scenes;

    // Start is called before the first frame update
    void Start() {
        if (forceRunInBackground) {
            Application.runInBackground = true;
        }
        if(Instance != null) {
            Debug.LogWarning("Multiple ToyManagers Destroying new one.");
            Destroy(this);
            return;
        }
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        SceneManager.activeSceneChanged += OnSceneChange;
        FindToyControls();

    }

    private void FindToyControls() {
        var toy = GameObject.Find("Toy");
        if (toy != null) {
            currentToy = toy.GetComponent<ToyControls>();
        }
        else {
            currentToy = null;
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuOpen = !menuOpen;
        }
    }

    private void OnGUI() {
        if (menuOpen) {
            windowRect = GUI.Window(0, windowRect, WindowFunction, "Toy Box");
        }
    }

    void WindowFunction(int windowID) {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical();
        GUILayout.Label(string.Format("Current Scene: {0}", SceneManager.GetActiveScene().name));
        if (currentToy != null) {
            currentToy.ToyGUIPanel();
        }
        else {
            GUILayout.Label("No controls for this toy");
        }
        GUILayout.Space(30);
        GUILayout.Label("Change Scene:");
        foreach (string sceneName in scenes) {
            if (GUILayout.Button(sceneName)) {
                SceneManager.LoadScene(sceneName);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void OnSceneChange(Scene current, Scene next) {
        FindToyControls();
    }
}
