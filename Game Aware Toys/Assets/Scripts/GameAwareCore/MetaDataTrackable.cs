using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameAware {
    public abstract class MetaDataTrackable : MonoBehaviour, IMetaDataTrackable {

        protected string objectKey = string.Empty;
        public string ObjectKey { get { return objectKey; } }

        [SerializeField]
        protected MetaDataFrameType frameType;
        public MetaDataFrameType FrameType { get { return frameType; } set { frameType = value; } }

        [SerializeField]
        protected bool persistAcrossScenes;
        public bool PersistAcrossScenes { get { return persistAcrossScenes; } set { persistAcrossScenes = value; } }

        [SerializeField]
        protected ScreenSpaceReference screenRectStyle;
        public ScreenSpaceReference ScreenRectStyle { get { return ScreenRectStyle; } set { screenRectStyle = value; } }

        private Collider col;
        private Renderer ren;


        protected virtual void Awake() {
            objectKey = System.Guid.NewGuid().ToString();
        }

        protected virtual void Start() {
            MetaDataTracker.Instance.AddTrackableObject(this);
            col = GetComponent<Collider>();
            ren = GetComponent<Renderer>();
        }

        protected virtual void OnDestroy() {
            MetaDataTracker.Instance.RemoveTrackableObject(this);
        }

        public virtual JObject InbetweenData() {
            JObject jObject = new JObject();
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                    jObject["screenRect"] = JsonConvert.SerializeObject(ScreenSpaceHelper.ScreenPosition(Camera.main, transform.position));
                    break;
                case ScreenSpaceReference.Collider:
                    //TODO we might want to have a better system for referencing cameras here. Both for flexibility and performance.
                    jObject["screenRect"] = JsonConvert.SerializeObject(ScreenSpaceHelper.ScreenRect(Camera.main, col));
                    break;
                case ScreenSpaceReference.Renderer:
                    jObject["screenRect"] = JsonConvert.SerializeObject(ScreenSpaceHelper.ScreenRect(Camera.main, ren));
                    break;
                case ScreenSpaceReference.None:
                    break;
                default:
                    Debug.LogWarning("Unrecognized ScreenRect Style");
                    break;
            }
            return jObject;
        }

        public virtual JObject KeyFrameData() {
            JObject jObject = new JObject();
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                    jObject["screenRect"] = JsonConvert.SerializeObject(ScreenSpaceHelper.ScreenPosition(Camera.main, transform.position));
                    break;
                case ScreenSpaceReference.Collider:
                    //TODO we might want to have a better system for referencing cameras here. Both for flexibility and performance.
                    jObject["screenRect"] = JsonConvert.SerializeObject(ScreenSpaceHelper.ScreenRect(Camera.main, col));
                    break;
                case ScreenSpaceReference.Renderer:
                    jObject["screenRect"] = JsonConvert.SerializeObject(ScreenSpaceHelper.ScreenRect(Camera.main, ren));
                    break;
                case ScreenSpaceReference.None:
                    break;
                default:
                    Debug.LogWarning("Unrecognized ScreenRect Style");
                    break;
            }
            return jObject;
        }
    }

}