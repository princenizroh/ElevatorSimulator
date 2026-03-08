using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KProject.ElevatorSystem
{
    public class ElevatorDisplay : MonoBehaviour
    {
        [Header("Target Elevator")]
        [SerializeField] private ElevatorController _elevator;
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _floorText;
        [SerializeField] private TextMeshProUGUI _directionText;

        private void OnEnable()
        {
            if (_elevator == null) return;
            _elevator.OnFloorChanged += HandleFloorChanged;
            _elevator.OnStateChanged += HandleStateChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (_elevator == null) return;
            _elevator.OnFloorChanged -= HandleFloorChanged;
            _elevator.OnStateChanged -= HandleStateChanged;
        }

        private void HandleFloorChanged(int floor) => UpdateFloorText(floor);

        private void HandleStateChanged(ElevatorState state)
        {
            UpdateDirectionText(state);
        }

        private void Refresh()
        {
            UpdateFloorText(_elevator.CurrentFloor);
            UpdateDirectionText(_elevator.State);
        }

        private void UpdateFloorText(int floor)
        {
            if (_floorText == null) return;
            _floorText.text = _elevator.Data != null ? _elevator.Data.GetFloorName(floor) : floor.ToString();
        }

        private void UpdateDirectionText(ElevatorState state)
        {
            if (_directionText == null) return;
            _directionText.text = state switch
            {
                ElevatorState.MovingUp => "UP",
                ElevatorState.MovingDown => "DN",
                ElevatorState.DoorsOpen => "OPEN",
                _ => "-"
            };
        }


    }
}
