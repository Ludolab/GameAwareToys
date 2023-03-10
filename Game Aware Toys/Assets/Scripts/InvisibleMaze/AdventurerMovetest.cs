using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InvisibleMaze {
    [RequireComponent(typeof(CircleCollider2D))]
    public class AdventurerMovetest : MonoBehaviour {

        Adventurer adventurer;
        public Direction direction;

        // Start is called before the first frame update
        void Start() {
            adventurer = GetComponentInParent<Adventurer>();
        }


        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Wall")) {
                adventurer.movable[(int)direction] = false;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Wall")) {
                adventurer.movable[(int)direction] = true;
            }
        }

        // Update is called once per frame
        void Update() {

        }
    }
}