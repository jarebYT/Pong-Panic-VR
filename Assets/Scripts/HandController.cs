using UnityEngine;
using UnityEngine.XR;

public class HandController : MonoBehaviour
{
    public SkinnedMeshRenderer handMesh;
    public int indexFingerBlendShape = 0; // numéro du blendshape pour l'index
    public int middleFingerBlendShape = 1; // numéro du blendshape pour le majeur, etc.

    private InputDevice targetDevice;

    void Start()
    {
        var handCharacteristics = (gameObject.name.Contains("Left")) ? 
                                  InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller : 
                                  InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(handCharacteristics, devices);

        if (devices.Count > 0)
            targetDevice = devices[0];
    }

    void Update()
    {
        if (targetDevice.isValid)
        {
            float triggerValue;
            if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue))
            {
                handMesh.SetBlendShapeWeight(indexFingerBlendShape, triggerValue * 100f);
            }

            float gripValue;
            if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out gripValue))
            {
                handMesh.SetBlendShapeWeight(middleFingerBlendShape, gripValue * 100f);
            }
        }
    }
}
