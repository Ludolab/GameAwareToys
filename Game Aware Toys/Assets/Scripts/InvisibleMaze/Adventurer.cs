using GameAware;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InvisibleMaze {

    public class Adventurer : MetaDataTrackable {
        public float speed;
        public Vector3 targetPosition;
        private Vector2 spawnPoint;
        public bool moving = false;
        MazeManager mazeManager;
        public bool[] movable = new bool[] { true, true, true, true };
        public GemBehavior[] picakbles = new GemBehavior[] { null, null, null, null};
        public PedestalBehavior[] placables = new PedestalBehavior[] { null, null, null, null };

        public GemBehavior heldGem = null;

        // Start is called before the first frame update
        override protected void Start() {
            base.Start();
            spawnPoint = transform.position;
            mazeManager = FindObjectOfType<MazeManager>();
            objectKey = "Adventurer";
        }

        void Respawn() {
            moving = false;
            transform.position = spawnPoint;
        }

        // Update is called once per frame
        void Update() {
            if (moving) {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetPosition) == 0) {
                    MazeTile tile = mazeManager.VisitMazeTile(transform.position);
                    if (tile != null && tile.type == TileType.Danger) {
                        Respawn();
                    }
                    moving = false;
                }
            }
            else {
                if (movable[(int)Direction.Up] && Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                    targetPosition = transform.position + Vector3.up;
                    moving = true;
                }
                else if (movable[(int)Direction.Down] && Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                    targetPosition = transform.position + Vector3.down;
                    moving = true;
                }
                else if (movable[(int)Direction.Left] && Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                    targetPosition = transform.position + Vector3.left;
                    moving = true;
                }
                else if (movable[(int)Direction.Right] && Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                    targetPosition = transform.position + Vector3.right;
                    moving = true;
                }
            }
        }

        public override JObject InbetweenData() {
            var job = base.InbetweenData();
            job["holding"] = heldGem != null ? heldGem.ObjectKey : "None";
            return job;
        }

        public override JObject KeyFrameData() {
            var job = base.KeyFrameData();
            job["holding"] = heldGem != null ? heldGem.ObjectKey : "None";
            return job;
        }
    }
}