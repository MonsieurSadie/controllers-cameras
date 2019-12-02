using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosHelper
{
  public static void DrawLine(Vector3 origin, Vector3 direction, float size)
  {
    Gizmos.DrawLine(origin - direction * size * 0.5f, origin + direction * size * 0.5f);
  }
}
