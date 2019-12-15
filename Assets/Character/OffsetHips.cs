using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetHips : MonoBehaviour
{
    public Vector3 hipsOffset;
    public Vector3 hipRotationOffset;
 
    Animator m_Animator;
    void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 0)
        {
            m_Animator.bodyPosition += hipsOffset;
            m_Animator.bodyRotation *= Quaternion.Euler(hipRotationOffset.x, hipRotationOffset.y, hipRotationOffset.z);
 
            //Set IK weights
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
 
        }
    }


    // Have some fun
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.Space))
      {
        StartCoroutine(DoLimbooooo());
      }
    }

    IEnumerator DoLimbooooo()
    {
      float t = 0;
      while(t < 1)
      {
        t += Time.deltaTime * 1f/2f;
        hipsOffset.y = Mathf.Lerp(0, -0.5f, t*t);
        hipRotationOffset.x = Mathf.Lerp(0, -80f, t*t);

        yield return null;
      }
    }
}
