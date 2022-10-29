using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;
using System.Linq;

public class TrackableWalker : MetaDataTrackable {
    //Tryin to evoke a similar energy to the Pac-Man Ghost names
    public static string[] Walker_Names = {
        "Walky", "Blocky", "Chalky", "Docky", "Flocky", "Gawky", "Hockey",
        "Jocky", "Knocky", "Locky", "Mocky", "Pocky",  "Rocky", "Stocky", "Talky", "Clyde"
    };

    public enum WalkerColor {
        Red, Yellow, Green, Blue
    }

    public enum CycleMode {
        Loop,
        LoopWithStart,
        OneWayTeleport,
        PingPong,
        Random
    }


    private int currentWaypointDex;
    public Vector3 startingPoint;


    public string secretName = string.Empty;
    public WalkerColor color = WalkerColor.Red;
    public Vector2[] waypoints;
    public CycleMode cycleMode = CycleMode.Loop;
    public float moveSpeed;
    public float timeAtPoints;

    public enum WalkingState {
        Idle,
        Walking,
        Waiting
    }

    public WalkingState walkingState = WalkingState.Idle;

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        objectKey = this.name;
        startingPoint = this.transform.position;
        secretName = Walker_Names[Random.Range(0, Walker_Names.Length - 1)];

        switch (cycleMode) {
            case CycleMode.LoopWithStart:
            case CycleMode.PingPong:
                Vector2[] temp = new Vector2[waypoints.Length+1];
                temp[0] = startingPoint;
                for(int i = 0; i < waypoints.Length; i++) {
                    temp[i + 1] = waypoints[i];
                }
                waypoints = temp;
                currentWaypointDex = 1;
                break;
            default:
                currentWaypointDex = 0;
                break;
        }
        walkingState = WalkingState.Walking;
        StartCoroutine(WalkCycle());
    }

    IEnumerator WalkCycle() {
        while (walkingState != WalkingState.Idle) {
            float startMoveTime = Time.time;
            Vector3 originalPosition = transform.position;
            float moveTime = Vector2.Distance(originalPosition, waypoints[currentWaypointDex]) / moveSpeed;
            while (Time.time - startMoveTime < moveTime) {
                transform.position = Vector3.Lerp(originalPosition, waypoints[currentWaypointDex],
                    (Time.time - startMoveTime) / moveTime);
                yield return new WaitForEndOfFrame();
            }
            walkingState = WalkingState.Waiting;
            yield return new WaitForSeconds(timeAtPoints);

            if (cycleMode == CycleMode.Random) {
                var next = Random.Range(0, waypoints.Length);
                while(next == currentWaypointDex) {
                    next = Random.Range(0, waypoints.Length);
                }
                currentWaypointDex = next;
            }
            else {
                currentWaypointDex += 1;
                if (currentWaypointDex == waypoints.Length) {
                    switch (cycleMode) {
                        case CycleMode.Loop:
                        case CycleMode.LoopWithStart:
                            currentWaypointDex %= waypoints.Length;
                            break;
                        case CycleMode.OneWayTeleport:
                            transform.position = startingPoint;
                            currentWaypointDex = 0;
                            break;
                        case CycleMode.PingPong:
                            waypoints = waypoints.Reverse().ToArray();
                            currentWaypointDex = 1;
                            break;
                    }
                }
            }
            walkingState = WalkingState.Walking;
        }
        yield break;
    }
    
    public override JObject KeyFrameData() {
        JObject job =  base.KeyFrameData();
        job["secret_name"] = secretName;
        job["color"] = color.ToString();
        job["state"] = walkingState.ToString();
        job["next_waypoint"] = ScreenSpaceHelper.ViewerScreenPoint(waypoints[currentWaypointDex]).ToJObject();
        return job;
    }
}
