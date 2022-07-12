using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

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

	/// <summary>
	/// An integer based Vector2 struct to simplify screenspace math and avoid serializing extra properties of the built in vector structs.
	/// 
	/// OPTIMIZATION POTENTIAL
	/// If we stick with the paradigm of return screen space as an integer fraction of 10,000 we could probably get away with this class 
	/// being based on shorts instead of ints if saving memory space is ever a concern.
	/// </summary>
	public struct IntVector2 {
		public static IntVector2 zero = new IntVector2(0, 0);

		public static IntVector2 Min(IntVector2 vec1, IntVector2 vec2) {
			return new IntVector2(Mathf.Min(vec1.x, vec2.x), Mathf.Min(vec1.y, vec2.y));
		}
		public static IntVector2 Max(IntVector2 vec1, IntVector2 vec2) {
			return new IntVector2(Mathf.Max(vec1.x, vec2.x), Mathf.Max(vec1.y, vec2.y));
		}

		public int x;
		public int y;

		public IntVector2 (int x, int y) {
			this.x = x;
			this.y = y;
        }

		public IntVector2(Vector2 vec) {
			x = (int)vec.x;
			y = (int)vec.y;
        }

		public JObject toJObject() {
			return new JObject {
				{"x", x },
				{"y", y }
			};
        }
	}

	public struct IntRect {
		public float x;
		public float y;
		public float w;
		public float h;

		public IntRect (float x, float y, float w, float h) {
			this.x = x;
			this.y = y;
			this.w = w;
			this.h = h;
		}

		public JObject toJObject() {
			return new JObject {
				{"x", x },
				{"y", y },
				{"w", w },
				{"h", h }
			};
		}

	}

	public static class ScreenSpaceHelper {

		public static IntVector2 ScreenPosition(Vector3 position) {
			return ScreenPosition(Camera.main, position);
		}

		public static IntVector2 ScreenPosition(MonoBehaviour gameObject) {
			return ScreenPosition(Camera.main, gameObject.transform.position);
		}

		public static IntVector2 ScreenPosition(Camera camera, MonoBehaviour gameObject) {
			return ScreenPosition(camera, gameObject.transform.position);
		}

		public static IntVector2 ScreenPosition(Camera camera, Vector3 position) {
			var screenPos = camera.WorldToScreenPoint(position);
			return new IntVector2((int)(10000 * screenPos.x / camera.pixelWidth), (int)(10000 * screenPos.y / camera.pixelHeight));
		}

		private static IntVector2[] boundsHelper = new IntVector2[8];
		private static IntVector2 min = IntVector2.zero;
		private static IntVector2 max = IntVector2.zero;

		public static IntRect ScreenRect(Camera camera, Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new IntRect(0, 0, 0, 0);
			}
			return ScreenRect(camera, renderer.bounds);
		}

		public static IntRect ScreenRect(Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new IntRect(0, 0, 0, 0);
			}
			return ScreenRect(Camera.main, renderer.bounds);
		}

		public static IntRect ScreenRect(Camera camera, Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new IntRect(0, 0, 0, 0);
			}
			return ScreenRect(camera, collider.bounds);
		}

		public static IntRect ScreenRect(Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new IntRect(0, 0, 0, 0);
			}
			return ScreenRect(Camera.main, collider.bounds);
		}

		public static IntRect ScreenRect(Camera camera, Bounds bounds) {
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

			foreach (IntVector2 vec in boundsHelper) {
				min = IntVector2.Min(min, vec);
				max = IntVector2.Max(max, vec);
			}

			return new IntRect(min.x, min.y, max.x - min.x, max.y - min.y);
		}
	}

}