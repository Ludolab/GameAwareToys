using GameAware;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InvisibleMaze {

    public class Adventurer : MetaDataTrackable {
        public float speed;
        private Vector3 targetPosition;
        private Vector3 lastPosition;
        private Vector2 spawnPoint;
        public bool moving = false;
        MazeManager mazeManager;
        [HideInInspector]
        public bool[] movable = new bool[] { true, true, true, true };
        [HideInInspector]
        public GemBehavior[] pickables = new GemBehavior[] { null, null, null, null, null};
        [HideInInspector]
        public PedestalBehavior[] placables = new PedestalBehavior[] { null, null, null, null, null };

        public GemBehavior heldGem = null;
        private Direction lastDirection = Direction.Up;

        // Start is called before the first frame update
        override protected void Start() {
            base.Start();
            spawnPoint = transform.position;
            mazeManager = FindObjectOfType<MazeManager>();
            objectKey = "Adventurer";
            pickables = new GemBehavior[] { null, null, null, null, null };
            placables = new PedestalBehavior[] { null, null, null, null, null };
        }

        void Respawn() {
            if(heldGem != null) {
                heldGem.transform.parent = null;
                heldGem.transform.position = lastPosition;
                heldGem.Placed();
                heldGem = null;
            }
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
                if (Input.GetKeyDown(KeyCode.E)) {
                    
                    if(heldGem != null) {
                        //if we're holding something and there is a placable target put it there
                        bool placed = false;
                        for(int i = 0; i < placables.Length; i++) {
                            if (placables[i] != null) {
                                placables[i].SlotGem(heldGem);
                                heldGem = null;
                                placed = true;
                                break;
                            }
                        }
                        if (!placed) {
                            //drop the gem where we're standing
                            heldGem.transform.parent = transform.parent;
                            heldGem.transform.position = transform.position;
                            heldGem.Placed();
                            heldGem = null;
                        }
                    }
                    else {
                        if (pickables[(int)lastDirection] != null) {
                            PickupGem(pickables[(int)lastDirection]);
                            pickables[(int)lastDirection] = null;
                        }
                        else {
                            for(int i = 0; i < pickables.Length; i++) {
                                if (pickables[i] != null) {
                                    PickupGem(pickables[i]);
                                    pickables[i] = null;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (movable[(int)Direction.Up] && Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                    lastPosition = transform.position;
                    targetPosition = transform.position + Vector3.up;
                    lastDirection = Direction.Up;
                    moving = true;
                }
                else if (movable[(int)Direction.Down] && Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                    targetPosition = transform.position + Vector3.down;
                    lastPosition = transform.position;
                    lastDirection = Direction.Down;
                    moving = true;
                }
                else if (movable[(int)Direction.Left] && Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
                    targetPosition = transform.position + Vector3.left;
                    lastPosition = transform.position;
                    lastDirection = Direction.Left;
                    moving = true;
                }
                else if (movable[(int)Direction.Right] && Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                    targetPosition = transform.position + Vector3.right;
                    lastPosition = transform.position;
                    lastDirection = Direction.Right;
                    moving = true;
                }
            }
        }

        void PickupGem(GemBehavior gem) {
            heldGem = gem;
            gem.PickUp();
            gem.transform.position = transform.position;
            gem.transform.parent = transform;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Gem")) {
                pickables[(int)Direction.Center] = other.gameObject.GetComponent<GemBehavior>();
            }
            if (other.gameObject.CompareTag("Pedestal")) {
                placables[(int)Direction.Center] = other.gameObject.GetComponent<PedestalBehavior>();
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.gameObject.CompareTag("Gem")) {
                pickables[(int)Direction.Center] = null;
            }
            if (collision.gameObject.CompareTag("Pedestal")) {
                placables[(int)Direction.Center] = null;
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