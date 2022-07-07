using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAware {
    public enum MetaDataFrameType {
        KeyFrame,
        Inbetween
    }

    public enum ScreenSpaceReference {
        None,
        Transform,
        Collider,
        Renderer
    }

	public static class ScreenSpaceHelper {
		public static Vector2 ScreenPosition(Camera camera, Vector3 position) {
			var screenPos = camera.WorldToScreenPoint(position);
			return new Vector2((int)(10000 * screenPos.x / camera.pixelWidth), (int)(10000 * screenPos.y / camera.pixelHeight));
		}

		public static Vector2 ScreenPosition(Camera camera, MonoBehaviour gameObject) {
			return ScreenPosition(camera, gameObject.transform.position);
		}

		private static Vector2[] boundsHelper = new Vector2[8];
		private static Vector2 min = Vector2.zero;
		private static Vector2 max = Vector2.zero;

		public static Rect ScreenRect(Camera camera, Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new Rect(0, 0, 0, 0);
			}
			return ScreenRect(camera, renderer.bounds);
		}

		public static Rect ScreenRect(Camera camera, Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new Rect(0, 0, 0, 0);
			}
			return ScreenRect(camera, collider.bounds);
		}

		public static Rect ScreenRect(Camera camera, Bounds bounds) {
			boundsHelper[0] = ScreenPosition(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
			boundsHelper[1] = ScreenPosition(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
			boundsHelper[2] = ScreenPosition(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
			boundsHelper[3] = ScreenPosition(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
			boundsHelper[4] = ScreenPosition(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
			boundsHelper[5] = ScreenPosition(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
			boundsHelper[6] = ScreenPosition(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
			boundsHelper[7] = ScreenPosition(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

			/* Get Max and Min bounding box positions */

			min = boundsHelper[0];
			max = boundsHelper[0];

			foreach (Vector2 vec in boundsHelper) {
				min = Vector2.Min(min, vec);
				max = Vector2.Max(max, vec);
			}

			return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
		}
	}

}