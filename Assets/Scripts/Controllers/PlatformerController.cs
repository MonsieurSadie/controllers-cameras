using System;
using UnityEngine;

public class PlatformerController : MonoBehaviour
{
  public enum MovementConstraint
  {
    MOVEMENT_2D,
    MOVEMENT_3D
  };
  public MovementConstraint constraint = MovementConstraint.MOVEMENT_2D;
  public float accel = 10;
  public float maxHorizontalSpeed = 10;
  public float maxVerticalSpeed   = 10;
  public float jumpForce = 10;
  Rigidbody rb;
  Collider col;


  bool onGround = false;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
    col = GetComponent<Collider>();
  }


  void Update()
  {
    GroundDetect();

    float hInput = Input.GetAxis("Horizontal");
    float vInput = Input.GetAxis("Vertical");
    if(constraint == MovementConstraint.MOVEMENT_2D)
    {
      rb.AddForce(hInput * transform.forward * accel);
    }else if(constraint == MovementConstraint.MOVEMENT_3D)
    {
      rb.AddForce(vInput * transform.forward * accel);
      rb.AddForce(hInput * transform.right * accel);
    }

    if(Input.GetButtonDown("Jump"))
    {
      rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    // clamp velocity. Beware, max speed is not the same in h and v directions
    Vector3 vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    vel = Vector3.ClampMagnitude(vel, maxHorizontalSpeed);
    vel.y = Mathf.Sign(rb.velocity.y) * Mathf.Min( Mathf.Abs(rb.velocity.y), maxVerticalSpeed);
    rb.velocity = vel;
  }

  // appelé à chaque frame pour détecter quand on est au sol ou non
  void GroundDetect()
  {
    bool touchingGround = false;
    // on lance un rayon vers le bas qui fait la moitié de la taille de l'avatar
    // (pour être sûr de ne pas considérer que l'on vien de toucher un sol alors qu'il est à 15m de nous...)
    RaycastHit hit;
    if(Physics.Raycast(transform.position, Vector3.down, out hit, col.bounds.extents.y + 0.01f))
    {
      // on vérifie que la normale va vers le haut
      // (en se laissant une marge de précision)
      if(Vector3.Dot(hit.normal, Vector3.up) > 0.99f)
      {
        touchingGround = true;
      }
    }

    // on lance un évènement pour la caméra au cas où on vient de toucher le sol
    if(touchingGround && !onGround)
    {
      Camera.main.gameObject.SendMessage("OnAvatarTouchedGround", hit.point, SendMessageOptions.DontRequireReceiver);
    }

    onGround = touchingGround;
  }
}