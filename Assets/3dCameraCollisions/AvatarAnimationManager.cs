using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarAnimationManager : MonoBehaviour
{
  public Rigidbody rb;
  public Animator animator;
  Vector3 prevVelocity;
  public CharacterControllerVelocity characterController;
  Vector3 prevPlayerInputAxis;

  void Start()
  {
    prevVelocity = rb.velocity;
    prevPlayerInputAxis = characterController.input;
    StartCoroutine(IdleSelector());
  }

  
  void Update()
  {
    Vector3 currentVelocity = rb.velocity;

    animator.SetFloat("VerticalSpeed", currentVelocity.y);

    float blendRatio = rb.velocity.magnitude / characterController.maxHorizontalSpeed;
    animator.SetFloat("speed", blendRatio);

    animator.SetBool("OnGround", characterController.isOnGround);

    animator.SetBool("IsSliding", characterController.isSliding);

    // détecter les moments où on commence à tomber
    if(prevVelocity.y >= 0 && currentVelocity.y < 0)
    {
      animator.SetTrigger("Falling");
    }

    // gestion des idle, on lance une coroutine quand le joueur s'arrête
    if(prevPlayerInputAxis.magnitude > 0 && characterController.input.magnitude == 0)
    {
      StartCoroutine(IdleSelector());
    }

    prevVelocity = currentVelocity;
    prevPlayerInputAxis = characterController.input;
  }


  IEnumerator IdleSelector()
  {
    Debug.LogFormat("Starting idle selector coroutine");
    animator.SetFloat("IdleIndex", 0);

    float idleTime = 0;

    float currentIdle = 0;
    float targetIdle  = 0;
    float blendTime   = 0.2f;
    float blendSpeed  = 0; // how fast do we blend towards next idle ?

    while(prevVelocity.magnitude == 0)
    {
      idleTime += Time.deltaTime;

      // NOTE: should wait for current anim cycle end before launching a new one
      // plus should not use a blend tree but maybe blended layers to avoiding passing by 3 or 4 idle animations before landing on the good one (our blend parameter will blend from current anim toward next one by passing by every anims in between)
      if(idleTime > 10)
      {
        targetIdle = Random.Range(0, 5);
        idleTime = 0;
        blendSpeed = Mathf.Abs(targetIdle-currentIdle) / blendTime;
      }

      currentIdle = Mathf.MoveTowards(currentIdle, targetIdle, blendSpeed * Time.deltaTime);
      animator.SetFloat("IdleIndex", currentIdle);

      yield return null;
    }
  }

  void InitiateJump()
  {
    animator.SetTrigger("Jump");
  }
}
