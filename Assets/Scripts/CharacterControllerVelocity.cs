using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerVelocity : MonoBehaviour
{
  public float accel = 10;
  public float decel = 100;
  public float maxHorizontalSpeed = 10;
  public float maxVerticalSpeed   = 10;
  public float jumpForce = 10;
  public float gravity = 10;
  
  public SphereCollider feetCollider;
  Rigidbody rb;
  Vector3 velocity = Vector3.zero;


  // permet d'activer ou non les forces que unity voudrait appliquer à notre rigidbody
  // (forces dues au collisions principalement)
  public bool enableUnityPhysicsExternalForces = false;

  public bool isOnGround = false;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }


  void FixedUpdate()
  {
    if(enableUnityPhysicsExternalForces && Vector3.Distance(velocity, rb.velocity) > 0.1f)
    {
      // si la vélocité du rigidbody n'est plus la même que celle notre variable velocity, 
      // c'est qu'une force a été appliquée à celui-ci, nous devons donc la rappatrier dans notre vélocité
      velocity = rb.velocity;
    }

    // on ground ?
    RaycastHit hit;
    if(Physics.SphereCast(feetCollider.transform.position, feetCollider.radius, Vector3.down, out hit, feetCollider.radius, 1, QueryTriggerInteraction.Ignore))
    {
      if(hit.distance < 0.01f)
      {
        isOnGround = true;
        // laisser la gravité lorsque l'on est au sol demande de mettre des forces de saut élevées
        // et empêche de monter des pentes
        // c'est souvent plus pratique de la mettre à 0
        // (même si ce n'est absolument pas réaliste)
        if(velocity.y < 0) velocity.y = 0;
      }
    }else
    {
      isOnGround = false;
    }


    // on applique la gravité, sauf si on est au sol
    if(!isOnGround)
    {
      velocity.y += gravity * Time.fixedDeltaTime;
    }

    float hInput = Input.GetAxis("Horizontal");
    float vInput = Input.GetAxis("Vertical");

    // on teste si on a des inputs cette frame pour savoir si on doit décélérer
    if(Mathf.Abs(hInput + vInput) < 0.1f )
    {
      // la décélération ne s'applique pas à la vitesse verticale, décomposons la vitesse
      Vector3 xzVelocity = new Vector3(velocity.x, 0, velocity.z);
      xzVelocity = Vector3.MoveTowards(velocity, Vector3.zero, decel * Time.fixedDeltaTime);
      velocity.x = xzVelocity.x;
      velocity.z = xzVelocity.z;
    }else
    {
      velocity += vInput * transform.forward * accel;
      velocity += hInput * transform.right * accel;
    }
    
    // clamp velocity. Beware, max speed can be different in horizontal and vertical directions
    velocity = GetClampedVelocity(velocity, maxHorizontalSpeed, maxVerticalSpeed);

    rb.velocity = velocity;

    // align forward with wanted direction
    Vector3 fwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
    transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
  }

  Vector3 GetClampedVelocity(Vector3 currentVelocity, float maxHorizontalVelocity, float maxVerticalVelocity)
  {
    Vector3 clampedVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
    clampedVelocity         = Vector3.ClampMagnitude(currentVelocity, maxHorizontalVelocity);
    clampedVelocity.y       = Mathf.Sign(currentVelocity.y) * Mathf.Min( Mathf.Abs(currentVelocity.y), maxVerticalVelocity);
    return clampedVelocity;
  }

  void Update()
  {
    // le saut est un "KeyDown", il doit donc être testé dans l'update
    if(Input.GetButtonDown("Jump"))
    {
      velocity += Vector3.up * jumpForce;
    }
  }

  void OnDrawGizmos()
  {
    Gizmos.DrawLine(transform.position, transform.position + velocity * 5);
  }
}
