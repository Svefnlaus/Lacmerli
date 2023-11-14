using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Vector3 displacement;
    private Animator animator;
    private Vector3 lastPosition;

    private Vector3 mousePosition
    {
        get
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return mousePosition;
        }
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // animate on click
        animator.SetBool("IsClicking", Input.GetMouseButton(0));

        // prevent from updating when in idle
        if (mousePosition == lastPosition) return;
        lastPosition = mousePosition;

        // hide default cursor and keep the current cursor confined
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        // add some offset to match the position of the default cursor
        float z = transform.position.z;
        Vector3 targetPosition = mousePosition + displacement;
        targetPosition.z = z;
        transform.position = targetPosition;
    }

}
