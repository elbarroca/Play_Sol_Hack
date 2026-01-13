using UnityEngine;
using UnityEngine.InputSystem;
using PlaceholderHack.Networking;

namespace PlaceholderHack.Input
{
    public class PlayerInput : MonoBehaviour
    {
        // Reference to our Interface (could be Mock, could be Blockchain)
        private IGameStateProvider _network;

        [Header("Settings")]
        [SerializeField] private float _inputThreshold = 0.1f;

        void Awake()
        {
            // For now, grab the Mock. Later, this grabs MagicBlockClient
            _network = GetComponent<IGameStateProvider>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (_network == null) return;

            Vector2 input = context.ReadValue<Vector2>();

            // 1. Deadzone Check
            if (input.magnitude < _inputThreshold)
            {
                _network.SendInput(0, 0);
                return;
            }

            // 2. Quantize Data (Float -> SByte)
            // We multiply by 10 to get a decent range (-10 to 10) for the engine
            sbyte xByte = (sbyte)Mathf.RoundToInt(input.x * 10);
            sbyte yByte = (sbyte)Mathf.RoundToInt(input.y * 10);

            // 3. Send to "Network"
            _network.SendInput(xByte, yByte);
        }
    }
}