using UnityEngine;
using UnityEngine.UI;

namespace KProject.ElevatorSystem
{
    public class FloorCallButton : MonoBehaviour
    {
        [Header("Floor")]
        [SerializeField] private int _floorIndex = 0;
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
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OnButtonPressed);
            ElevatorManager.OnFloorCallChanged += HandleFloorCallChanged;
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonPressed);
            ElevatorManager.OnFloorCallChanged -= HandleFloorCallChanged;
        }

        private void OnButtonPressed()
        {
            if (ElevatorManager.Instance == null)
            {
                Debug.LogWarning("[FloorCallButton] ElevatorManager not found in scene.");
                return;
            }
            ElevatorManager.Instance.RequestElevator(_floorIndex);
        }

        private void HandleFloorCallChanged(int floor, bool isActive)
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
