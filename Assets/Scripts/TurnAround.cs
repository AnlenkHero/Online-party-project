using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TurnAround : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 720f;
    private float _currentRotationSpeed;
    private float _currentLerpTime;
    private  float _lerpTime = 2f;
    private bool _isDecelerating;

    private void Update()
    {
        bool isRotating = false;

        if (Input.GetMouseButton(0))
        {
            isRotating = true;
            _isDecelerating = false;
            _currentLerpTime = 0;
            RotateModel();
        }
        else if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
        {
            isRotating = true;
            _isDecelerating = false;
            _currentLerpTime = 0;
            RotateModelWithWheel();
        }

        if (!isRotating && !_isDecelerating && _currentRotationSpeed != 0)
        {
            _isDecelerating = true;
        }

        if (_isDecelerating)
        {
            ApplyInertia();
        }
    }

    private void RotateModel()
    {
        var mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        _currentRotationSpeed = mouseX;

        transform.Rotate(Vector3.up * mouseX);
    }

    private void RotateModelWithWheel()
    {
        var mouseWheel = Input.GetAxis("Mouse ScrollWheel") * rotationSpeed/2 * Time.deltaTime;
        _currentRotationSpeed += mouseWheel;

        transform.Rotate(Vector3.up * mouseWheel);
    }

    private void ApplyInertia()
    {
        if (!(_currentLerpTime < _lerpTime)) return;
        
        _currentLerpTime += Time.deltaTime;

        float t = _currentLerpTime / _lerpTime;

            
        float deceleratedSpeed = Mathf.Lerp(_currentRotationSpeed, 0, t);
        transform.Rotate(Vector3.up * deceleratedSpeed);

        if (!(_currentLerpTime >= _lerpTime)) return;
            
        _isDecelerating = false;
        _currentRotationSpeed = 0;
    }
}
