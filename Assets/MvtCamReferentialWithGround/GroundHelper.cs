using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundHelper
{
  public static Vector3 GetTerrainNormalAtWorldPos(Vector3 worldPos)
  {
    // TODO
    return Vector3.up;
  }

  // la fonction se contente de retourner la normale du premier collider situé en dessous de la position donnée en paramètres
  // si la direction "dessous" n'est pas celle du monde, le deuxième paramètre permet de le préciser
  public static Vector3 GetGroundNormalAtWorldPos(Vector3 worldPos, Vector3 downDirection)
  {
    Vector3 normal = Vector3.up;
    RaycastHit hit;
    if(Physics.Raycast(worldPos, downDirection, out hit))
    {
      normal = hit.normal;
    }

    return normal;
  }

  // version projettant un collider plutôt qu'un rayon
  public static Vector3 GetGroundNormalBelowCollider(Vector3 worldPos, Vector3 downDirection, Collider collider)
  {
    Vector3 normal = Vector3.up;
    RaycastHit hit;
    
    if(collider.Raycast(new Ray(worldPos, downDirection), out hit, Mathf.Infinity))
    {
      normal = hit.normal;
    }

    return normal;
  }

  // version projettant un cube plutôt qu'un rayon
  public static Vector3 GetGroundNormalBelowCube(Vector3 worldPos, Vector3 downDirection, Vector3 cubeExtents, Quaternion cubeOrientation)
  {
    Vector3 normal = Vector3.up;
    RaycastHit hit;
    
    if(Physics.BoxCast(worldPos, cubeExtents, downDirection, out hit, cubeOrientation, Mathf.Infinity))
    {
      normal = hit.normal;
    }

    return normal;
  }

  // version projettant une sphère plutôt qu'un rayon
  public static Vector3 GetGroundNormalBelowSphere(Vector3 worldPos, Vector3 downDirection, float radius)
  {
    Vector3 normal = Vector3.up;
    RaycastHit hit;
    
    if(Physics.SphereCast(worldPos, radius, downDirection, out hit))
    {
      normal = hit.normal;
    }

    return normal;
  }
}
