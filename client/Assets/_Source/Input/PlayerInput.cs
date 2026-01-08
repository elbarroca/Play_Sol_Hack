using UnityEngine;
using UnityEngine.InputSystem;

namespace PlaceholderHack.Input
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction interactAction;

        public Vector2 MoveInput { get; private set; }
        public bool InteractPressed { get; private set; }

        private void Awake()
        {
            if (inputActions != null)
            {
                moveAction = inputActions.FindAction("Move");
                interactAction = inputActions.FindAction("Interact");

                moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
                moveAction.canceled += ctx => MoveInput = Vector2.zero;

                interactAction.performed += ctx => InteractPressed = true;
                interactAction.canceled += ctx => InteractPressed = false;
            }
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            interactAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            interactAction?.Disable();
        }

        private void Start()
        {
            Debug.Log("PlayerInput initialized - ready for blockchain gaming!");
        }

        private void LateUpdate()
        {
            // Reset input flags that should only be true for one frame
            if (InteractPressed)
            {
                InteractPressed = false;
            }
        }
    }
}