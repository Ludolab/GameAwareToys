using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;

namespace InvisibleMaze {

    [RequireComponent(typeof(BoxCollider2D))]
    public class GemBehavior : MetaDataTrackable {


        public GemColors color;
        private Collider2D colider;

        // Start is called before the first frame update
        override protected void Start() {
            objectKey = color.ToString() + " Gem";
            frameType = MetaDataFrameType.Inbetween;
            screenRectStyle = ScreenSpaceReference.Renderer;
            colider= GetComponent<BoxCollider2D>();
        }

        public override JObject KeyFrameData() {
            var job =  base.KeyFrameData();
            job["color"] = color.ToString();
            return job;
        }

        public void Held() {
            colider.enabled = false;
        }

        public void Placed() {
            colider.enabled = true;
        }
    }
}

