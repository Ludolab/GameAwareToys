using GameAware;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace InvisibleMaze {

    [RequireComponent(typeof(BoxCollider2D))]
    public class PedestalBehavior : MetaDataTrackable {

        public GemBehavior targetGem = null;
        public GemBehavior slottedGem = null;
        private BoxCollider2D boxCollider; 
        
        public bool IsUnlocked {
            get {
                return slottedGem != null && slottedGem == targetGem;
            }
        }
        
        // Start is called before the first frame update
        override protected void Start() {
            boxCollider = GetComponent<BoxCollider2D>();
            frameType = MetaDataFrameType.KeyFrame;
            screenRectStyle = ScreenSpaceReference.Collider;
            
            base.Start();
        }

        protected override IEnumerator RegisterTrackable() {
            while(targetGem == null) {
                yield return new WaitForEndOfFrame();
            }
            objectKey = targetGem.color + " Pedestal";
            MetaDataTracker.Instance.AddTrackableObject(this);
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
                ret.PickUp();
                return slottedGem;
            }
            return null;
        }

        public override DepthRect ScreenRect() {
            return ScreenSpaceHelper.WorldBoundsToViewerScreenRect(Camera.main, boxCollider.bounds);
        }

        override public JObject KeyFrameData() {
            var job = base.KeyFrameData();
            job["targetGem"] = targetGem.ObjectKey;
            job["correct"] = IsUnlocked;
            return job;
        }

    }
}
