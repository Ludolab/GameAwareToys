using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;
using static TrackableWalker;
using System.Drawing;

public class InteractiveWalker : MetaDataTrackable {

    [System.Serializable]
    public class WalkerColors {
        public Sprite sprite;
        public TrackableWalker.WalkerColor color;
    }

    public WalkerColors[] colors;
    private int currentColor = 0;
    new private Rigidbody2D rigidbody;
    new private SpriteRenderer renderer;
    public float speed;

    // Start is called before the first frame update
    override protected void Start() {
        base.Start();
        this.objectKey = this.name;
        rigidbody = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        Vector2 vel = rigidbody.velocity;
        vel.x = Input.GetAxis("Horizontal");
        vel.y = Input.GetAxis("Vertical");
        rigidbody.velocity = vel.normalized * speed;

        if (Input.GetKeyDown(KeyCode.E)) {
            currentColor += 1;
            currentColor %= colors.Length;
            renderer.sprite = colors[currentColor].sprite;
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            currentColor -= 1;
            if (currentColor < 0) {
                currentColor = colors.Length - 1;
            }
            renderer.sprite = colors[currentColor].sprite;
        }

        if (Input.GetKey(KeyCode.F)) {
            speed += .1f;
        }
        if (Input.GetKey(KeyCode.R)) {
            speed -= .1f;
        }
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Alpha0)) {
            transform.position = Vector2.zero;
            speed = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            var box = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(renderer.bounds).rect;
            box.x = 0;
            box.y = 0;
            transform.position = ScreenSpaceHelper.ViewerScreenPointToWorldPoint(box.center);
            //transform.position = ScreenSpaceHelper.ViewerScreenPointToWorldPoint(Vector2Int.zero);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            var box = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(renderer.bounds).rect;
            box.x = Screen.width - box.width;
            box.y = 0;
            transform.position = ScreenSpaceHelper.ViewerScreenPointToWorldPoint(box.center);
            //transform.position = ScreenSpaceHelper.ViewerScreenPointToWorldPoint(new Vector2Int(Screen.width,0));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            var box = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(renderer.bounds).rect;
            box.x = 0;
            box.y = Screen.height - box.height;
            transform.position = ScreenSpaceHelper.ViewerScreenPointToWorldPoint(box.center);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            var box = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(renderer.bounds).rect;
            box.x = Screen.width - box.width;
            box.y = Screen.height - box.height;
            transform.position = ScreenSpaceHelper.ViewerScreenPointToWorldPoint(box.center);
        }
    }

    public override JObject KeyFrameData() {
        JObject job = base.KeyFrameData();
        job["color"] = colors[currentColor].color.ToString();
        job["speed"] = speed;
        return job;
    }
}
