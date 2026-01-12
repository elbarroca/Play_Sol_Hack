using UnityEngine;
using PlaceholderHack.Networking;

namespace PlaceholderHack.Games.Tanks
{
    public class NetworkTank : MonoBehaviour
    {
        public bool IsLocalPlayer;
        private MagicBlockClient _network;

        // Visuals
        [SerializeField] private AudioSource _movementAudio;
        [SerializeField] private AudioClip _idleClip;
        [SerializeField] private AudioClip _drivingClip;
        
        // Smoothing
        private Vector3 _targetPos;
        private Quaternion _targetRot;

        void Start()
        {
            _network = FindFirstObjectByType<MagicBlockClient>();
        }

        void Update()
        {
            if (IsLocalPlayer)
            {
                // INPUT: Read Left Stick (Y) for Drive, (X) for Turn
                // Send to MagicBlock
            }
            else
            {
                // REMOTE: Interpolate
                transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, Time.deltaTime * 10f);
            }

            // Audio Logic (Simple)
            if (Vector3.Distance(transform.position, _targetPos) > 0.1f)
            {
                if(_movementAudio.clip != _drivingClip) { _movementAudio.clip = _drivingClip; _movementAudio.Play(); }
            }
            else
            {
                if(_movementAudio.clip != _idleClip) { _movementAudio.clip = _idleClip; _movementAudio.Play(); }
            }
        }

        public void OnServerUpdate(long x, long z, long rot)
        {
            _targetPos = new Vector3(x / 100f, 0, z / 100f);
            _targetRot = Quaternion.Euler(0, rot / 100f, 0);
        }
    }
}