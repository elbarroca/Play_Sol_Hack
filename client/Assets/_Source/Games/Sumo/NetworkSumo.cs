using UnityEngine;
using PlaceholderHack.Networking;

namespace PlaceholderHack.Games.Sumo
{
    public class NetworkSumo : MonoBehaviour
    {
        [Header("Identity")]
        public bool IsPlayerOne;   // Checked in Editor for the Blue/Red Ball

        [Header("Smoothing")]
        private Vector3 _targetPos;

        // This receives the RAW integer data from GameState.cs (NetworkManager)
        public void OnServerUpdate(long x, long y)
        {
            // SCALE FACTOR: 100
            // Solana: 255 -> Unity: 2.55f
            float unityX = x / 100.0f;
            float unityZ = y / 100.0f; // Sumo uses X/Z plane (Y is up/height)

            UpdateTargetPosition(unityX, unityZ);
        }

        // This receives the scaled float data from MagicBlockClient
        public void UpdateTargetPosition(float x, float z)
        {
            _targetPos = new Vector3(x, 0.5f, z); // 0.5f is ball radius height
        }

        void Update()
        {
            // Anti-Jitter Logic: Snap for large distances, smooth for small ones
            float distance = Vector3.Distance(transform.position, _targetPos);

            if (distance > 2.0f)
            {
                // Large distance - snap immediately (teleport)
                transform.position = _targetPos;
            }
            else
            {
                // Small distance - smooth interpolation
                transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * 20f);
            }
        }
    }
}