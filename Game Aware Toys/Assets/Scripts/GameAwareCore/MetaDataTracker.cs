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

        private const string LATEST_FRAME = "latest";
        private const string START_FRAME = "start_frame";
        private const string END_FRAME = "end_frame";
        private const string PUB_SUB_CHANNEL = "server_control";

        public enum RecordingUpdate {
            Update,
            LateUpdate,
            FixedUpdate
        }

        private List<IMetaDataTrackable> keyItems = new List<IMetaDataTrackable>();
        private List<IMetaDataTrackable> tweenItems = new List<IMetaDataTrackable>();
        private List<IMetaDataTrackable> newItems = new List<IMetaDataTrackable>();

        private long keyFrameNum = 0;
        private float lastKeyTime = float.NegativeInfinity;
        private float lastTweenTime = float.NegativeInfinity;

        private JArray tweens = new JArray();
        private JObject currentFrameData = new JObject();

        private ConnectionMultiplexer redisConn;
        private IDatabase redDb = null;

        [Tooltip("The URL of the Middleware Redis service")]
        public string middleWareURI = "localhost";
        
        [Tooltip("The port used by the Middleware Redis service. The conventional Redis port is 6379.")]
        public int middleWarePort = 6379;
        
        [Tooltip("The number of seconds used to keep the connection to the Middleware service alive")]
        public int keepAliveTime = 180;
        
        [Tooltip("Password for the Middleware Redis service")]
        public string middleWareRedisPassword = "changeme";
        
        [Tooltip("True = a connection to the MiddleWare service will be made on Start. False = a connection to the Middleware service must be triggered at some point later.")]
        public bool BeginMetaDataOnStart = false;
        
        [Tooltip("Name of your game, hardcoded for the start message")]
        public string gameName = "";

        [Tooltip("Name of the streamer, ideally set before connection")]
        public string streamerName = "";

        [Tooltip("Which of Unity's update loops should be used to record data. This is more for making it easy to do testing between the options for now. I don't think this setting will make it into the final version.")]
        public RecordingUpdate updateMode = RecordingUpdate.Update;

        [Tooltip("The number of key frames per second")]
        public float keyFrameRate = 1.0f;

        [Tooltip("The number of tween frames per second")]
        public float tweenFrameRate = 24f;

        public bool Recording { get; private set; }

        public bool Connected { get { return redDb != null; } }

        public string LastKeyFrameSent { get; private set; }

        public IReadOnlyList<IMetaDataTrackable> CurrentTrackables { 
            get {
                return keyItems.AsReadOnly();
            }
        } 

        private float CurrentTime {
            get {
                return updateMode == RecordingUpdate.FixedUpdate ? Time.fixedTime : Time.time;
            }
        }

        public enum DebugSetting {
            None,
            All,
            ConstantsOnly
        }
        
        [Tooltip("Controls debug printing. All=prints everything, None=turns off everything, ConstantsOnly=only prints start_frame, latest, and end_frame")]
        public DebugSetting debugSetting = DebugSetting.All;

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
            if (BeginMetaDataOnStart) { 
                yield return new WaitForEndOfFrame();
                yield return StartMetaDataCoroutine();
            }
        }

        void InitConnection() {
            ConfigurationOptions config = new ConfigurationOptions {
                EndPoints = {
                    { middleWareURI, middleWarePort },
                },
                KeepAlive = keepAliveTime,
                Password = middleWareRedisPassword
            };

            redisConn = ConnectionMultiplexer.Connect(config);
            redDb = redisConn.GetDatabase();
        }


        public void StartMetaData() {
            if (!Connected && !Recording) {
                StartCoroutine(StartMetaDataCoroutine());
            }
        }

        IEnumerator StartMetaDataCoroutine() {
            if (!Connected) {
                InitConnection();
            }
            while (!Connected) {
                yield return new WaitForEndOfFrame();
            }
            var startMessage = new JObject {
                {"game_name", gameName },
                {"streamer_name", streamerName },
                {"key_frame_rate", keyFrameRate },
                {"tween_frame_rate", tweenFrameRate },
                {"game_time", (int)CurrentTime/1000 },
                {"clock_mills",DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
                {"screen_width", Screen.width },
                {"screen_height", Screen.height },
            };
            string mess = JsonConvert.SerializeObject(startMessage);
            PublishMessageToMiddleware(PUB_SUB_CHANNEL, "start");
            WriteMetaDataToMiddleware(START_FRAME, mess, false);

            Recording = true;
            SnapKeyFrame();
        }

        public void StopMetaData() {
            Recording = false;
            var endMessage = new JObject {
                {"final_frame_num", keyFrameNum },
                {"game_time", (int)CurrentTime/1000 },
                {"clock_mills",DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
            };
            string mess = JsonConvert.SerializeObject(endMessage);
            WriteMetaDataToMiddleware(END_FRAME, mess, false);
            PublishMessageToMiddleware(PUB_SUB_CHANNEL, "end-frame");
            redDb = null;
            if (redisConn != null) {
                redisConn.Close();
            }
        }

        /// <summary>
        /// The principle here is to allow for a user to pause the metaData system from sending data for some time.
        /// The immediate flag is to allow for letting the current keyframe playout (immediate=false) or if it should stop everything now (immediate=true) this would require changes throughout the system.
        /// </summary>
        /// <param name="immediate"></param>
        public void PauseMetaData(bool immediate=false) {
            throw new NotImplementedException("Haven't implemented metadata pausing yet.");
        }


        private void WriteMetaDataToMiddleware(string key, string message, bool asynchronous) {
            if (Connected) {
                if (asynchronous) {
                    switch (debugSetting) {
                        case DebugSetting.All:
                            Debug.LogFormat("Redis Write Async: {0}: {1}", key, message);
                            break;
                        case DebugSetting.ConstantsOnly:
                            if (key == LATEST_FRAME || key == START_FRAME || key == END_FRAME) {
                                Debug.LogFormat("Redis Write Async: {0}: {1}", key, message);
                            }
                            break;
                    }
                    redDb.StringSetAsync(key, message, flags: CommandFlags.FireAndForget);
                    if(key == LATEST_FRAME) {
                        LastKeyFrameSent = message;
                    }
                }
                else {
                    switch (debugSetting) {
                        case DebugSetting.All:
                            Debug.LogFormat("Redis Write: {0}: {1}", key, message);
                            break;
                        case DebugSetting.ConstantsOnly:
                            if (key == LATEST_FRAME || key == START_FRAME || key == END_FRAME) {
                                Debug.LogFormat("Redis Write: {0}: {1}", key, message);
                            }
                            break;
                    }
                    redDb.StringSet(key, message);
                    if (key == LATEST_FRAME) {
                        LastKeyFrameSent = message;
                    }
                }
            }
        }

        private void PublishMessageToMiddleware(string channel, string message) {
            if (Connected) {
                redDb.Publish(channel, message);
            }
        }



        void FixedUpdate() {
            if (updateMode != RecordingUpdate.FixedUpdate || !Recording) return;

            if (Time.fixedTime - lastKeyTime > 1 / keyFrameRate) {
                SendKeyFrame();
            }
            else if(Time.fixedTime - lastTweenTime > 1 / tweenFrameRate) {
                SnapTweenFrame();
            }
        }

        void Update() {
            if (updateMode != RecordingUpdate.Update || !Recording) return;

            if (Time.time - lastKeyTime > 1 / keyFrameRate) {
                SendKeyFrame();
            }
            else if (Time.time - lastTweenTime > 1 / tweenFrameRate) {
                SnapTweenFrame();
            }
        }

        void LateUpdate() {
            if (updateMode != RecordingUpdate.LateUpdate || !Recording) return;

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
                tweens = new JArray();
            }
            string frameString = JsonConvert.SerializeObject(currentFrameData);

            WriteMetaDataToMiddleware(keyFrameNum.ToString(), frameString, true);
            WriteMetaDataToMiddleware(LATEST_FRAME, frameString, true);

            keyFrameNum += 1;
            SnapKeyFrame();
        }

        /** General Data Schema
         * keyFrameNum: {"game_time":int, // in milliseconds as browsers encode time
         *               "frame":int,
         *               "key":{"obj_id1":{...}, "obj_id2":{...}, ...}, 
         *               "tweens": [{"dt":int, "game_time":int, "obj_id1":{...}, "obj_id2":{...}, ...},
         *                          {"dt":int, "game_time":int, "obj_id1":{...}, "obj_id2":{...}, ...}]}
         *                    
         * If there are no Tween objects being recorded or if they report nothing then the tween list might be missing
         * keyFrameNum: {"key":{"obj_id1":{}, "obj_id2":{}, ...}}
         */
        private void SnapKeyFrame() {
            keyItems.AddRange(newItems);
            foreach(IMetaDataTrackable mdo in newItems) {
                if (mdo.FrameType == MetaDataFrameType.Inbetween) {
                    tweenItems.Add(mdo);
                }
            }
            newItems.Clear();

            currentFrameData = new JObject {
                {"game_time", (int)CurrentTime/1000 },
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
            if (tweenItems.Count == 0 || !Recording) {
                return;
            }

            //inbetweenNum += 1;
            JObject newInbetween = new JObject {
                    {"dt", (int)CurrentTime/1000 - (int)lastTweenTime/1000 },
                    {"game_time", (int)CurrentTime/1000},
                    //{"frame_num", inbetweenNum }
                };
            foreach (IMetaDataTrackable mdo in tweenItems) {
                JObject mdoTween = mdo.InbetweenData();
                if (mdoTween.Count > 0) {
                    newInbetween[mdo.ObjectKey] = mdo.InbetweenData();
                }
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
            newItems.Add(mdo);
        }

        public void RemoveTrackableObject(IMetaDataTrackable mdo) {
            keyItems.Remove(mdo);
            tweenItems.Remove(mdo);
        }

        void OnDestroy() {
            StopMetaData();
        }
    }
}
