using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO : ajouter une courbe de sensibilité qui fasse que j'ai très peu de sensibilité quand je me déplace lentement et beaucoup quand je veux me déplacer vite

// DEBUG : ground collision detection

public class CharacterControllerVelocity : MonoBehaviour
{
  public bool useAcceleration = false;
  public float accel = 10;

  public float decel = 100;
  public float maxHorizontalSpeed = 10;
  public float maxVerticalSpeed   = 10;
  public float jumpForce = 10;
  public float gravity = 10;
  
  public SphereCollider feetCollider;
  Rigidbody rb;

  Vector3 velocity = Vector3.zero;


  public enum ForwardAlignment
  {
    None,
    Camera,
    MovementDirection
  };
  public ForwardAlignment fwdAlignment = ForwardAlignment.Camera;


  // permet d'activer ou non les forces que unity voudrait appliquer à notre rigidbody
  // (forces dues au collisions principalement)
  public bool enableUnityPhysicsExternalForces = false;

  public bool isOnGround = false;

  public AnimationCurve axisSensitivity = AnimationCurve.Constant(-1, 1, 1);

  public float deadzone = 0.1F;


  public bool isSliding = false;

  public enum JumpState
  {
    None,
    Start,
    InAir
  };
  JumpState jumpState = JumpState.None;

  public Vector3 input = Vector3.zero;

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
    if(Physics.SphereCast(feetCollider.transform.position, feetCollider.radius, Vector3.down, out hit, feetCollider.radius * 1.2f, 1, QueryTriggerInteraction.Ignore))
    {
      if(hit.distance < 0.1f)
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


    

    float hInput = axisSensitivity.Evaluate( Input.GetAxis("Horizontal") );
    float vInput = axisSensitivity.Evaluate( Input.GetAxis("Vertical") );
    // les inputs sont interprétées dans le référentiel de la caméra
    Vector3 camFwd    = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
    Vector3 camRight  = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up);
    input             = camFwd.normalized * vInput + camRight.normalized * hInput;



    // cancel moves when jump is starting
    if(jumpState == JumpState.Start) return;

    if(isSliding) return;


    //// Orientation avatar

    // don't change orientation if no input
    if(input.magnitude > deadzone)
    {
      switch(fwdAlignment)
      {
        case ForwardAlignment.Camera :
        {
          // align forward with wanted direction
          Vector3 fwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
          transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
        break;

        case ForwardAlignment.MovementDirection :
        {
          // align forward with wanted direction
          // should project on ground in case of non-flat floor
          transform.rotation = Quaternion.LookRotation(input, Vector3.up);
        }
        break;
      }
    }


    // on teste si on a des inputs cette frame pour savoir si on doit décélérer
    if(input.magnitude < deadzone )
    {
      // la décélération ne s'applique pas à la vitesse verticale, décomposons la vitesse
      Vector3 xzVelocity = new Vector3(velocity.x, 0, velocity.z);
      xzVelocity = Vector3.MoveTowards(velocity, Vector3.zero, decel * Time.fixedDeltaTime);
      velocity.x = xzVelocity.x;
      velocity.z = xzVelocity.z;
    }else
    {
      if(useAcceleration)
      {
        velocity += input.magnitude * transform.forward * accel * Time.fixedDeltaTime;
      }else
      {
        Vector3 xzVelocity = input.magnitude * transform.forward * maxHorizontalSpeed;
        xzVelocity.y = velocity.y;
        velocity = xzVelocity;
      }
    }
    
    // clamp velocity. Beware, max speed can be different in horizontal and vertical directions
    velocity = GetClampedVelocity(velocity, maxHorizontalSpeed, maxVerticalSpeed);

    // autre cas particulier : si le joueur avance contre une surface alors qu'il est en train de sauter.
    // dans ce cas on ne devrait pas lui donner de vitesse horizontale (un peu comme quand on est au sol pour la vitesse verticale)
    // on peut choisir de projeter le vecteur de déplacement horizontal sur ce mur si il y a collision
    if(!isOnGround)
    {
      Vector3 hVelocity = new Vector3(velocity.x, 0, velocity.z);
      if(Physics.Raycast(feetCollider.transform.position, hVelocity.normalized, out hit, feetCollider.radius))
      {
        if(hit.distance < 0.01f)
        {
          velocity.x = 0;
          velocity.z = 0;
        }
      }
    }

    rb.velocity = velocity;
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
    if(Input.GetButtonDown("Jump") && isOnGround)
    {
      BroadcastMessage("InitiateJump", SendMessageOptions.DontRequireReceiver);
      velocity += Vector3.up * jumpForce;
      // InitiateJump();
    }

    if(Input.GetButtonDown("Slide") && isOnGround)
    {
      isSliding = true;
    }
  }

  void OnDrawGizmos()
  {
    Gizmos.DrawLine(transform.position, transform.position + velocity * 5);
    // debgu separate horizontal and vertical velocities
    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(velocity.x, 0, velocity.y));
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, velocity.y, 0));
  }
}
