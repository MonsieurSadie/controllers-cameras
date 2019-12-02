using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2DBehaviour : MonoBehaviour
{
  public enum Axis
  {
    Horizontal,
    Vertical,
    Both
  };

  public enum MoveType
  {
    Teleport,
    AsymptoticLerp,
    ConstantSpeed
  };

  public Axis axis = Axis.Horizontal;
  public MoveType moveType = MoveType.Teleport;

  protected Vector3 target;

  public Vector3 offset = Vector3.zero;

  public float speed = 0;

  virtual protected void Awake()
  {
    target = transform.position;
  }

  void Update()
  {
    Vector3 curentTarget = transform.position;
    if(axis == Axis.Horizontal) curentTarget.x = target.x + offset.x;
    if(axis == Axis.Vertical) curentTarget.y = target.y + offset.y;
    
    switch(moveType)
    {
      case MoveType.Teleport:
      transform.position = curentTarget;
      break;

      case MoveType.AsymptoticLerp:
      transform.position = Vector3.Lerp(transform.position, curentTarget, 0.1f);
      break;

      case MoveType.ConstantSpeed:
      transform.position = Vector3.MoveTowards(transform.position, curentTarget, speed * Time.deltaTime);
      break;
    }
  }
}
