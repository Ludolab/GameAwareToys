using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;

public class TrackableWalker : MetaDataTrackable {
    //Tryin to evoke a similar energy to the Pac-Man Ghost names
    public static string[] Walker_Names = {
        "Walky", "Blocky", "Chalky", "Docky", "Flocky", "Gawky", "Hockey",
        "Jocky", "Knocky", "Locky", "Mocky", "Pocky",  "Rocky", "Stocky", "Talky", "Clyde"
    };

    private int currentWaypointDex;

    public string secretName = string.Empty;
    public string color = string.Empty;
    public Vector2[] waypoints;
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
        secretName = Walker_Names[Random.Range(0, Walker_Names.Length - 1)];
        currentWaypointDex = 0;
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
            currentWaypointDex += 1;
            currentWaypointDex %= waypoints.Length;
            walkingState = WalkingState.Walking;
        }
        yield break;
    }
    
    public override JObject KeyFrameData() {
        JObject job =  base.KeyFrameData();
        job["secret_name"] = secretName;
        job["color"] = color;
        job["state"] = walkingState.ToString();
        job["next_waypoint"] = ScreenSpaceHelper.ScreenPosition(waypoints[currentWaypointDex]).toJObject();
        return job;
    }

    public override JObject InbetweenData() {
        if (walkingState == WalkingState.Walking) {
            return base.InbetweenData();
        }
        else {
            return new JObject();
        }
    }
}
