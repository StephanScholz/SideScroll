using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller : MonoBehaviour
{
    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Acceleration while in the air.")]
    float airAcceleration = 30;

    [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
    float jumpHeight = 4;

    [SerializeField, Tooltip("Max speed, in units per second, that the character slides")]
    float slideSpeed = 12;

    [SerializeField, Tooltip("Acceleration while sliding")]
    float slideAcceleration = 800;

    [SerializeField, Tooltip("Deceleration when sliding stops")]
    float slideDeceleration = 800;

    private BoxCollider2D boxCollider;

    private Vector2 velocity;

    private bool grounded;

    private bool enableDoubleJump;

    private bool enableSlide;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        //Sliding
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PerformSlide(moveInput, slideSpeed);
            enableSlide = true;
        }

        //Jumping
        if (grounded)
        {
            velocity.y = 0;

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
                enableDoubleJump = true; 
            }

            
        }
        else if (enableDoubleJump)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
                enableDoubleJump = false; 
            }
        }
        
        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = grounded ? groundDeceleration : 0;

        if (moveInput != 0)
        {
            if (!enableSlide)
                velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
        }
        else
        {
            if (!enableSlide)
                velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
        }

        velocity.y += Physics2D.gravity.y * Time.deltaTime;

        transform.Translate(velocity * Time.deltaTime);

        //Collisions
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        grounded = false;
        //Collision Resolution. Pushing Player out of offending collider to no longer detect collision
        foreach(Collider2D hit in hits)
        {
            if (hit == boxCollider)
            {
                continue;
            }
            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);
            
            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                if(Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y <= 0)
                {
                    grounded = true;
                }
            }
        }
    }

    private void PerformSlide(float moveInput, float slideSpeed)
    {

        velocity.x = Mathf.MoveTowards(velocity.x, slideSpeed * moveInput, slideAcceleration * Time.deltaTime);
        StartCoroutine("StopSlide");
    }

    private void Jump()
    {
        // Calculate the velocity required to achieve the target jump height.
        velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
    }

    IEnumerator StopSlide()
    {
        yield return new WaitForSeconds(0.5f);
        velocity.x = Mathf.MoveTowards(velocity.x, 0, slideDeceleration * Time.deltaTime);
        enableSlide = false;
    }
}
