using System.Collections;
using UnityEngine;
using Platformer.Controller;

namespace KProject.ElevatorSystem
{
    public class ElevatorDoor : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _animator;
        [Header("Animation Durations")]
        [SerializeField] private float _openAnimDuration = 0.5f;
        [SerializeField] private float _closeAnimDuration = 0.5f;
        [Header("UI")]
        [SerializeField] private GameObject _carCallPanel;

        private bool _playerInside = false;
        public bool IsPlayerInside => _playerInside;
        private PlayerController _playerController = null;
        private Rigidbody2D _playerRb = null;
        private Transform _playerTransform = null;
        private bool _playerLocked = false;
        private Vector2 _lockOffset;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_carCallPanel != null) _carCallPanel.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (!_playerLocked || _playerRb == null) return;
            _playerRb.position = (Vector2)transform.position + _lockOffset;
        }

        public void OnPlayerEnter(Collider2D other)
        {
            _playerInside = true;
            _playerTransform = other.transform;
            _playerController = other.GetComponent<PlayerController>();
            _playerRb = other.GetComponent<Rigidbody2D>();
        }

        public void OnPlayerExit(Collider2D other)
        {
            _playerInside = false;
        }

        public IEnumerator OpenAndClose(float holdDuration)
        {
            ReleasePlayer();
            _animator.Play("Door_Open");
            yield return new WaitForSeconds(_openAnimDuration);
            yield return new WaitForSeconds(holdDuration);
            if (_playerInside)
                LockPlayer();
            _animator.Play("Door_Close");
            yield return new WaitForSeconds(_closeAnimDuration);
            if (!_playerLocked)
                _animator.Play("Door_Idle");
        }

        private void LockPlayer()
        {
            if (_playerTransform == null) return;
            if (_playerRb != null)
            {
                _playerRb.linearVelocity = Vector2.zero;
                _playerRb.gravityScale = 0f;
                _playerRb.isKinematic = true;
            }
            if (_playerController != null) _playerController.PlayerActions.Disable();
            _lockOffset = _playerRb.position - (Vector2)transform.position;
            _playerLocked = true;
            if (_carCallPanel != null) _carCallPanel.SetActive(true);
        }

        private void ReleasePlayer()
        {
            _playerLocked = false;
            if (_carCallPanel != null) _carCallPanel.SetActive(false);
            if (_playerRb != null)
            {
                _playerRb.isKinematic = false;
                _playerRb.gravityScale = 1f;
            }
            if (_playerController != null) _playerController.PlayerActions.Enable();
        }

    }
}
