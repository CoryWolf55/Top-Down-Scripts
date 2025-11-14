/* Copyright by: Cory Wolf */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    private Camera mainCamera;
    private Rigidbody rb;
    private void Awake() 
    { 
        instance = this; 
        rb = GetComponent<Rigidbody>();
    }

    [Header("Movement")]
    [SerializeField]
    private float movementSpeed;
    private float moveSpeedDefault;
    [SerializeField]
    private float maxSpeed;
    private float defaultMaxSpeed;

    [Header("Sprinting")]
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    public float maxStamina;
    private float currentStamina;
    private bool canSprint = true;
    [HideInInspector]
    public Vector3 point;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
        private float currentHealth;
    private HealthBarManager healthBar;




    [Header("Inputs")]
    [SerializeField]
    private KeyCode forwardKey = KeyCode.W;
    [SerializeField]
    private KeyCode altForwardKey = KeyCode.UpArrow;

    [SerializeField]
    private KeyCode backKey = KeyCode.S;
    [SerializeField]
    private KeyCode altBackKey = KeyCode.DownArrow;

    [SerializeField]
    private KeyCode leftKey = KeyCode.A;
    [SerializeField]
    private KeyCode altLeftKey = KeyCode.LeftArrow;

    [SerializeField]
    private KeyCode rightKey = KeyCode.D;
    [SerializeField]
    private KeyCode altRightKey = KeyCode.RightArrow;



    private void Start()
    {
        mainCamera = Camera.main;
        moveSpeedDefault = movementSpeed;
        currentStamina = maxStamina;
        defaultMaxSpeed = maxSpeed;

        //Get Health
        healthBar = GetComponentInChildren<HealthBarManager>();
        healthBar.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;

    }




    private void FixedUpdate()
    {
        HandleRotationinput();
        Movement();
    }


     void HandleRotationinput()
    {
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
             Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            Debug.DrawLine(cameraRay.origin, pointToLook, Color.blue);

            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
            point = pointToLook;
        }
    }


    private void Movement()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Sprinting();
            maxSpeed = sprintSpeed;
        }
        else
            maxSpeed = defaultMaxSpeed;

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);


        if (Input.GetKey(forwardKey) || Input.GetKey(altForwardKey))
        {
            rb.AddForce(0, 0, movementSpeed * Time.deltaTime, ForceMode.VelocityChange);

        }
        else if (Input.GetKey(backKey) || Input.GetKey(altBackKey))
        {
            rb.AddForce(0, 0, -movementSpeed * Time.deltaTime, ForceMode.VelocityChange);

        }
        if (Input.GetKey(leftKey) || Input.GetKey(altLeftKey))
        {
            rb.AddForce(-movementSpeed * Time.deltaTime, 0, 0, ForceMode.VelocityChange);

        }
        else if (Input.GetKey(rightKey) || Input.GetKey(altRightKey))
        {
            rb.AddForce(movementSpeed * Time.deltaTime, 0, 0, ForceMode.VelocityChange);

        }

        
    }

 
    private void Sprinting()
    {

        movementSpeed = sprintSpeed;
        
        
    }

    public void TakeDamage(float damage)
    {
        
        currentHealth -= damage;
        if (currentHealth <= 0) 
        {
            currentHealth = 0;
            Debug.Log("Player Died");
        }

        healthBar.SetHealth(currentHealth);

    }

    

}