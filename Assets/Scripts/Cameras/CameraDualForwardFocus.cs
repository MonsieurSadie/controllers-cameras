using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDualForwardFocus : MonoBehaviour
{
  public Transform followee;


  // Anchor définit la zone de la caméra (donc de l'écran) où doti se trouver l'avatar
  // Trigger défini la zone de la caméra qui déclenche le changement d'ancre
  public float leftAnchor    = -1;
  public float rightAnchor   = 1;
  public float leftTrigger   = -2;
  public float rightTrigger  = 2;
  
  Vector3 previousFolloweePosition;

  Vector3 target;

  //
  Transform targetAnchorTr;
  Transform leftAnchorTr;
  Transform rightAnchorTr;

  void Start()
  {
    target = transform.position;
    leftAnchorTr = new GameObject("left-anchor").transform;
    leftAnchorTr.parent = transform;
    leftAnchorTr.position = new Vector3(leftAnchor, 0, 0);
    rightAnchorTr = new GameObject("right-anchor").transform;
    rightAnchorTr.parent = transform;
    rightAnchorTr.position = new Vector3(rightAnchor, 0, 0);
    targetAnchorTr = leftAnchorTr;
  }

  void Update()
  {
    // if we hit left trigger
    if(followee.position.x < transform.position.x + leftTrigger && 
      previousFolloweePosition.x > transform.position.x + leftTrigger
      )
    {
      // target right anchor
      targetAnchorTr = rightAnchorTr;
    }

    // if we hit right trigger
    if(followee.position.x > transform.position.x + rightTrigger && 
      previousFolloweePosition.x < transform.position.x + rightTrigger
      )
    {
      // target left anchor
      targetAnchorTr = leftAnchorTr;
    }

    // only follow player if going in opposite direction of current anchor
    if( (targetAnchorTr == leftAnchorTr && followee.position.x > targetAnchorTr.position.x) ||
        (targetAnchorTr == rightAnchorTr && followee.position.x < targetAnchorTr.position.x)
    )
    {
      // Pour la destination, on ne touche pas au Y et au Z de la caméra
      Vector3 currentTarget = transform.position;
      // Le centre de la caméra doit se trouver à la position de l'avatar - la position locale de l'ancre sélectionnée (si on n'utilise pas la position locale, il faut simplement soustraire position de l'ancre et position de la caméra)
      currentTarget.x = followee.position.x - targetAnchorTr.localPosition.x;
      transform.position = Vector3.Lerp(transform.position, currentTarget, 0.2f);
    }

    previousFolloweePosition = followee.position;
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.white;
    GizmosHelper.DrawLine(transform.position + Vector3.right*leftAnchor, Vector3.up, 2);
    GizmosHelper.DrawLine(transform.position + Vector3.right*rightAnchor, Vector3.up, 2);
    Gizmos.color = Color.yellow;
    GizmosHelper.DrawLine(transform.position + Vector3.right*leftTrigger, Vector3.up, 2);
    GizmosHelper.DrawLine(transform.position + Vector3.right*rightTrigger, Vector3.up, 2);
  }
}
