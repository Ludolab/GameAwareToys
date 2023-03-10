using GameAware;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InvisibleMaze {
    public class PedestalBehavior : MetaDataTrackable {

        public GemBehavior targetGem;
        public GemBehavior slottedGem = null;
        
        public bool IsUnlocked {
            get {
                return slottedGem != null && slottedGem == targetGem;
            }
        }
        
        // Start is called before the first frame update
        override protected void Start() {
            base.Start();
            frameType = MetaDataFrameType.KeyFrame;
            screenRectStyle = ScreenSpaceReference.Renderer;
            objectKey = targetGem.color.ToString() + " Pedestal";
        }

        public void SlotGem(GemBehavior gem) {
            slottedGem = gem;
            gem.Placed();
            gem.transform.position = transform.position;
            gem.transform.parent = transform;
        }

        public GemBehavior PickUpGem() {
            if(slottedGem != null) {
                var ret = slottedGem;
                slottedGem = null;
                ret.Held();
                return slottedGem;
            }
            return null;
        }

        override public JObject KeyFrameData() {
            var job = base.KeyFrameData();
            job["needs"] = targetGem.color.ToString();
            return job;
        }

    }
}
