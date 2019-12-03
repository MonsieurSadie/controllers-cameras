using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

// TODO : ajouter XInput
public class OrbitCameraInput : MonoBehaviour
{
  public enum ControlType
  {
    GamepadUnity,
    GamepadXInput,
    Mouse
  };

  public ControlType controlType = ControlType.GamepadUnity;

  [Tooltip("Value of the input bound to yaw rotation (rotation around Y-axis)")]
  public float yawInput;
  [Tooltip("Value of the input bound to pitch rotation (rotation around X-axis)")]
  public float pitchInput;

  [Tooltip("Multiplier on input values")]
  public float sensitivity = 1;

  void Start()
  {
    // Quand on utilise la souris pour contrôler une caméra, il faut bloquer le curseur au centre de l'écran
    // Sinon, on cesse d'obtenir des déplacements de souris quand on arrive sur les bords
    if(controlType == ControlType.Mouse)
    {
      Cursor.lockState = CursorLockMode.Locked;
    }
  }
  
  void Update()
  {
    switch(controlType)
    {
      case ControlType.GamepadUnity:
        yawInput      = Input.GetAxis("RHorizontal") * sensitivity;
        pitchInput    = Input.GetAxis("RVertical") * sensitivity;
      break;

      case ControlType.Mouse:
        yawInput      = Input.GetAxis("Mouse X") * sensitivity; // Mouse X est la quantité de déplacement effectuée par la souris depuis la dernière frame
        pitchInput    = Input.GetAxis("Mouse Y") * sensitivity;
      break;

      case ControlType.GamepadXInput:
        PlayerIndex playerIndex = (PlayerIndex)0;
        GamePadState gamepadState = GamePad.GetState(playerIndex);
        if (gamepadState.IsConnected)
        {
          yawInput    = gamepadState.ThumbSticks.Right.X * sensitivity;
          pitchInput  = gamepadState.ThumbSticks.Right.Y * sensitivity;
        }
      break;
    }    
  }
}
