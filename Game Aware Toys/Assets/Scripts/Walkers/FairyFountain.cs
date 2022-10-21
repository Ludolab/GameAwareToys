using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyFountain : MonoBehaviour {
    [System.Serializable]
    public class FairyColor {
        public Sprite sprite;
        public TrackableWalker.WalkerColor color;
    }


    public GameObject FairyPrefab;
    public FairyColor [] fairyColors;
    public float minLifeSpan;
    public float maxLifeSpan;
    public float minSpeed;
    public float maxSpeed;

    public float wanderRange;
    
    public float minSpawnDelay;
    public float maxSpawnDelay;
    public float currentSpawnDelay;
    private float lastSpawnTime;

    // Start is called before the first frame update
    void Start() {
        SpawnFairy();
    }

    // Update is called once per frame
    void Update() {
        if(Time.time - lastSpawnTime > currentSpawnDelay) {
            SpawnFairy();
        }
    }

    public void SpawnFairy() {
        GameObject gob = Instantiate(FairyPrefab, this.transform.position, Quaternion.identity);
        FairyBehavior fairy = gob.GetComponent<FairyBehavior>();

        fairy.spawningFountain = this;
        fairy.lifeSpan = Random.Range(minLifeSpan, maxLifeSpan);
        fairy.moveSpeed = Random.Range(minSpeed, maxSpeed);

        FairyColor fc = fairyColors[Random.Range(0, fairyColors.Length)];
        fairy.color = fc.color;
        gob.GetComponent<SpriteRenderer>().sprite = fc.sprite;

        
        lastSpawnTime = Time.time;
        currentSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
    }

}
