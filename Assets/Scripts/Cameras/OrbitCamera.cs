using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

  SphereCollider camCollider;
  OrbitCameraInput input;
    
  void Start()
  {
    camCollider = gameObject.GetComponent<SphereCollider>();
    input = gameObject.GetComponent<OrbitCameraInput>();
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

    float rotationAmount = yawSpeed * input.yawInput * Time.deltaTime;
    TryRotatingCameraAround(target.position, Vector3.up, rotationAmount);

    // clamp pitch rotation
    // PB : on tourne autour de transform.right donc on ne peut pas utiliser directement eulerAngles qui sont la rotation autour des axes du monde (car transform.right est probablement une combinaison de X et Z)
    RotatePitch(input.pitchInput);
  }

  // Solution 2: pour avoir le pitch actuel de la caméra, on peut regarder l'angle entre le forward actuel et le forward projeté sur la plane X-Z (voir fonction GetPitch())
  void RotatePitch(float pitchInput)
  {
    float currentPitchAngle = GetPitch();
    // on calcule la quantité de rotation qu'on serait censé faire cette frame
    float rotationStep = pitchSpeed * pitchInput * Time.deltaTime;
    // on calcule la rotation cible (rot actuelle + rotation induite par les inputs)
    // et on clampe celle-ci
    float rotationTarget = Mathf.Clamp(currentPitchAngle + rotationStep, pitchDownwardLimitAngle, pitchUpwardLimitAngle);
    // dans tous les cas, la quantité de rotation à effectuer pendant cette frame est la différence entre la rotation totale cible et la rotation actuelle
    float rotationAmount = rotationTarget - currentPitchAngle;

    TryRotatingCameraAround(target.position, transform.right, rotationAmount);
  }

  // fonction qui fait tourner la caméra si aucune collision ne se produit
  void TryRotatingCameraAround(Vector3 rotationPivot, Vector3 rotationAxis, float rotationAmount)
  {
    // COLLISIONS
    // ici on va détecter si la caméra est en collision avec un obstacle
    // 1) Si c'est le cas, la première option est de ne pas autoriser la rotation
    // 2) Une autre option est de quand même prendre en compte la demande du joueur et
    // par exemple de rapprocher la caméra de l'avatar pour permettre de tourner sans rentrer dans le mur

    // on commence par bouger la caméra
    transform.RotateAround(rotationPivot, rotationAxis, rotationAmount);
    // on regarde ensuite s'il y a une collision (ne pas prendre la layer de la caméra, ce qui veut dire qu'elle ne doit pas être sur la layer default)
    if(Physics.CheckSphere(transform.position, camCollider.radius, ~(1 << gameObject.layer)))
    {
      // collision, on annule la rotation qu'on vient de faire
      transform.RotateAround(rotationPivot, rotationAxis, -rotationAmount);
    }
  }

  float GetPitch()
  {
    // Si vous trackez le pitch dans une variable comme pour la solution1, cette fonction n'est pas nécessaire

    // Le pitch est l'angle entre le forward et le plan du sol (celui par défaut)
    return Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), transform.forward, transform.right);
  }
}
