using GameAware;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace InvisibleMaze {
    public class MazeTile: IMetaDataTrackable {
        private static int COUNTER = 0;

        private string objectKey;
        public bool exposed;
        public int bridge;
        public TileType type;
        private Vector2Int pos;

        public MazeTile(TileType type, Vector2Int pos) {
            COUNTER++;
            this.objectKey = "tile" + COUNTER;
            this.type = type;
            this.exposed = false;
            this.bridge = -1;
            this.pos = pos;
            MetaDataTracker.Instance.AddTrackableObject(this);
        }

        public MetaDataFrameType FrameType => MetaDataFrameType.KeyFrame;

        public bool PersistAcrossScenes => false;

        public string ObjectKey => objectKey; 

        public JObject InbetweenData() {
            return new JObject();
        }

        public JObject KeyFrameData() {
            return new JObject {
                {IMetaDataTrackable.SCREEN_RECT_KEY, ScreenRect().ToJObject() },
                {"type", type.ToString() },
                {"exposed", exposed },
            };
        }

        public DepthRect ScreenRect() {
            Bounds b = MazeManager.Instance.GetTileBounds(this.pos);
            return ScreenSpaceHelper.WorldBoundsToViewerScreenRect(Camera.main, b);
        }
    }
}