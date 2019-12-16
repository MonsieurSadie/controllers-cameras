using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a component that defines when the camera move
public class CameraWindow : MonoBehaviour
{
  public Transform pivot; // target
  public float top    = 1;
  public float bottom = 1;
  public float left   = 1;
  public float right  = 1;

  

  void OnDrawGizmos()
  {
    // on affiche les zones autour de la caméra
    Gizmos.color = Color.black;
    // left limit
    Gizmos.DrawLine( new Vector3(pivot.position.x - left, pivot.position.y-bottom, 0), new Vector3(pivot.position.x - left, pivot.position.y+top, 0) );
    // right limit
    Gizmos.DrawLine( new Vector3(pivot.position.x + right, pivot.position.y-bottom, 0), new Vector3(pivot.position.x + right, pivot.position.y+top, 0) );
    // up limit
    Gizmos.DrawLine( new Vector3(pivot.position.x-left, pivot.position.y + top, 0), new Vector3(pivot.position.x+right, pivot.position.y + top, 0) );
    Gizmos.DrawLine( new Vector3(pivot.position.x-left, pivot.position.y - bottom, 0), new Vector3(pivot.position.x+right, pivot.position.y - bottom, 0) );
  }
}
