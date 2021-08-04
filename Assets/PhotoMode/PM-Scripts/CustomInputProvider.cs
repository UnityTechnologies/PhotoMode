using UnityEngine;
using UnityEngine.InputSystem;
using static Cinemachine.AxisState;
using PhotoMode;

namespace PhotoMode
{

    public class CustomInputProvider : MonoBehaviour, IInputAxisProvider
    {
        [SerializeField] private InputActionReference XYAxisAction;

        private Vector2 XYAxis;
        public bool active = true;

        private void Awake()
        {
            XYAxisAction.action.performed += Action_performed;
        }

        private void Action_performed(InputAction.CallbackContext callback)
        {
            XYAxis = callback.ReadValue<Vector2>();

            if (active)
            {
                GetAxisValue(0);
                GetAxisValue(1);
            }
        }

        public float GetAxisValue(int axis)
        {
            if (!active)
                return 0;

            switch (axis)
            {
                default: return 0;
                case 0: return XYAxis.x;
                case 1: return XYAxis.y;
            }
        }


    }
}
