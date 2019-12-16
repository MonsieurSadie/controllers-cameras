using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MvtInAvatarReferential : MonoBehaviour
{
  public float maxSpeed = 10; // m/s
  public float accel = 1; // m/s
  Rigidbody rb;
  void Start()
  {
    rb = gameObject.GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    float inputX = Input.GetAxis("Horizontal");
    float inputY = Input.GetAxis("Vertical");
    Vector3 mvtDirection = inputX * transform.right + inputY * transform.forward;
    rb.AddForce(mvtDirection * accel);
    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
  }
}
