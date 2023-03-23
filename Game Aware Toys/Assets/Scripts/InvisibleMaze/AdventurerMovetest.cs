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
            if (other.gameObject.CompareTag("Gem")) {
                adventurer.pickables[(int)direction] = other.gameObject.GetComponent<GemBehavior>();
            }
            if(other.gameObject.CompareTag("Pedestal")) {
                adventurer.placables[(int)direction] = other.gameObject.GetComponent<PedestalBehavior>();
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Wall")) {
                adventurer.movable[(int)direction] = true;
            }
            if (other.gameObject.CompareTag("Gem")) {
                adventurer.pickables[(int)direction] = null;
            }
            if (other.gameObject.CompareTag("Pedestal")) {
                adventurer.placables[(int)direction] = null;
            }
        }
    }
}