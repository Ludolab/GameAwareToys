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
        [HideInInspector]
        protected MetaDataFrameType frameType;
        public MetaDataFrameType FrameType { get { return frameType; } set { frameType = value; } }

        [SerializeField]
        [HideInInspector]
        protected bool persistAcrossScenes;
        public bool PersistAcrossScenes { get { return persistAcrossScenes; } set { persistAcrossScenes = value; } }

        [SerializeField]
        [HideInInspector]
        protected ScreenSpaceReference screenRectStyle;
        public ScreenSpaceReference ScreenRectStyle { get { return screenRectStyle; } set { screenRectStyle = value; } }

        private Camera screenSpaceCamera = null;
        public Camera ScreenSpaceCamera {
            set {
                screenSpaceCamera = value;
            }
            get {
                return screenSpaceCamera == null ? MetaDataTracker.Instance.ScreenSpaceCamera : screenSpaceCamera;
            }
        }

        private Collider col;
        private Renderer ren;


        protected virtual void Awake() {
            objectKey = System.Guid.NewGuid().ToString();
        }

        protected virtual void Start() {
            MetaDataTracker.Instance.AddTrackableObject(this);
            col = GetComponent<Collider>();
            ren = GetComponent<Renderer>();
            if(ren != null) {
                screenRectStyle = ScreenSpaceReference.Renderer;
            }
            else if(col != null) {
                screenRectStyle = ScreenSpaceReference.Collider;
            }
            else {
                screenRectStyle = ScreenSpaceReference.Transform;
            }
        }

        protected virtual void OnDestroy() {
            MetaDataTracker.Instance.RemoveTrackableObject(this);
        }

        public virtual JObject InbetweenData() {
            JObject jObject = new JObject();
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                    jObject[IMetaDataTrackable.SCREEN_RECT_KEY] = ScreenSpaceHelper.WorldToViewerScreenPoint(ScreenSpaceCamera, transform.position).ToJObject();
                    break;
                case ScreenSpaceReference.Collider:
                    //TODO we might want to have a better system for referencing cameras here. Both for flexibility and performance.
                    jObject[IMetaDataTrackable.SCREEN_RECT_KEY] = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, col).ToJObject();
                    break;
                case ScreenSpaceReference.Renderer:
                    jObject[IMetaDataTrackable.SCREEN_RECT_KEY] = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, ren).ToJObject();
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
                    jObject[IMetaDataTrackable.SCREEN_RECT_KEY] = ScreenSpaceHelper.WorldToViewerScreenPoint(ScreenSpaceCamera, transform.position).ToJObject();
                    break;
                case ScreenSpaceReference.Collider:
                    //TODO we might want to have a better system for referencing cameras here. Both for flexibility and performance.
                    jObject[IMetaDataTrackable.SCREEN_RECT_KEY] = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, col).ToJObject();
                    break;
                case ScreenSpaceReference.Renderer:
                    jObject[IMetaDataTrackable.SCREEN_RECT_KEY] = ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, ren).ToJObject();
                    break;
                case ScreenSpaceReference.None:
                    break;
                default:
                    Debug.LogWarning("Unrecognized ScreenRect Style");
                    break;
            }
            return jObject;
        }

        public DepthRect ScreenRect() {
            switch (this.screenRectStyle) {
                case ScreenSpaceReference.Transform:
                    var pos = ScreenSpaceHelper.WorldToViewerScreenPoint(ScreenSpaceCamera, transform.position);
                    return new DepthRect((int)pos.x, (int)pos.y, 0, 0, pos.z);
                case ScreenSpaceReference.Collider:
                    //TODO we might want to have a better system for referencing cameras here. Both for flexibility and performance.
                    return ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, col);
                case ScreenSpaceReference.Renderer:
                    return  ScreenSpaceHelper.WorldBoundsToViewerScreenRect(ScreenSpaceCamera, ren);
                case ScreenSpaceReference.None:
                    return DepthRect.zero;
                default:
                    Debug.LogWarning("Unrecognized ScreenRect Style");
                    return DepthRect.zero;
            }
        }
    }

}