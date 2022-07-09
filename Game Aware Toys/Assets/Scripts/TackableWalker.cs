using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;

public class TackableWalker : MetaDataTrackable {
    //Tryin to evoke a similar energy to the Pac-Man Ghost names
    public static string[] Walker_Names = {
        "Walky", "Blocky", "Chalky", "Pocky", "Talky", "Mocky", "Gawky", "Flocky", "Rocky", "Stocky", "Clyde"
    };

    public string secretName = string.Empty;
    public Vector2[] waypoints;
    public int currentWaypointDex;
    public float speed;
    public float waitTimeAtPoints;

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
            while ((waypoints[currentWaypointDex] - (Vector2)transform.position).magnitude > .0001) {
                Vector2 direction = waypoints[currentWaypointDex] - (Vector2)transform.position;
                transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            walkingState = WalkingState.Waiting;
            yield return new WaitForSeconds(waitTimeAtPoints);
            currentWaypointDex += 1;
            currentWaypointDex %= waypoints.Length;
            walkingState = WalkingState.Walking;
        }
        yield break;
    }
    
    public override JObject KeyFrameData() {
        JObject job =  base.KeyFrameData();
        job["secret_name"] = secretName;
        return job;
    }
}
