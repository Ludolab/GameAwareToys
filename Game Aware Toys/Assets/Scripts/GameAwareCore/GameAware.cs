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

		public static Vector2Int ViewerScreenPoint(Vector3 position) {
			return ViewerScreenPoint(Camera.main, position);
		}

		public static Vector2Int ViewerScreenPoint(MonoBehaviour gameObject) {
			return ViewerScreenPoint(Camera.main, gameObject.transform.position);
		}

		public static Vector2Int ViewerScreenPoint(Camera camera, MonoBehaviour gameObject) {
			return ViewerScreenPoint(camera, gameObject.transform.position);
		}

		public static Vector2Int ViewerScreenPoint(Camera camera, Vector3 position) {
			var screenPos = camera.WorldToScreenPoint(position);
			return new Vector2Int((int)screenPos.x, (int)(camera.pixelHeight - screenPos.y));
		}

		private static Vector2Int[] boundsHelper = new Vector2Int[8];
		private static Vector2Int min = Vector2Int.zero;
		private static Vector2Int max = Vector2Int.zero;

		public static RectInt ViewerScreenRect(Camera camera, Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new RectInt(0, 0, 0, 0);
			}
			return ViewerScreenRect(camera, renderer.bounds);
		}

		public static RectInt ViewerScreenRect(Renderer renderer) {
			if (renderer == null) {
				Debug.LogWarning("ScreenRect Called on null Renderer");
				return new RectInt(0, 0, 0, 0);
			}
			return ViewerScreenRect(Camera.main, renderer.bounds);
		}

		public static RectInt ViewerScreenRect(Camera camera, Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new RectInt(0, 0, 0, 0);
			}
			return ViewerScreenRect(camera, collider.bounds);
		}

		public static RectInt ViewerScreenRect(Collider collider) {
			if (collider == null) {
				Debug.LogWarning("ScreenRect Called on null Collider");
				return new RectInt(0, 0, 0, 0);
			}
			return ViewerScreenRect(Camera.main, collider.bounds);
		}



		public static RectInt ViewerScreenRect(Camera camera, Bounds bounds) {
			boundsHelper[0] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
			boundsHelper[1] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
			boundsHelper[2] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
			boundsHelper[3] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
			boundsHelper[4] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
			boundsHelper[5] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
			boundsHelper[6] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
			boundsHelper[7] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

			/* Get Max and Min bounding box positions */

			min = boundsHelper[0];
			max = boundsHelper[0];

			foreach (Vector2Int vec in boundsHelper) {
				min = Vector2Int.Min(min, vec);
				max = Vector2Int.Max(max, vec);
			}

			return new RectInt(min.x, min.y, max.x - min.x, max.y - min.y);
		}

        public static Vector2Int ViewerScreenRectPosition(Camera camera, Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("ScreenRect Called on null Renderer");
                return new Vector2Int(0, 0);
            }
            return ViewerScreenRectPosition(camera, renderer.bounds);
        }

        public static Vector2Int ViewerScreenRectPosition(Renderer renderer) {
            if (renderer == null) {
                Debug.LogWarning("ScreenRect Called on null Renderer");
                return new Vector2Int(0, 0);
            }
            return ViewerScreenRectPosition(Camera.main, renderer.bounds);
        }

        public static Vector2Int ViewerScreenRectPosition(Camera camera, Collider collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return new Vector2Int(0, 0);
            }
            return ViewerScreenRectPosition(camera, collider.bounds);
        }

        public static Vector2Int ViewerScreenRectPosition(Collider collider) {
            if (collider == null) {
                Debug.LogWarning("ScreenRect Called on null Collider");
                return new Vector2Int(0, 0);
            }
            return ViewerScreenRectPosition(Camera.main, collider.bounds);
        }

        public static Vector2Int ViewerScreenRectPosition(Camera camera, Bounds bounds) {
            boundsHelper[0] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));  //ftl
            boundsHelper[1] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));  //ftr
            boundsHelper[2] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));  //fbr
            boundsHelper[3] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));  //fbl
            boundsHelper[4] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));  //btl
            boundsHelper[5] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));  //btr
            boundsHelper[6] = ViewerScreenPoint(camera, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));  //bbr
            boundsHelper[7] = ViewerScreenPoint(camera, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));  //bbl

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
