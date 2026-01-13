using UnityEngine;
using PlaceholderHack.Networking;
using UnityEngine.InputSystem; // Must be present

namespace PlaceholderHack.Input
{
    public class PlayerInputBridge : MonoBehaviour
    {
        [Header("Dependencies")]
        public NetworkManager Network;
        public SessionKeyManager Session;

        [Header("Settings")]
        public float SendRate = 0.05f; // 20 times per second (50ms)
        
        private float _nextSendTime;
        private Vector2 _currentInput;

        void Update()
        {
            _currentInput = Vector2.zero;

            // 1. Priority: PSG1 Gamepad (Physical Hardware)
            if (Gamepad.current != null)
            {
                _currentInput = Gamepad.current.leftStick.ReadValue();
            }
            // 2. Fallback: Editor Keyboard (WASD) - Using NEW Input System
            else if (Keyboard.current != null)
            {
                float x = 0;
                float y = 0;
                
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1;

                _currentInput = new Vector2(x, y);
            }

            // 3. Rate Limiting (Don't flood the chain)
            if (Time.time >= _nextSendTime)
            {
                SendInput();
                _nextSendTime = Time.time + SendRate;
            }
        }

        void SendInput()
        {
            // Only send if we have a session
            if (Session == null || !Session.IsSessionValid) return;

            // Convert Float (-1.0 to 1.0) to SByte (-100 to 100)
            sbyte x = (sbyte)(_currentInput.x * 100);
            sbyte y = (sbyte)(_currentInput.y * 100);

            // Deadzone Check (Prevent drift spam)
            if (Mathf.Abs(x) < 10 && Mathf.Abs(y) < 10) return;

            // Debug log to prove it's working in Editor
            Debug.Log($"🚀 Sending Input: [{x}, {y}]");

            Network.SendMove(x, y);
        }
    }
}