using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

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


		public static Vector3 ViewerScreenPointToWorldPoint(Vector2 screenPoint) {
            return ViewerScreenPointToWorldPoint(Camera.main, screenPoint, 0);
        }
        
		public static Vector3 ViewerScreenPointToWorldPoint(Vector2 screenPoint, float z) {
            return ViewerScreenPointToWorldPoint(Camera.main, screenPoint, z);
        }

        public static Vector3 ViewerScreenPointToWorldPoint(Camera camera, Vector2 screenPoint) {
			return ViewerScreenPointToWorldPoint(camera, screenPoint, 0);
        }

		/// <summary>
		/// Given a 2D Point in Viewer Screen Space return a Unity World Space Point at a given z depth from the given camera's perspective.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="screenPoint"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public static Vector3 ViewerScreenPointToWorldPoint(Camera camera, Vector2 screenPoint, float z) {
			screenPoint.y = Screen.height - screenPoint.y;
			var worldPoint = camera.ScreenToWorldPoint(screenPoint);
			worldPoint.z = z;
			return worldPoint;
		}

		public static Vector2Int WorldToViewerScreenPoint(Vector3 position) {
			return WorldToViewerScreenPoint(Camera.main, position);
		}

		public static Vector2Int WorldToViewerScreenPoint(MonoBehaviour gameObject) {
			return WorldToViewerScreenPoint(Camera.main, gameObject.transform.position);
		}

		public static Vector2Int WorldToViewerScreenPoint(Camera camera, MonoBehaviour gameObject) {
			return WorldToViewerScreenPoint(camera, gameObject.transform.position);
		}

		/// <summary>
		/// Given a Unity World Space point return a Viewer Screen Space Point based on the camera.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static Vector2Int WorldToViewerScreenPoint(Camera camera, Vector3 position) {
			var screenPos = camera.WorldToScreenPoint(position);
			return new Vector2Int((int)screenPos.x, (int)(camera.pixelHeight - screenPos.y));
		}

		private static Vector2Int[] boundsHelper = new Vector2Int[8];
		private static Vector2Int min = Vector2Int.zero;
		private static Vector2Int max = Vector2Int.zero;

		public static RectInt WorldBoundsToViewerScreenRect(Camera camera, Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new RectInt(0, 0, 0, 0);
			}
			return WorldBoundsToViewerScreenRect(camera, renderer.bounds);
		}

		public static RectInt WorldBoundsToViewerScreenRect(Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new RectInt(0, 0, 0, 0);
			}
			return WorldBoundsToViewerScreenRect(Camera.main, renderer.bounds);
		}

		public static RectInt WorldBoundsToViewerScreenRect(Camera camera, Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new RectInt(0, 0, 0, 0);
			}
			return WorldBoundsToViewerScreenRect(camera, collider.bounds);
		}

		public static RectInt WorldBoundsToViewerScreenRect(Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new RectInt(0, 0, 0, 0);
			}
			return WorldBoundsToViewerScreenRect(Camera.main, collider.bounds);
		}

		public static RectInt WorldBoundsToViewerScreenRect(Bounds bounds) {
			return WorldBoundsToViewerScreenRect(Camera.main, bounds);
		}

		/// <summary>
		/// Given a bounds object return a Viewer Space rectangle from the perspective of the given camera.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="bounds"></param>
		/// <returns></returns>
		public static RectInt WorldBoundsToViewerScreenRect(Camera camera, Bounds bounds) {
			boundsHelper[0] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
			boundsHelper[1] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
			boundsHelper[2] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
			boundsHelper[3] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
			boundsHelper[4] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
			boundsHelper[5] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
			boundsHelper[6] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
			boundsHelper[7] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

			/* Get Max and Min bounding box positions */

			min = boundsHelper[0];
			max = boundsHelper[0];

			foreach (Vector2Int vec in boundsHelper) {
				min = Vector2Int.Min(min, vec);
				max = Vector2Int.Max(max, vec);
			}

			return new RectInt(min.x, min.y, max.x - min.x, max.y - min.y);
		}

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Camera camera, Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("ScreenRect Called on null Renderer");
                return new Vector2Int(0, 0);
            }
            return WorldBoundsToViewerScreenRectPosition(camera, renderer.bounds);
        }

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("ScreenRect Called on null Renderer");
                return new Vector2Int(0, 0);
            }
            return WorldBoundsToViewerScreenRectPosition(Camera.main, renderer.bounds);
        }

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Camera camera, Collider collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return new Vector2Int(0, 0);
            }
            return WorldBoundsToViewerScreenRectPosition(camera, collider.bounds);
        }

        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Collider collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return new Vector2Int(0, 0);
            }
            return WorldBoundsToViewerScreenRectPosition(Camera.main, collider.bounds);
        }

		/// <summary>
		/// Given a bounds object return the position of the Viewer Space rectangle from the perspective of the given camera.
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="bounds"></param>
		/// <returns></returns>
        public static Vector2Int WorldBoundsToViewerScreenRectPosition(Camera camera, Bounds bounds) {
            boundsHelper[0] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
            boundsHelper[1] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
            boundsHelper[2] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
            boundsHelper[3] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
            boundsHelper[4] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
            boundsHelper[5] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
            boundsHelper[6] = WorldToViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
            boundsHelper[7] = WorldToViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

            /* Get Max and Min bounding box positions */

            min = boundsHelper[0];
            max = boundsHelper[0];

            foreach (Vector2Int vec in boundsHelper) {
                min = Vector2Int.Min(min, vec);
                max = Vector2Int.Max(max, vec);
            }

			return new Vector2Int(min.x, min.y);
        }
		
    }

	public static class Extensions {
		public static JObject ToJObject(this Vector2Int vec) {
			return new JObject {
				{"x", vec.x },
				{"y", vec.y }
			};
		}

		public static JObject ToJObject(this Vector2 vec) {
			return new JObject {
				{"x", vec.x },
				{"y", vec.y }
			};
		}

		public static JObject ToJObject(this Vector3 vec) {
			return new JObject {
				{"x",vec.x },
				{"y",vec.y },
				{"z",vec.z }
			};
        }

		public static JObject ToJObject(this RectInt rect) {
			return new JObject {
				{"x", rect.x },
				{"y", rect.y },
				{"w", rect.width },
				{"h", rect.height }
			};
		
		}

		public static JObject ToJObject(this MonoBehaviour mb) {
			JObject ret = new JObject();
			foreach (PropertyInfo pi in mb.GetType().GetProperties()){ 
				try {
					ret[pi.Name] = pi.GetValue(mb).ToString();
				}
				catch {
					continue;
				}
			}
			return ret;
        }

		public static JObject ToJObject(this GameObject gob) {
			JObject ret = new JObject();
			foreach (PropertyInfo pi in gob.GetType().GetProperties()) {
				try {
					ret[pi.Name] = pi.GetValue(gob).ToString();
				}
				catch {
					continue;
				}
			}
			JObject comps = new JObject();
			foreach(MonoBehaviour comp in gob.GetComponents<MonoBehaviour>()) {
				comps[comp.GetType().Name] = comp.ToJObject();
            }
			ret["Components"] = comps;
			return ret;
		}
	}

}
