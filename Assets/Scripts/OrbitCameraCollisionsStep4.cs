using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrbitCameraCollisionsStep4 : MonoBehaviour
{
  public Transform target;
  public float distToTarget = 5f;
  public float yawSpeed = 90f;
  public float pitchSpeed = -90f; // invert by default
  public float pitchUpwardLimitAngle = 70f;
  public float pitchDownwardLimitAngle = -45f;

  // Courbe qui permet d'ajuster la distance en fonction de l'angle que l'on a à l'avatar
  public bool useDistanceCurve = true;
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

    // la position de la caméra est toujours déterminée par celle de l'avatar
    // On part de la position de l'avatar puis on se déplace dans la direction actuelle de regard de la caméra (qui est auto-calculée par les rotate around) de la distance voulue
    Vector3 wantedPosition = target.position - transform.forward * currentDistanceToTarget;
    TryMovingCamera(wantedPosition);
    // Debug.LogFormat("Camera position after positioning: {0}", transform.position.ToString("F4"));

    float yawRotationAmount = yawSpeed * input.yawInput * Time.deltaTime;
    TryRotatingCameraAround(target.position, Vector3.up, yawRotationAmount);
    // Debug.LogFormat("Camera position after yaw rotation: {0}", transform.position.ToString("F4"));

    // clamp pitch rotation
    // PB : on tourne autour de transform.right donc on ne peut pas utiliser directement eulerAngles qui sont la rotation autour des axes du monde (car transform.right est probablement une combinaison de X et Z)
    RotatePitch(input.pitchInput);
    // Debug.LogFormat("Camera position after pitch rotation: {0}", transform.position.ToString("F4"));
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

    // on commence par bouger la caméra (car on ne connait pas la position induite par la rotation que l'on veut faire)
    transform.RotateAround(rotationPivot, rotationAxis, rotationAmount);
    // on regarde ensuite s'il y a une collision (ne pas prendre la layer de la caméra, ce qui veut dire qu'elle ne doit pas être sur la layer default)
    if(Physics.CheckSphere(transform.position, camCollider.radius, ~(1 << gameObject.layer)))
    {
      // collision, on annule la rotation qu'on vient de faire (ça ne pose pas de problème, pour rappel le résultat de nos calculs ne se verra qu'à la fin de la frame)
      transform.RotateAround(rotationPivot, rotationAxis, -rotationAmount);
    }
  }

  // même idée que pour try rotate, on valide on pas la demande de nouvelle position
  void TryMovingCamera(Vector3 newPosition)
  {
    // ici c'est plus simple, on connait la position cible
    if(!Physics.CheckSphere(newPosition, camCollider.radius, ~(1 << gameObject.layer)))
    {
      // pas de collision, on accepte le déplacement
      transform.position = newPosition;
    }else{
      Debug.LogFormat("collision while trying moving camera from position {0} to position {1}", transform.position.ToString("F4"), newPosition.ToString("F4"));

      // on risque de créer un décalage entre la position de l'avatar et celle de la caméra, on refait un lookat
      transform.LookAt(target.position, Vector3.up);
    }
  }

  float GetPitch()
  {
    // Si vous trackez le pitch dans une variable comme pour la solution1, cette fonction n'est pas nécessaire

    // Le pitch est l'angle entre le forward et le plan du sol (celui par défaut)
    return Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), transform.forward, transform.right);
  }
}
