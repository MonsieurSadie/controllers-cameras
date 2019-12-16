using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterControllerVelocity : MonoBehaviour
{
  [Header("Physics")]
  public bool enableUnityPhysicsExternalForces = false; // permet d'activer ou non les forces que unity voudrait appliquer à notre rigidbody (forces dues au collisions principalement)
  public bool accelerateTowardsTargetVelocity = false;
  public float accel = 10;
  public float decel = 100;
  public float maxHorizontalSpeed = 10;
  public float maxVerticalSpeed   = 10;
  public float jumpForce = 10;
  public float gravity = 10;
  Vector3 velocity = Vector3.zero;
  Vector3 targetVelocity = Vector3.zero;



  public enum ForwardAlignment
  {
    None,
    Camera,
    MovementDirection
  };
  [Header("Alignment")]
  public ForwardAlignment fwdAlignment = ForwardAlignment.Camera;



  [Header("Input")]
  public AnimationCurve axisSensitivity = AnimationCurve.Constant(-1, 1, 1);
  public float deadzone = 0.1F;
  public Vector3 inputAxis = Vector3.zero;



  [Header("States")]
  public bool isOnGround  = false;
  public bool isSliding   = false;
  public bool isJumping   = false;



  // Unity things
  public SphereCollider feetCollider;
  Rigidbody rb;



  void Awake()
  {
    rb = GetComponent<Rigidbody>();
  }


  void FixedUpdate()
  {

    /////////////////////////////////////////////////////////////
    //// EXTERNAL FORCES
    /////////////////////////////////////////////////////////////

    if(enableUnityPhysicsExternalForces && Vector3.Distance(velocity, rb.velocity) > 0.1f)
    {
      // si la vélocité du rigidbody n'est plus la même que celle notre variable velocity, 
      // c'est qu'une force a été appliquée à celui-ci, nous devons donc la rappatrier dans notre vélocité
      velocity = rb.velocity;
    }


    /////////////////////////////////////////////////////////////
    //// GROUND DETECTION
    /////////////////////////////////////////////////////////////

    RaycastHit hit;
    if(Physics.SphereCast(feetCollider.transform.position, feetCollider.radius, Vector3.down, out hit, feetCollider.radius * 1.2f, 1, QueryTriggerInteraction.Ignore))
    {
      if(hit.distance < 0.1f)
      {
        isOnGround = true;
        isJumping = false;
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



    /////////////////////////////////////////////////////////////
    //// GRAVITY
    /////////////////////////////////////////////////////////////
    if(!isOnGround)
    {
      velocity.y += gravity * Time.fixedDeltaTime;
    }


    
    /////////////////////////////////////////////////////////////
    //// DIRECTION INPUT (KEYDOWN INPUTS IN UPDATE)
    /////////////////////////////////////////////////////////////

    float hInput = axisSensitivity.Evaluate( Input.GetAxis("Horizontal") );
    float vInput = axisSensitivity.Evaluate( Input.GetAxis("Vertical") );
    // les inputs sont interprétées dans le référentiel de la caméra
    Vector3 camFwd    = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
    Vector3 camRight  = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up);
    inputAxis             = camFwd.normalized * vInput + camRight.normalized * hInput;


    // Don't apply movement when sliding
    if(isSliding) return;


    /////////////////////////////////////////////////////////////
    //// AVATAR ORIENTATION
    /////////////////////////////////////////////////////////////
    // don't change orientation if no input
    if(inputAxis.magnitude > deadzone)
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
          transform.rotation = Quaternion.LookRotation(inputAxis, Vector3.up);
        }
        break;
      }
    }


    /////////////////////////////////////////////////////////////
    //// COMPUTE VELOCITY
    /////////////////////////////////////////////////////////////

    // on teste si on a des inputs cette frame pour savoir si on doit décélérer
    if(inputAxis.magnitude < deadzone )
    {
      // la décélération ne s'applique pas à la vitesse verticale, décomposons la vitesse
      Vector3 xzVelocity = new Vector3(velocity.x, 0, velocity.z);
      xzVelocity = Vector3.MoveTowards(velocity, Vector3.zero, decel * Time.fixedDeltaTime);
      velocity.x = xzVelocity.x;
      velocity.z = xzVelocity.z;
    }else
    {
      Vector3 xzVelocity = inputAxis.magnitude * transform.forward * maxHorizontalSpeed;
      xzVelocity.y = velocity.y;
      targetVelocity = xzVelocity;

      if(accelerateTowardsTargetVelocity)
      {
        velocity = Vector3.MoveTowards(velocity, targetVelocity, accel * Time.fixedDeltaTime);
      }else
      {
        velocity = targetVelocity;
      }
    }
    
    // clamp velocity. Beware, max speed can be different in horizontal and vertical directions
    velocity = GetClampedVelocity(velocity, maxHorizontalSpeed, maxVerticalSpeed);

    // autre cas particulier : si le joueur avance contre une surface alors qu'il est déjà en collision avec celle-ci.
    // dans ce cas on ne devrait pas lui donner de vitesse horizontale (un peu comme quand on est au sol pour la vitesse verticale)
    // on peut choisir de projeter le vecteur de déplacement horizontal sur ce mur si il y a collision
    Vector3 hVelocity = new Vector3(velocity.x, 0, velocity.z);
    if(Physics.Raycast(feetCollider.transform.position, hVelocity.normalized, out hit, Mathf.Infinity))
    {
      if(hit.distance < feetCollider.radius*1.1f)
      {
        velocity.x = 0;
        velocity.z = 0;
      }
    }

    // apply back to rigidbody
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
      isJumping = true;
      velocity += Vector3.up * jumpForce;
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
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(velocity.x, 0, velocity.z));
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, velocity.y, 0));
  }
}
