using System;
using UnityEngine;


// 2D Camera with simple sliding window
// /!\ SIDE SCROLLER : need modifications if Top View
public class Camera2D : MonoBehaviour
{
  public enum CameraBehaviour
  {
    SPEED_UP_PULL_ZONE,
    SPEED_UP_PULL_ZONE_LERPED,
    PLATFORM_SNAPPING,
  };

  public CameraBehaviour cameraType = CameraBehaviour.SPEED_UP_PULL_ZONE;
  public Transform target;
  public Vector2 windowExtent;

  Vector3 velocity = Vector3.zero;

  Vector3 currentTarget;


  void Update()
  {
    switch(cameraType)
    {
      case CameraBehaviour.PLATFORM_SNAPPING:
      {

      }
      break;

      case CameraBehaviour.SPEED_UP_PULL_ZONE :
      {
        Vector3 camPos      = transform.position;
        Vector3 targetPos   = target.position;
        Vector3 toTargetVec = targetPos - camPos;
        if( toTargetVec.x > windowExtent.x)
        {
          camPos.x = targetPos.x -windowExtent.x;
        }
        if(toTargetVec.x < -windowExtent.x)
        {
          camPos.x = targetPos.x + windowExtent.x;
        }

        if( toTargetVec.y > windowExtent.y)
        {
          camPos.y = targetPos.y - windowExtent.y;
        }
        if(toTargetVec.y < -windowExtent.y)
        {
          camPos.y = targetPos.y + windowExtent.y;
        }
        transform.position = camPos;
      }
      break;

      case CameraBehaviour.SPEED_UP_PULL_ZONE_LERPED :
      {
        Vector3 targetPos = target.position;
        Vector3 camPos = transform.position;
        Vector3 toTargetVec = targetPos - camPos;

        if( toTargetVec.x > windowExtent.x)
        {
          camPos.x = targetPos.x -windowExtent.x;
        }
        if(toTargetVec.x < -windowExtent.x)
        {
          camPos.x = targetPos.x + windowExtent.x;
        }

        if( toTargetVec.y > windowExtent.y)
        {
          camPos.y = targetPos.y - windowExtent.y;
        }
        if(toTargetVec.y < -windowExtent.y)
        {
          camPos.y = targetPos.y + windowExtent.y;
        }
        
        

        // ### solution en lerp avec T fixé
        {
        transform.position = Vector3.Lerp(transform.position, camPos, 0.1f);
        }

        // ### solution avec move towards (vitesse fixée)
        // {
        // float camSpeed = 1f;
        // transform.position = Vector3.MoveTowards(transform.position, camPos, camSpeed *Time.deltaTime);
        // }

        // ### solution avec smooth damp
        // {
        // transform.position = Vector3.SmoothDamp(transform.position, camPos, ref velocity, 0.5f);
        // }
      }
      break;
    }
  }

  // must be called by the avatar
  public void OnAvatarTouchedGround(Vector3 groundHitPos)
  {
    if(cameraType == CameraBehaviour.PLATFORM_SNAPPING)
    {
      currentTarget.y = groundHitPos.y;
    }
  }


  void OnDrawGizmos()
  {
    // on affiche les zones autour de la caméra
    Gizmos.color = Color.black;
    // left limit
    Gizmos.DrawLine( new Vector3(transform.position.x - windowExtent.x, transform.position.y-2, 0), new Vector3(transform.position.x - windowExtent.x, transform.position.y+2, 0) );
    // right limit
    Gizmos.DrawLine( new Vector3(transform.position.x + windowExtent.x, transform.position.y-2, 0), new Vector3(transform.position.x + windowExtent.x, transform.position.y+2, 0) );
    // up limit
    Gizmos.DrawLine( new Vector3(transform.position.x-2, transform.position.y + windowExtent.y, 0), new Vector3(transform.position.x+2, transform.position.y + windowExtent.y, 0) );
    Gizmos.DrawLine( new Vector3(transform.position.x-2, transform.position.y - windowExtent.y, 0), new Vector3(transform.position.x+2, transform.position.y - windowExtent.y, 0) );
  }

}