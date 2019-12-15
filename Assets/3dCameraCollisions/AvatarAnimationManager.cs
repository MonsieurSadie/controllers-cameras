using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarAnimationManager : MonoBehaviour
{
  public Rigidbody rb;
  public Animator animator;
  Vector3 prevVelocity;
  public CharacterControllerVelocity characterController;

  void Start()
  {
    prevVelocity = rb.velocity;
  }

  
  void Update()
  {
    Vector3 currentVelocity = rb.velocity;

    animator.SetFloat("VerticalSpeed", currentVelocity.y);

    float blendRatio = rb.velocity.magnitude / characterController.maxHorizontalSpeed;
    animator.SetFloat("speed", blendRatio);

    animator.SetBool("OnGround", characterController.isOnGround);

    // détecter les moments où on commence à tomber
    if(prevVelocity.y >= 0 && currentVelocity.y < 0)
    {
      animator.SetTrigger("Falling");
    }

    prevVelocity = currentVelocity;
  }

  void InitiateJump()
  {
    animator.SetTrigger("Jump");
  }
}
