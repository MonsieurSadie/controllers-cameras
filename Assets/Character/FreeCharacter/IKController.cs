using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
  public Transform rightHandTarget;
  Animator animator;

  void Start()
  {
    animator = GetComponent<Animator>();
  }

  void OnAnimatorIK()
  {
    // Set the right hand target position and rotation, if one has been assigned
    if(rightHandTarget != null) {
      animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
      animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);
      animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandTarget.position);
      animator.SetIKRotation(AvatarIKGoal.RightHand,rightHandTarget.rotation);
    }
  }
}
