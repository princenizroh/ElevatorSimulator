using UnityEngine;

namespace KProject.ElevatorSystem
{
    public class ElevatorInteriorZone : MonoBehaviour
    {
        private ElevatorDoor _door;

        private void Awake()
        {
            _door = GetComponentInParent<ElevatorDoor>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _door?.OnPlayerEnter(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _door?.OnPlayerExit(other);
        }
    }
}
