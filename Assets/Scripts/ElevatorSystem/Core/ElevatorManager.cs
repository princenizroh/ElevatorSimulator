using System;
using System.Collections.Generic;
using UnityEngine;
using KProject.Core.Patterns;

namespace KProject.ElevatorSystem
{
    public class ElevatorManager : Singleton<ElevatorManager>
    {
        public static event Action<int, bool> OnFloorCallChanged;

        [Header("Elevators")]
        [SerializeField] private List<ElevatorController> _elevators = new List<ElevatorController>();

        private readonly HashSet<int> _pendingCalls = new HashSet<int>();

        public void RequestElevator(int floor)
        {
            if (_pendingCalls.Contains(floor)) return;
            _pendingCalls.Add(floor);
            OnFloorCallChanged?.Invoke(floor, true);
            ElevatorController best = FindBestElevator(floor);
            if (best != null)
                best.RequestFloor(floor);
        }

        public void OnElevatorArrivedAtFloor(ElevatorController elevator, int floor)
        {
            if (_pendingCalls.Remove(floor))
                OnFloorCallChanged?.Invoke(floor, false);
        }

        private ElevatorController FindBestElevator(int targetFloor)
        {
            ElevatorController best = null;
            int bestScore = int.MaxValue;
            foreach (ElevatorController elevator in _elevators)
            {
                int score = CalculateScore(elevator, targetFloor);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = elevator;
                }
            }
            return best;
        }

        private int CalculateScore(ElevatorController elevator, int targetFloor)
        {
            int distance = Mathf.Abs(elevator.CurrentFloor - targetFloor);
            switch (elevator.State)
            {
                case ElevatorState.Idle:
                    return distance;
                case ElevatorState.MovingUp when targetFloor > elevator.CurrentFloor:
                    return distance;
                case ElevatorState.MovingDown when targetFloor < elevator.CurrentFloor:
                    return distance;
                default:
                    return distance + 100;
            }
        }
    }
}
