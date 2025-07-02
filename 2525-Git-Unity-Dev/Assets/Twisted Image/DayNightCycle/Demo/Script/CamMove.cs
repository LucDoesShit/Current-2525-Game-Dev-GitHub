using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CamMove
{
    public class CamMove : MonoBehaviour
    {
        public bool showCursor = false;
        public float lookSpeed = 0.1f;
        float mousetilt;
        float mouserotation;

        void Start()
        {
            mousetilt = transform.rotation.eulerAngles.y;
            mouserotation = transform.rotation.eulerAngles.x;
            Cursor.visible = showCursor;
            Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        }

        void Update()
        {
            Vector2 mouseDelta = GetMouseDelta();

            mousetilt = (mousetilt + lookSpeed * mouseDelta.x) % 360f;
            mouserotation = (mouserotation - lookSpeed * mouseDelta.y) % 360f;
            transform.rotation = Quaternion.AngleAxis(mousetilt, Vector3.up) * Quaternion.AngleAxis(mouserotation, Vector3.right);
        }

        Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.delta.ReadValue();
            }
            else
            {
                return Vector2.zero;
            }
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }
    }
}
