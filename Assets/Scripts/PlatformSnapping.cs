using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSnapping : Camera2DBehaviour
{
  override protected void Awake()
  {
    base.Awake();
    
    axis = Axis.Vertical;
  }

  void OnAvatarTouchedGround(Vector3 groundPos)
  {
    target.y = groundPos.y;
  }
}
