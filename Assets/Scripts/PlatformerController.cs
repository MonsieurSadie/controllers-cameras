using System;
using UnityEngine;

public class PlatformerController : MonoBehaviour
{
  public enum MovementConstraint
  {
    MOVEMENT_2D,
    MOVEMENT_3D
  };
  public MovementConstraint constraint = MovementConstraint.MOVEMENT_2D;
  public float accel = 10;
  public float maxHorizontalSpeed = 10;
  public float maxVerticalSpeed   = 10;
  public float jumpForce = 10;
  Rigidbody rb;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }


  void Update()
  {
    float hInput = Input.GetAxis("Horizontal");
    float vInput = Input.GetAxis("Vertical");
    if(constraint == MovementConstraint.MOVEMENT_2D)
    {
      rb.AddForce(hInput * transform.forward * accel);
    }else if(constraint == MovementConstraint.MOVEMENT_3D)
    {
      rb.AddForce(vInput * transform.forward * accel);
      rb.AddForce(hInput * transform.right * accel);
    }

    if(Input.GetButtonDown("Jump"))
    {
      rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    // clamp velocity. Beware, max speed is not the same in h and v directions
    Vector3 vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    vel = Vector3.ClampMagnitude(vel, maxHorizontalSpeed);
    vel.y = Mathf.Sign(rb.velocity.y) * Mathf.Min( Mathf.Abs(rb.velocity.y), maxVerticalSpeed);
    rb.velocity = vel;
  }
}