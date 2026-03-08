using UnityEngine;

namespace KProject.ElevatorSystem
{
    [CreateAssetMenu(fileName = "ElevatorData", menuName = "KProject/Elevator System/Elevator Data")]
    public class ElevatorData : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 3f;
        [Header("Doors")]
        [SerializeField] private float _doorOpenDuration = 2f;
        [Header("Floors")]
        [SerializeField] private float _floorHeight = 3f;
        [SerializeField] private string[] _floorNames = { "G", "1", "2", "3" };

        public float MoveSpeed => _moveSpeed;
        public float DoorOpenDuration => _doorOpenDuration;
        public float FloorHeight => _floorHeight;
        public string[] FloorNames => _floorNames;
        public int FloorCount => _floorNames.Length;

        public string GetFloorName(int floorIndex)
        {
            if (floorIndex < 0 || floorIndex >= _floorNames.Length) return floorIndex.ToString();
            return _floorNames[floorIndex];
        }
    }
}
