using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[DisallowMultipleComponent]
public class VRHandFingerController : MonoBehaviour
{
    public enum HandSide { Right, Left }
    public HandSide side = HandSide.Right;
    public XRNode xrNode = XRNode.RightHand;

    [Header("Optional: drag phalange transforms (prox -> tip). Leave empty to auto-find by name)")]
    public Transform[] thumb;
    public Transform[] index;
    public Transform[] middle;
    public Transform[] ring;
    public Transform[] pinky;

    [Header("Settings")]
    public float maxCurlAngle = 85f;
    public float smoothSpeed = 10f;

    // internals
    private Dictionary<Transform, Quaternion> restLocal = new Dictionary<Transform, Quaternion>();
    private float curIndex = 0f, curGrip = 0f, curThumb = 0f;

    private Transform rootOfPrefab;

    void Awake()
    {
        rootOfPrefab = transform; // the prefab root the script is on
    }

    void Start()
    {
        // If arrays empty, try to auto-find bones by common naming
        string suffix = (side == HandSide.Right) ? ".R" : ".L";
        AutoAssignIfEmpty("thumb", suffix, ref thumb, 3);
        AutoAssignIfEmpty("index", suffix, ref index, 3);
        AutoAssignIfEmpty("middle", suffix, ref middle, 3);
        AutoAssignIfEmpty("ring", suffix, ref ring, 3);
        AutoAssignIfEmpty("pinky", suffix, ref pinky, 3);

        StoreRest(thumb);
        StoreRest(index);
        StoreRest(middle);
        StoreRest(ring);
        StoreRest(pinky);

        // set XRNode if not set by inspector
        xrNode = (side == HandSide.Right) ? XRNode.RightHand : XRNode.LeftHand;
    }

    void AutoAssignIfEmpty(string fingerBaseName, string suffix, ref Transform[] arr, int phalanges)
    {
        if (arr != null && arr.Length > 0) return;

        List<Transform> found = new List<Transform>();
        for (int i = 1; i <= phalanges; i++)
        {
            // try several name variants
            string[] tries = new string[] {
                $"{fingerBaseName}_{i}{suffix}",
                $"{fingerBaseName}{i}{suffix}",
                $"{fingerBaseName}_{i}",
                $"{fingerBaseName}{i}",
                $"{fingerBaseName}_{i.ToString()}",
                $"{fingerBaseName}{i}"
            };

            Transform t = FindChildByNameVariants(rootOfPrefab, tries);
            if (t != null) found.Add(t);
        }

        if (found.Count > 0)
            arr = found.ToArray();
    }

    Transform FindChildByNameVariants(Transform root, string[] names)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            foreach (string n in names)
            {
                if (string.IsNullOrEmpty(n)) continue;
                if (t.name == n || t.name.Contains(n))
                    return t;
            }
        }
        return null;
    }

    void StoreRest(Transform[] arr)
    {
        if (arr == null) return;
        foreach (var t in arr)
            if (t != null && !restLocal.ContainsKey(t))
                restLocal[t] = t.localRotation;
    }

    void Update()
    {
        var device = InputDevices.GetDeviceAtXRNode(xrNode);
        device.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal);
        device.TryGetFeatureValue(CommonUsages.grip, out float gripVal);
        bool thumbTouch = false;
        device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out thumbTouch);

        float targetIndex = triggerVal;
        float targetGrip = gripVal;
        float targetThumb = thumbTouch ? 1f : 0f;

        curIndex = Mathf.Lerp(curIndex, targetIndex, Time.deltaTime * smoothSpeed);
        curGrip = Mathf.Lerp(curGrip, targetGrip, Time.deltaTime * smoothSpeed);
        curThumb = Mathf.Lerp(curThumb, targetThumb, Time.deltaTime * smoothSpeed);

        ApplyCurl(index, curIndex);
        ApplyCurl(middle, curGrip);
        ApplyCurl(ring, curGrip);
        ApplyCurl(pinky, curGrip);
        ApplyCurl(thumb, curThumb);
    }

    // remplace la ApplyCurl précédente par celle-ci
    void ApplyCurl(Transform[] bones, float t)
    {
        if (bones == null || bones.Length == 0) return;

        // ex: bones.Length == 2 -> factors = [0.6, 1.0]
        // ex: bones.Length == 3 -> factors = [0.4, 0.75, 1.0]
        for (int i = 0; i < bones.Length; i++)
        {
            var b = bones[i];
            if (b == null || !restLocal.ContainsKey(b)) continue;

            // factor progressif : on veut que la dernière phalange plie le plus
            float factor = (i + 1f) / bones.Length; // 0..1
            // on remappe pour favoriser la dernière : pow pour courbe non linéaire
            factor = Mathf.Pow(factor, 0.8f); // 0.8 => plus linéaire; 0.6 => accentue la dernière

            float angle = maxCurlAngle * factor * t;

            // Ajuste l'axe si besoin (X/Y/Z ou signe négatif)
            Quaternion target = restLocal[b] * Quaternion.Euler(angle, 0f, 0f);

            b.localRotation = Quaternion.Slerp(b.localRotation, target, Time.deltaTime * 20f);
        }
    }
}
