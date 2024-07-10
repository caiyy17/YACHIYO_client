using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Rotatable : MonoBehaviour 
{
	[SerializeField] private InputAction pressed, axis, point;
	
	private Transform cam;
	[SerializeField] private float speed = 1;
	[SerializeField] private bool inverted;
	private Vector2 rotation;
	private bool rotateAllowed;
	private void Awake() 
    {
        cam = Camera.main.transform;
        pressed.Enable();
        axis.Enable();
        point.Enable();
        pressed.performed += _ => CheckAndStartRotate();
        pressed.canceled += _ => { rotateAllowed = false; };
        axis.performed += context => { rotation = context.ReadValue<Vector2>(); };
        axis.canceled += context => { rotation = Vector2.zero; };
    }

    private void CheckAndStartRotate()
    {
        if (IsPointerOverCollider())
        {
            StartCoroutine(Rotate());
        }
    }

    private bool IsPointerOverCollider()
    {
        Vector2 screenPosition = point.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            print(hit.collider.name);
            return hit.collider.transform == transform;
        }
        return false;
    }

    private IEnumerator Rotate()
    {
        rotateAllowed = true;
        while (rotateAllowed)
        {
            // apply rotation
            transform.Rotate(Vector3.up * (inverted ? 1 : -1) * rotation.x * speed, Space.World);
            yield return null;
        }
    }
}
