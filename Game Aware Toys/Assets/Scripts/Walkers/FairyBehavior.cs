using System.Collections;
using UnityEngine;
using GameAware;
using Newtonsoft.Json.Linq;
using static TrackableWalker;

public class FairyBehavior : MetaDataTrackable {

    //Fairy names generated by: https://www.fantasynamegenerators.com/fairy-names.php
    public static string[] FAIRY_NAMES = {
        "Navi", "Din", "Faroe", "Nayru", "Cotera", "Kaysa", "Mija", "Tera", "Tingle",
        "Cirro Foggybell",
        "Cricket Pumpkinsage",
        "Echo Swiftgrass",
        "Dragonfly Icesand",
        "Bud Turtletwirl",
        "Tiny Willowmello",
        "Skylark Lightflame",
        "Firo Jumpyspark",
        "Nyx Briarpond",
        "Jeremy Magicbreath"
    };
    public static int FAIRY_COUNTER = 0;

    public string secretName;
    public FairyFountain spawningFountain;
    public float lifeSpan;
    public float moveSpeed;
    public TrackableWalker.WalkerColor color;
    private float spawnTime;

    // Start is called before the first frame update
    override protected void Start() {
        base.Start();

        FAIRY_COUNTER++;
        objectKey = string.Format("Fairy_{0}", FAIRY_COUNTER);
        secretName = FAIRY_NAMES[Random.Range(0, FAIRY_NAMES.Length - 1)];
        spawnTime = Time.time;
        StartCoroutine(WanderCycle());
    }

    // Update is called once per frame
    void Update() {
        if(Time.time - spawnTime > lifeSpan) {
            Destroy(this.gameObject);
        }
    }

    IEnumerator WanderCycle() {
        while (Time.time - spawnTime < lifeSpan) {
            //pick a random spot
            Vector2 target = (Vector2)spawningFountain.transform.position + Random.insideUnitCircle * spawningFountain.wanderRange;
            Vector3 originalPosition = transform.position;
            float startMoveTime = Time.time;
            float moveTime = Vector2.Distance(originalPosition, target) / moveSpeed;
            while (Time.time - startMoveTime < moveTime) {
                transform.position = Vector3.Lerp(originalPosition, target, (Time.time - startMoveTime) / moveTime);
                yield return new WaitForEndOfFrame();
            }
        }
        yield break;
    }

    public override JObject KeyFrameData() {
        JObject job = base.KeyFrameData();
        job["name"] = secretName;
        job["fountain"] = spawningFountain.name;
        job["color"] = color.ToString();
        job["life_remaining"] = lifeSpan - (spawnTime - Time.time);
        return job;
    }

}