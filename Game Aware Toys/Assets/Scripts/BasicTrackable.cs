using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;

public class BasicTrackable : MetaDataTrackable {

    bool is_visible;
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        this.objectKey = name;
        this.screenRectStyle = ScreenSpaceReference.Collider;
        this.frameType = MetaDataFrameType.Inbetween;
    }

    // Update is called once per frame

    private void OnBecameVisible() {
        is_visible = true;
        Debug.LogFormat("{0} became visible", name);
    }

    private void OnBecameInvisible() {
        is_visible = false;
        Debug.LogFormat("{0} became INVISIBLE", name);
    }

    public override JObject KeyFrameData() {
        var job = base.KeyFrameData();
        job["is_visible"] = is_visible;
        return job;
    }
}
