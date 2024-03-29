using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace GameAware {
    public interface IMetaDataTrackable {

        public const string SCREEN_RECT_KEY = "screenRect";

        /// <summary>
        /// This defines the types of frames where this object should be tracked. KeyFrames are recorded at regular 
        /// intervals (1 second by default) and contain current information for all objects. Inbetweens are the frames
        /// that exist between keyframes and only contain information that is changing quickly.
        /// </summary>
        MetaDataFrameType FrameType { get; }

        /// <summary>
        /// Defines whether this object should continue to be in the metadata list between scenes or whether it should
        /// expected that a new instance will add itself again. It should generlaly correlate with using 
        /// DontDestroyOnLoad, maybe there's a way to just read that property?
        /// </summary>
        bool PersistAcrossScenes { get; }

        /// <summary>
        /// A unique ID used to make sure metadata remains properly aligned between objects. In most cases this will
        /// just be a GUID but you could potentially get fancy by intentionally overwritting a common key. In principle
        /// this key only needs to be unique within a given run of the game as we don't care about re-instantiating for 
        /// now.
        /// </summary>
        string ObjectKey { get; }

        /// <summary>
        /// A function to get a description of the object for a KeyFrame. This should contain all metadata relevant to
        /// this GameObject.
        /// 
        /// This is currently a Hashtable as a Hack, this will be replaced with a better JSON solution later.
        /// </summary>
        /// <returns>A JSON serialization of this GameObject.</returns>
        JObject KeyFrameData();

        /// <summary>
        /// A function to get description of this object for an Inbetween Frame. This should conain only metadata that 
        /// changes within the time span between KeyFrames, like positions or health. Things that don't change over 
        /// time will be tracked in the KeyFrameData.
        /// 
        /// This is currently a Hashtable as a Hack, this will be replaced with a better JSON solution later.
        /// </summary>
        /// <returns>A JSON serialization of this GameObject.</returns>
        JObject InbetweenData();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        DepthRect ScreenRect();
    }
}