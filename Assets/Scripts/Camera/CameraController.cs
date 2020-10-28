using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    public Transform cameraTransform;
    public Transform followTransform;

    public float normalSpeed;
    public float fastSpeed;
    public float movementTime;
    public float rotationAmount;
    public Vector3 zoomAmount;
    public float minZoom;
    public float maxZoom;

    [SerializeField] private bool bindToPlayer = false;

    float zoomLevels = 3f;

    float movementSpeed;
    float currentZoomLevel;
    Vector3 newPosition;
    Quaternion newRotation;
    Vector3 newZoom;

    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;
    Vector3 rotateStartPosition;
    Vector3 rotateCurrentPosition;

    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;

        if (bindToPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null) return;
            
            followTransform = player.transform;
        }
    }

    // Update is called once per frame
    void Update()
    { 
        HandleMouseInput();
        HandleRotation();
        HandleZoom();
        
        if (followTransform != null)
        {
            transform.position = followTransform.position;
        } else
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTransform = null;
        }
    }

    void CalculateZoomLevel()
    {
        float heightMultiples = Mathf.Floor(maxZoom / zoomLevels);

        if (newZoom.y > heightMultiples * (zoomLevels - 1))
        {
            currentZoomLevel = 1f;
        }
        else if (newZoom.y < heightMultiples * (zoomLevels - 1) && newZoom.y > heightMultiples * (zoomLevels - 2))
        {
            currentZoomLevel = 2f;
        }
        else if (newZoom.y < heightMultiples * (zoomLevels - 2))
        {
            currentZoomLevel = 3f;
        }
    }

    void HandleMouseInput()
    {
        //Scrollwheel zoom
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }

        //Zoom clamping
        newZoom.y = Mathf.Clamp(newZoom.y, minZoom, maxZoom);
        newZoom.z = Mathf.Clamp(newZoom.z, -maxZoom, -minZoom); //min and max need to be flipped here because the numbers are negative, so max is technically lower than the min
        CalculateZoomLevel();

        //Mouse rotation
        #region Mouse rotation
        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;

            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
        #endregion Mouse rotation

        //Mouse drag camera movement (disabled)
        #region Mouse drag camera movement (disabled)
       /* if (Input.GetMouseButtonDown(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + (dragStartPosition - dragCurrentPosition);
            }
        }*/
        #endregion Mouse drag camera movement
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = fastSpeed / currentZoomLevel;
        }
        else
        {
            movementSpeed = normalSpeed / currentZoomLevel;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition -= (transform.forward * movementSpeed);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition -= (transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }

        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
    }

    void HandleRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
    }

    void HandleZoom()
    {
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }
}
