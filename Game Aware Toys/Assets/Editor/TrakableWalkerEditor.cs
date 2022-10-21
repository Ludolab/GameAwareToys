using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(TrackableWalker))]
public class TrackableWalkerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        TrackableWalker walker = (TrackableWalker)target;

        EditorGUILayout.BeginVertical();
        if(GUILayout.Button("Snap Path")) {
            for(int i = 0; i < walker.waypoints.Length; i++) {
                Vector2 vec = walker.waypoints[i];
                vec.x = Mathf.Round(vec.x * 2) / 2;
                vec.y = Mathf.Round(vec.y * 2) / 2;
                walker.waypoints[i] = vec;
            }
        }

        EditorGUILayout.EndVertical();

    }

    public void OnSceneGUI() {
        TrackableWalker walker = (TrackableWalker)target;

        Color color = walker.color switch {
            TrackableWalker.WalkerColor.Red => Color.red,
            TrackableWalker.WalkerColor.Blue => Color.blue,
            TrackableWalker.WalkerColor.Green => Color.green,
            TrackableWalker.WalkerColor.Yellow => Color.yellow,
            _ => Color.cyan,
        };

        Handles.color = color;
        List<Vector3> linePoints = new List<Vector3>();

        if (!Application.isPlaying) {
            linePoints.Add(walker.transform.position);
        }
        else {
            if(walker.cycleMode == TrackableWalker.CycleMode.OneWayTeleport) {
                linePoints.Add(walker.startingPoint);
            }
        }

        for (int i = 0; i < walker.waypoints.Length; i++) {
            linePoints.Add(walker.waypoints[i]);
            walker.waypoints[i] = Handles.PositionHandle(walker.waypoints[i], Quaternion.identity);
            GUI.color = color;
            Handles.Label(walker.waypoints[i], i.ToString());

        }

        switch (walker.cycleMode) {
            case TrackableWalker.CycleMode.LoopWithStart:
                linePoints.Add(linePoints[0]);
                break;
            case TrackableWalker.CycleMode.Loop:
                if (!Application.isPlaying) {
                    linePoints.Add(linePoints[1]);
                }
                else {
                    linePoints.Add(linePoints[0]);
                }
                break;
        }
        
        Handles.DrawAAPolyLine(8, linePoints.ToArray());
    }


    


}
