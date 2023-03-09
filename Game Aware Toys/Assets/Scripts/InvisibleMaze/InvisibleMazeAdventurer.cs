using GameAware;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleMazeAdventurer : MetaDataTrackable
{

    public float speed;
    public Vector3 targetPosition;
    private Vector2 spawnPoint;
    public bool moving= false;
    InvisibleMazeManager mazeManager;

    // Start is called before the first frame update
    override protected void Start() {
        base.Start();
        spawnPoint = transform.position;
        mazeManager = FindObjectOfType<InvisibleMazeManager>();
    }

    void Respawn() {
        moving = false;
        transform.position = spawnPoint;
    }

    IEnumerator TakeABeat() {
        yield return new WaitForSeconds(.2f);
        moving = false;
    }

    // Update is called once per frame
    void Update() {
        if (moving) {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed*Time.deltaTime);
            if(Vector2.Distance(transform.position, targetPosition) == 0){
                InvisibleMazeManager.MazeTile tile =  mazeManager.VisitMazeTile(transform.position);
                if(tile != null && tile.type == InvisibleMazeManager.TileType.Danger) {
                    Respawn();
                }
                moving = false;
            }
        }
        else {
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                targetPosition = transform.position + Vector3.up;
                moving = true;
            }
            else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  {
                targetPosition = transform.position + Vector3.down;
                moving = true;
            }
            else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { 
                targetPosition = transform.position + Vector3.left;
                moving = true;
            }
            else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { 
                targetPosition = transform.position + Vector3.right;
                moving = true;
            }
        }
    }
}
