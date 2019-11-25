using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO :
// + ajouter un "trigger" en hauteur pour éviter que la caméra fasse une translation prématurée lorsque l'on monte des colines, un escalier, etc (et surtout éviter un effet "bobbing")
// + ajouter animationCurve pour lier la distance à l'avatar au pitch (plus près quand on regarde d'en dessous, plus loin d'au-dessus)
// + ajouter une animationCurve pour que la vitesse de rotation de la cam ne soit pas linéaire par rapport aux inputs  du joueur (à vous de chercher une courbe intéressante)
public class OrbitCamera : MonoBehaviour
{
  public Transform target;
  public float distToTarget = 5f;
  public float yawSpeed = 90f;
  public float pitchSpeed = 90f;
  public float pitchUpwardLimitAngle = 70f;
  public float pitchDownwardLimitAngle = -45f;

  // Courbe qui permet d'ajuster la distance en fonction de l'angle que l'on a à l'avatar
  public bool useDistanceCurve = false;
  public float distToTargetAtMinAngle = 4;
  public float distToTargetAtMaxAngle = 20;

  float currentPitch = 0;
    
  void Start()
  {
    currentPitch = -Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(transform.forward, Vector3.up), transform.right);
  }

  void Update()
  {
    // avatar might have moved, recenter the camera
    float currentDistanceToTarget = distToTarget;
    if(useDistanceCurve)
    {
      // on va faire une interpolation pour la distance à l'avatar
      // qui prend min distance quand le pitch est à l'angle min
      // et max distance quand il est à l'angle max
      // OR, lerp demande un pourcentage, il nous faut donc un pitch en pourcentage
      float pitchPercentage   = Mathf.InverseLerp(pitchDownwardLimitAngle, pitchUpwardLimitAngle, GetPitch());
      currentDistanceToTarget = Mathf.Lerp(distToTargetAtMinAngle, distToTargetAtMaxAngle, pitchPercentage);
    }
    transform.position = target.position - transform.forward * currentDistanceToTarget;

    float yawInput    = Input.GetAxis("RHorizontal");
    float pitchInput  = Input.GetAxis("RVertical");

    transform.RotateAround(target.position, Vector3.up, yawSpeed * yawInput * Time.deltaTime);

    // clamp pitch rotation
    // PB : on tourne autour de transform.right donc on ne peut pas utiliser directement eulerAngles qui sont la rotation autour des axes du monde (car transform.right est probablement une combinaison de X et Z)
    
    RotatePitchSolution0(pitchInput);
    
    //RotatePitchSolution1(pitchInput);

    //RotatePitchSolution2(pitchInput);
  }


  // Solution 0: on tourne seulement mais on ne pourra pas limiter l'angle de pitch
  void RotatePitchSolution0(float pitchInput)
  {
    // on calcule la quantité de rotation qu'on serait censé faire cette frame
    float rotationStep = pitchSpeed * pitchInput * Time.deltaTime;
    // et on l'applique
    transform.RotateAround(target.position, transform.right, rotationStep);
  }

  // Solution 1: on peut utiliser des variables qui trackent la rotation qu'on a fait jusque là et ensuite autoriser ou non la rotation de la caméra
  void RotatePitchSolution1(float pitchInput)
  {

    // on calcule la quantité de rotation qu'on serait censé faire cette frame
    float rotationStep = pitchSpeed * pitchInput * Time.deltaTime;

    // on clampe la valeur si elle est trop grande
    if(currentPitch + rotationStep > pitchUpwardLimitAngle) rotationStep = pitchUpwardLimitAngle - currentPitch;
    if(currentPitch + rotationStep < pitchDownwardLimitAngle) rotationStep = pitchDownwardLimitAngle - currentPitch;

    // et ensuite on tourne
    transform.RotateAround(target.position, transform.right, rotationStep);

    // sans oublier de mettre à jour le pitch
    currentPitch += rotationStep;
  }

  // Solution 2: pour avoir le pitch actuel de la caméra, on peut regarder l'angle entre le forward actuel et le forward projeté sur la plane X-Z (voir fonction GetPitch())
  void RotatePitchSolution2(float pitchInput)
  {
    float currentPitchAngle = GetPitch();
    // on calcule la quantité de rotation qu'on serait censé faire cette frame
    float rotationStep = pitchSpeed * pitchInput * Time.deltaTime;

    // on limite cette rotation pour être sûr de ne pas dépasser l'angle max ou min de pitch
    float rotationAmount = Mathf.Clamp(currentPitchAngle + rotationStep, pitchDownwardLimitAngle, pitchUpwardLimitAngle);

    transform.RotateAround(target.position, transform.right, rotationStep);
  }

  float GetPitch()
  {
    // Si vous trackez le pitch dans une variable comme pour la solution1, cette fonction n'est pas nécessaire

    // Le pitch est l'angle entre le forward et le plan du sol (celui par défaut)
    return Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), transform.forward, transform.right);
  }
}
