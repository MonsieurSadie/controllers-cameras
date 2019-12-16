using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MvtInCamReferential : MonoBehaviour
{
  public enum TestCase{
    USE_NOTHING,
    PROJECT_ON_GROUND_PLANE,
    PROJECT_ON_GROUND_TERRAIN,
    PROJECT_ON_GROUND_MESH
  };

  public float maxSpeed = 10; // m/s
  public float accel    = 1; // m/s
  public float drag     = 1;
  Rigidbody rb;
  Collider col;
  Camera cam;


  // store ground normal to display it as Gizmo
  Vector3 groundNormal      = Vector3.up;
  Vector3 camFwd            = Vector3.forward;
  Vector3 camRight          = Vector3.forward;

  public TestCase testCase = TestCase.USE_NOTHING;

  void Start()
  {
    rb  = gameObject.GetComponent<Rigidbody>();
    col = gameObject.GetComponent<Collider>();
    cam = Camera.main;
  }

  void FixedUpdate()
  {
    float inputX = Input.GetAxis("Horizontal");
    float inputY = Input.GetAxis("Vertical");

    // no dragwhenMoving
    if(inputX != 0 || inputY != 0) rb.drag = 0;
    else rb.drag = drag;
    // ou
    // Vector2 dir = new Vector2(inputX, inputY).normalized;
    // rb.drag = (1 - Vector2.Dot(dir, dir)) * drag;
    
    // compute movement direction
    Vector3 mvtDirection = Vector3.zero;
    switch(testCase)
    {
      case TestCase.USE_NOTHING:
        mvtDirection = inputX * cam.transform.right + inputY * cam.transform.forward;
      break;

      case TestCase.PROJECT_ON_GROUND_PLANE:
      { // ici les accolades sont nécessaire pour éviter des erreurs de redéclaration des variables camFwd et camRight
        camFwd            = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
        camRight          = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up);
        mvtDirection      = inputX * camRight + inputY * camFwd;
      }
      break;

      case TestCase.PROJECT_ON_GROUND_TERRAIN:
      {
        // TODO
      }
      break;

      case TestCase.PROJECT_ON_GROUND_MESH:
      {
        Vector3 nextGroundNormal  = GroundHelper.GetGroundNormalBelowSphere(transform.position, Vector3.down, transform.lossyScale.magnitude * 0.5f);
        groundNormal              = Vector3.RotateTowards(groundNormal, nextGroundNormal, 10 * Time.fixedDeltaTime, 1);
        
        
        Vector3 fwdOnFlatPlane = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
        camFwd = Vector3.ProjectOnPlane(fwdOnFlatPlane, groundNormal);

        Vector3 rightOnFlatPlane = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up);
        camRight = Vector3.ProjectOnPlane(rightOnFlatPlane, groundNormal);
        // ou
        //camRight = Vector3.Cross(groundNormal, camFwd);

        mvtDirection = inputX * camRight + inputY * camFwd;

        transform.rotation = Quaternion.LookRotation(camFwd, groundNormal);
      }
      break;
    }
    
    mvtDirection.Normalize();
    rb.AddForce(mvtDirection * accel);
    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + groundNormal*5);

    Gizmos.color = Color.blue;
    Gizmos.DrawLine(transform.position, transform.position + camFwd*5);

    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, transform.position + camRight*5);
  }
}
