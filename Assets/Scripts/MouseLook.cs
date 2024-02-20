using Cinemachine;
using UnityEngine;
using Photon.Pun;

public class MouseLook : MonoBehaviourPun
{
    public Transform playerBody;
    public Transform faceFocusPoint;
    public float mouseSensitivity = 100f;
    private bool focusOnFace = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float transitionSpeed = 5f;
    float xRotation = 0f;
    float yRotation = 0f;
    private bool isSet;
    [SerializeField] Camera cam;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }


    void Update()
    {
        if (!photonView.IsMine) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        yRotation += mouseX;
        
        if (!focusOnFace)
        {
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
             SetAnimState();
        }
    }

    private void SetAnimState()
    {
        if (!isSet)
        {
            isSet = true;
            cam.nearClipPlane = 0.5f;
            transform.position = faceFocusPoint.position;
            transform.rotation = faceFocusPoint.rotation;
            Vector3 eulerAngles = faceFocusPoint.localRotation.eulerAngles;
            //xRotation = eulerAngles.x; 
            //yRotation = eulerAngles.y;
        }
        else
        {
           // transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }

    public void ToggleFaceFocus(bool focus)
    {
        focusOnFace = focus;
        if (!focus)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            cam.nearClipPlane = 0.1f;
            isSet = false;
        }
    }
}