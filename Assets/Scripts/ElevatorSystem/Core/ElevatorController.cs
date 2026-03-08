using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KProject.ElevatorSystem
{
    public enum ElevatorState { Idle, MovingUp, MovingDown, DoorsOpen }

    public class ElevatorController : MonoBehaviour
    {
        public event Action<int> OnFloorChanged;
        public event Action<ElevatorState> OnStateChanged;
        public event Action<int, bool> OnDestinationChanged;

        [Header("Config")]
        [SerializeField] private ElevatorData _data;
        [Header("Door")]
        [SerializeField] private ElevatorDoor _door;
        [Header("Floor Setup")]
        [Tooltip("Set ini sama dengan Y position Ground floor di scene")]
        [SerializeField] private float _groundFloorWorldY = 0f;

        private int _currentFloor = 0;
        private ElevatorState _state = ElevatorState.Idle;
        private readonly SortedSet<int> _upQueue = new SortedSet<int>();
        private readonly SortedSet<int> _downQueue = new SortedSet<int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));

        public int CurrentFloor => _currentFloor;
        public ElevatorState State => _state;
        public bool IsAvailable => _state == ElevatorState.Idle;
        public ElevatorData Data => _data;

        private void Awake()
        {
            if (_door == null) _door = GetComponentInChildren<ElevatorDoor>();
        }

        private void Start()
        {
            SnapToFloor(_currentFloor);
        }

        public void RequestFloor(int floor)
        {
            if (floor == _currentFloor && _state == ElevatorState.Idle)
            {
                StartCoroutine(OpenDoorsAtCurrentFloorRoutine(floor));
                return;
            }
            if (floor > _currentFloor)
                _upQueue.Add(floor);
            else
                _downQueue.Add(floor);
            OnDestinationChanged?.Invoke(floor, true);
            if (_state == ElevatorState.Idle)
                StartCoroutine(ProcessQueueRoutine());
        }

        private IEnumerator OpenDoorsAtCurrentFloorRoutine(int floor)
        {
            yield return OpenDoorsRoutine();
            ElevatorManager.Instance?.OnElevatorArrivedAtFloor(this, floor);
            SetState(ElevatorState.Idle);
        }

        private IEnumerator ProcessQueueRoutine()
        {
            while (_upQueue.Count > 0 || _downQueue.Count > 0)
            {
                int target = PickNextTarget();
                yield return MoveToFloorRoutine(target);
                yield return OpenDoorsRoutine();
            }
            SetState(ElevatorState.Idle);
        }

        private int PickNextTarget()
        {
            if (_state == ElevatorState.MovingDown || _state == ElevatorState.DoorsOpen)
            {
                if (_downQueue.Count > 0) return _downQueue.Min;
                return _upQueue.Min;
            }
            if (_upQueue.Count > 0) return _upQueue.Min;
            return _downQueue.Min;
        }

        private IEnumerator MoveToFloorRoutine(int targetFloor)
        {
            SetState(targetFloor > _currentFloor ? ElevatorState.MovingUp : ElevatorState.MovingDown);
            Vector3 startPos = transform.position;
            Vector3 targetPos = FloorToWorldPosition(targetFloor);
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / _data.MoveSpeed;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }
            transform.position = targetPos;
            _upQueue.Remove(targetFloor);
            _downQueue.Remove(targetFloor);
            OnDestinationChanged?.Invoke(targetFloor, false);
            int previousFloor = _currentFloor;
            _currentFloor = targetFloor;
            if (_currentFloor != previousFloor)
                OnFloorChanged?.Invoke(_currentFloor);
            ElevatorManager.Instance?.OnElevatorArrivedAtFloor(this, _currentFloor);
        }

        private IEnumerator OpenDoorsRoutine()
        {
            SetState(ElevatorState.DoorsOpen);
            if (_door != null)
                yield return _door.OpenAndClose(_data.DoorOpenDuration);
            else
                yield return new WaitForSeconds(_data.DoorOpenDuration);
        }

        private void SetState(ElevatorState newState)
        {
            _state = newState;
            OnStateChanged?.Invoke(_state);
        }

        private Vector3 FloorToWorldPosition(int floor)
        {
            Vector3 pos = transform.position;
            pos.y = _groundFloorWorldY + (floor * _data.FloorHeight);
            return pos;
        }

        private void SnapToFloor(int floor)
        {
            Vector3 pos = transform.position;
            pos.y = _groundFloorWorldY + (floor * _data.FloorHeight);
            transform.position = pos;
        }
    }
}
