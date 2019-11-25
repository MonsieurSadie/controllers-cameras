using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// version qui détecte le mesh en dessous
public class MovePositionController : MonoBehaviour
{
  public enum ForwardType{
    AVATAR,
    CAMERA
  };
  public ForwardType movementForward = ForwardType.CAMERA;
  public float speed = 10;

  public bool alignUpWithSlope = false;

  Vector3 velocity = Vector3.zero;

  Vector3 fwd;
  Vector3 right;
  Vector3 groundNormal;

  Rigidbody rb;
  Transform cam;

  float halfHeight;
  float radius;

  void Awake()
  {
    rb          = gameObject.GetComponent<Rigidbody>();
    Debug.Assert(rb.isKinematic == true); // Assert génère une erreur si la condition est fausse, ici on a besoin que ce soit kinematic
    cam         = Camera.main.transform;
    halfHeight  = transform.lossyScale.y * 0.5f;
    radius      = transform.lossyScale.x * 0.5f;
  }

  void FixedUpdate()
  {
    float inputX = Input.GetAxis("Horizontal");
    float inputY = Input.GetAxis("Vertical");

    Vector3 nextGroundNormal  = GroundHelper.GetGroundNormalBelowSphere(transform.position, Vector3.down, radius);
    groundNormal              = Vector3.RotateTowards(groundNormal, nextGroundNormal, 90 * Time.fixedDeltaTime, 1);
    
    switch(movementForward)
    {
      case ForwardType.CAMERA :
      {
        fwd = cam.transform.forward;
        right = cam.transform.right;
      }
      break;

      case ForwardType.AVATAR:
      {
        fwd = transform.forward;
        right = transform.right;
      }
      break;
    }

    Vector3 fwdOnFlatPlane = Vector3.ProjectOnPlane(fwd, Vector3.up);
    fwd = Vector3.ProjectOnPlane(fwdOnFlatPlane, groundNormal);  

    Vector3 rightOnFlatPlane = Vector3.ProjectOnPlane(right, Vector3.up);
    right = Vector3.ProjectOnPlane(rightOnFlatPlane, groundNormal);
    // ou
    //right = Vector3.Cross(groundNormal, fwd);

    Vector3 mvtDirection = inputX * right + inputY * fwd;
    mvtDirection.Normalize();

    Vector3 targetPosition = rb.position + mvtDirection * speed * Time.fixedDeltaTime;
    targetPosition = HandleMovementCollision(targetPosition);
    rb.MovePosition( targetPosition );

    if(alignUpWithSlope) transform.rotation = Quaternion.LookRotation(fwd, groundNormal);
  }


  Vector3 collisionNormal;
  Vector3 HandleMovementCollision(Vector3 desiredPosition)
  {
    Vector3 movementDir = desiredPosition - rb.position;
    RaycastHit hit;
    if(rb.SweepTest(movementDir.normalized, out hit, movementDir.magnitude))
    {
      Debug.LogFormat("== detected collision in movement direction");
      // on se déplace jusqu'à la collision
      // On retire cependant une petite valeur afin d'éviter d'être pile en contact avec l'objet de collision
      // (si cela arrivait, on risquerait de passer à travers à la prochaine frame car SweepTest ne permet pas de détecter une collision avec un objet avec lequel on est déjà en overlap)
      Vector3 finalPosition = rb.position + movementDir.normalized * (hit.distance-0.01f);
      Debug.LogFormat("current position: {2} / desired position: {0} / modified position {1}", desiredPosition.ToString("0.000000"), finalPosition.ToString("0.000000"), rb.position.ToString("0.000000"));

      // le problème, si on fonce dans un mur, c'est qu'on va être bloqué par celui-ci.
      // Or, si on faisait un mouvement en diagonale, on voudrait "glisser le long du mur"
      // pour résoudre ça, on se déplace jusqu'au mur
      // On prend ensuite ce qui reste du mouvement initial à faire et on le projet sur le mur
      // ainsi, si on avait un mouvement qui n'est pas orthogonal à la surface de collision, on va faire un pas de côté
      Vector3 remainingMove = desiredPosition - finalPosition;
      collisionNormal = -movementDir.normalized; // par défaut, on met une normale qui est celle du déplacement
      if(Physics.Raycast(finalPosition, movementDir.normalized, out hit))
      {
        collisionNormal = hit.normal;
        finalPosition += Vector3.ProjectOnPlane(remainingMove, collisionNormal);
      }

      return finalPosition;
    }
    return desiredPosition;
  }



  void OnDrawGizmos()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + groundNormal*5);

    Gizmos.color = Color.blue;
    Gizmos.DrawLine(transform.position, transform.position + fwd*5);

    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, transform.position + right*5);

    Gizmos.color = Color.black;
    Gizmos.DrawLine(transform.position, transform.position + collisionNormal*5);
  }
}
