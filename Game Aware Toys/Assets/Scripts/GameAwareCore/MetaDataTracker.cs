using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using StackExchange.Redis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameAware {

    public class MetaDataTracker : MonoBehaviour {
        public static MetaDataTracker Instance { get; private set; } = null;

        private List<IMetaDataTrackable> keyItems = new List<IMetaDataTrackable>();
        private List<IMetaDataTrackable> tweenItems = new List<IMetaDataTrackable>();

        private long keyFrameNum = 0;
        private float lastKeyTime = float.NegativeInfinity;
        private float lastTweenTime = float.NegativeInfinity;

        private JArray tweens = new JArray();
        private JObject currentFrameData = new JObject();

        [Tooltip("The URL of the Redis Middlware service")]
        public string url = "localhost";
        [Tooltip("The port used by the Redis Middleware service. The conventional Redis port is 6379.")]
        public int port = 6379;
        [Tooltip("The number of seconds used to keep the connection to the Middleware service alive")]
        public int keepAliveTime = 180;
        [Tooltip("Password for the Redis Middleware service")]
        public string password = "changeme";
        [Tooltip("True = a connection to the MiddleWare service will be made on Start. False = a connection to the Middleware service must be triggered at some point later.")]
        public bool connectOnStart = false;

        public bool recording = false;

        [Tooltip("Name of your game, hardcoded for the start message")]
        public string gameName = "";
        [Tooltip("Name of the streamer, ideally set before connection")]
        public string streamerName = "";

        private ConnectionMultiplexer redisConn;
        private IDatabase redDb = null;
        private const string LATEST_FRAME = "latest";
        private const string START_FRAME = "start_frame";
        private const string END_FRAME = "end_frame";
        private const string PUB_SUB_CHANNEL = "server_control";

        public enum RecordingUpdate {
            Update,
            LateUpdate,
            FixedUpdate
        }

        [Tooltip("Which of Unity's update loops should be used to record data. This is more for making it easy to do testing between the options for now. I don't think this setting will make it into the final version.")]
        public RecordingUpdate updateMode = RecordingUpdate.Update;

        private float CurrentTime {
            get {
                return updateMode == RecordingUpdate.FixedUpdate ? Time.fixedTime : Time.time;
            }
        }

        [Tooltip("The number of key frames per second")]
        public float keyFrameRate = 1.0f;

        [Tooltip("The number of tween frames per second")]
        public float tweenFrameRate = 24f;

        // Use this for initialization
        void Awake() {
            if (Instance != null) {
                Debug.LogWarning("Multiple MetaDataTrackers in Scene");
                Destroy(this.gameObject);
            }
            else {
                Instance = this;
                DontDestroyOnLoad(this);
                SceneManager.activeSceneChanged += OnSceneChange;
            }
        }

        IEnumerator Start() {
            yield return new WaitForEndOfFrame();
            if (connectOnStart) {
                InitConnection();
                yield return StartMetaData();
            }
            SnapKeyFrame();
        }

        private void WriteMetaData(string key, string message) {
            WriteMetaData(key, message, true);
        }

        void InitConnection() {
            ConfigurationOptions config = new ConfigurationOptions {
                EndPoints = {
                    { url, port },
                },
                KeepAlive = keepAliveTime,
                Password = password
            };

            redisConn = ConnectionMultiplexer.Connect(config);
            redDb = redisConn.GetDatabase();
        }

        private void WriteMetaData(string key, string message, bool asynchronous) {

            /*if (redDb == null) { InitConnection(); }*/

            if (redDb != null && recording) {
                if (asynchronous) {
                    Debug.Log("Writing Meta Data Async: " + key + ", " + message);
                    redDb.StringSetAsync(key, message, flags: CommandFlags.FireAndForget);
                }
                else {
                    Debug.Log("Writing Meta Data: " + key + ", " + message);
                    redDb.StringSet(key, message);
                }
            }
        }

        private void PublishMetaData(string channel, string message) {
            /*if (redDb == null) {
                InitConnection();
            }*/
            /*ConfigurationOptions config = new ConfigurationOptions {
                EndPoints = {
                    { metaDataURL, metaDataPort },
                },
                KeepAlive = metaDataKeepAlive,
                Password = metaDataPassword
            };

            redisConn = ConnectionMultiplexer.Connect(config);
            redDb = redisConn.GetDatabase();*/
            if (redDb != null && recording) {
                redDb.Publish(channel, message);
            }
        }

        public IEnumerator StartMetaData() {
            while(redDb == null) {
                yield return new WaitForEndOfFrame();
            }
            var startMessage = new JObject {
                {"game_name", gameName },
                {"streamer_name", streamerName },
                {"key_frame_rate", keyFrameRate },
                {"tween_frame_rate", tweenFrameRate },
                {"game_secs", CurrentTime },
                {"clock_mills",DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
            };
            string mess = JsonConvert.SerializeObject(startMessage);
            recording = true;
            PublishMetaData(PUB_SUB_CHANNEL, "start");
            WriteMetaData(START_FRAME, mess);
        }

        public void EndMetaDataConnection() {
            var endMessage = new JObject {
                {"final_frame_num", keyFrameNum },
                {"game_secs", CurrentTime },
                {"clock_mills",DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
            };
            string mess = JsonConvert.SerializeObject(endMessage);
            WriteMetaData(END_FRAME, mess);
            PublishMetaData(PUB_SUB_CHANNEL, "end");
        }

        /** General Data Schema
         * keyFrameNum: {"game_time":float,
         *               "frame_num":int,
         *               "key":{"obj_id1":{...}, "obj_id2":{...}, ...}, 
         *               "tweens": [{"dt":float, "game_time":float, "obj_id1":{...}, "obj_id2":{...}, ...},
         *                          {"dt":float, "game_time":float, "obj_id1":{...}, "obj_id2":{...}, ...}]}
         *                    
         * If there are no Tween objects being recorded or if they report nothing then the tween list might be missing
         * keyFrameNum: {"key":{"obj_id1":{}, "obj_id2":{}, ...}}
         * 
         * Possible Future Schema
         * keyFrameNum: {"key":{"obj_id1":{...}, "obj_id2":{...}, ...}, 
         *               "tweens": [{"obj_id1":{...}, "obj_id2":{...}, ...},
         *                          {"obj_id1":{...}, "obj_id2":{...}, ...}],
         *               "events": [{"event1"}, {"event2"}]}
         * 
         */
        void FixedUpdate() {
            if (updateMode != RecordingUpdate.FixedUpdate || !recording) return;

            if (Time.fixedTime - lastKeyTime > 1 / keyFrameRate) {
                SendKeyFrame();
            }
            else if(Time.fixedTime - lastTweenTime > 1 / tweenFrameRate) {
                SnapTweenFrame();
            }
        }

        void Update() {
            if (updateMode != RecordingUpdate.Update || !recording) return;

            if (Time.time - lastKeyTime > 1 / keyFrameRate) {
                SendKeyFrame();
            }
            else if (Time.time - lastTweenTime > 1 / tweenFrameRate) {
                SnapTweenFrame();
            }
        }

        void LateUpdate() {
            if (updateMode != RecordingUpdate.LateUpdate || !recording) return;

            if (Time.time - lastKeyTime > 1 / keyFrameRate) {
                SendKeyFrame();
            }
            else if (Time.time - lastTweenTime > 1 / tweenFrameRate) {
                SnapTweenFrame();
            }
        }


        private void SendKeyFrame() {
            if (tweens.Count > 0) {
                currentFrameData["tweens"] = tweens;
            }
            string frameString = JsonConvert.SerializeObject(currentFrameData);

            WriteMetaData(keyFrameNum.ToString(), frameString, true);
            WriteMetaData(LATEST_FRAME, frameString, true);

            keyFrameNum += 1;
            SnapKeyFrame();
        }


        private void SnapKeyFrame() {
            if(!recording) {
                return;
            }
            currentFrameData = new JObject {
                {"game_time", CurrentTime },
                {"frame", keyFrameNum }
            };
            JObject key = new JObject();
            foreach (IMetaDataTrackable mdo in keyItems) {
                key[mdo.ObjectKey] = mdo.KeyFrameData();
            }
            currentFrameData["key"] = key;

            lastKeyTime = CurrentTime;
            lastTweenTime = CurrentTime;
            //currentFrameData["frame"] = keyFrameNum;
        }

        private void SnapTweenFrame() {
            if (tweenItems.Count == 0 || !recording) {
                return;
            }

            //inbetweenNum += 1;
            JObject newInbetween = new JObject {
                    {"dt", CurrentTime - lastTweenTime },
                    {"game_time", CurrentTime},
                    //{"frame_num", inbetweenNum }
                };
            foreach (IMetaDataTrackable mdo in tweenItems) {
                newInbetween[mdo.ObjectKey] = mdo.InbetweenData();
            }
            if (newInbetween.Count > 0) {
                tweens.Add(newInbetween);
                lastTweenTime = CurrentTime;
            }
        }

        void OnSceneChange(Scene current, Scene next) {
            keyItems = keyItems.Where(md => md.PersistAcrossScenes).ToList();
            tweenItems = tweenItems.Where(md => md.PersistAcrossScenes).ToList();
        }

        public void AddTrackableObject(IMetaDataTrackable mdo) {
            keyItems.Add(mdo);
            if (mdo.FrameType == MetaDataFrameType.Inbetween) {
                tweenItems.Add(mdo);
            }
        }

        public void RemoveTrackableObject(IMetaDataTrackable mdo) {
            keyItems.Remove(mdo);
            tweenItems.Remove(mdo);
        }

        void OnDestroy() {
            EndMetaDataConnection();
        }
    }
}
