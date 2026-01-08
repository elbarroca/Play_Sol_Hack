using UnityEngine;
using PlaceholderHack.Networking;

namespace PlaceholderHack.Games.Sumo
{
    public class NetworkSumo : MonoBehaviour
    {
        [Header("Identity")]
        public bool IsLocalPlayer; // Set this if it is YOUR ball
        public bool IsPlayerOne;   // Used to decode state

        [Header("Visuals")]
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Color _p1Color = Color.blue;
        [SerializeField] private Color _p2Color = Color.red;

        [Header("Smoothing")]
        private Vector3 _targetPos;
        
        // Reference to the Network Manager
        private MagicBlockClient _network;

        void Start()
        {
            _network = FindObjectOfType<MagicBlockClient>();
            
            // Set Color based on ID
            if (_renderer) _renderer.material.color = IsPlayerOne ? _p1Color : _p2Color;
        }

        void Update()
        {
            if (IsLocalPlayer)
            {
                // 1. INPUT: Send Joystick Data to MagicBlock
                // We do NOT move the ball here. The server moves it.
                // (Input logic will be hooked up to PlayerInput.cs later)
            }
            else
            {
                // 2. REMOTE: Interpolate to the server position
                transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * 20f);
            }
        }

        // Called by MagicBlockClient when a new packet arrives
        public void OnServerUpdate(long x, long y)
        {
            // The Sumo game is 2D physics on a 3D plane (X/Z)
            // We divide by 100 because the blockchain uses Integers (Fixed Point)
            _targetPos = new Vector3(x / 100f, 0.5f, y / 100f);
        }
    }
}