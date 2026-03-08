using UnityEngine;
using UnityEngine.UI;

namespace KProject.ElevatorSystem
{
    public class ElevatorCarCallButton : MonoBehaviour
    {
        [Header("Floor")]
        [SerializeField] private int _floorIndex = 0;
        private ElevatorController _elevator;
        private ElevatorDoor _door;
        [Header("Button Visuals")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _idleColor = Color.white;
        [SerializeField] private Color _activeColor = new Color(1f, 0.8f, 0f);

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_buttonImage == null && _button != null)
                _buttonImage = _button.GetComponent<Image>();
            if (_elevator == null)
                _elevator = GetComponentInParent<ElevatorController>();
            if (_door == null)
                _door = GetComponentInParent<ElevatorDoor>();
        }

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(OnButtonPressed);
            if (_elevator != null) _elevator.OnDestinationChanged += HandleDestinationChanged;
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveListener(OnButtonPressed);
            if (_elevator != null) _elevator.OnDestinationChanged -= HandleDestinationChanged;
        }

        private void OnButtonPressed()
        {
            if (_elevator == null)
            {
                Debug.LogWarning("[ElevatorCarCallButton] No ElevatorController assigned.");
                return;
            }
            if (_door != null && !_door.IsPlayerInside) return;
            _elevator.RequestFloor(_floorIndex);
        }

        private void HandleDestinationChanged(int floor, bool isActive)
        {
            if (floor != _floorIndex) return;
            SetLit(isActive);
        }

        private void SetLit(bool lit)
        {
            if (_buttonImage == null) return;
            _buttonImage.color = lit ? _activeColor : _idleColor;
        }
    }
}
