using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

/// <summary>
/// Diagnostic tools to check for common VR setup issues
/// </summary>
public class DiagnosticTools : MonoBehaviour
{
    [ContextMenu("Check XR Setup")]
    public void CheckXRSetup()
    {
        Debug.Log("========== XR DIAGNOSTIC REPORT ==========");
        
        // Check for XROrigin components (using reflection as fallback)
        try
        {
            var xrOriginType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XROrigin, Unity.XR.Interaction.Toolkit");
            if (xrOriginType != null)
            {
                var xrOrigins = FindObjectsByType(xrOriginType, FindObjectsSortMode.None);
                Debug.Log($"[XR Origins Found]: {xrOrigins.Length}");
                if (xrOrigins.Length > 1)
                {
                    Debug.LogWarning("⚠️ MULTIPLE XR ORIGINS DETECTED! There should be only ONE.");
                    foreach (var origin in xrOrigins)
                    {
                        var go = (origin as MonoBehaviour)?.gameObject ?? (origin as Component)?.gameObject;
                        if (go != null)
                        {
                            Debug.LogWarning($"  - {go.name} at {go.transform.position}");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("[XR Origins]: Could not find XROrigin type in this Unity version");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[XR Origins]: Error checking XROrigin - {e.Message}");
        }

        // Check for multiple pairs of controllers
        var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        int leftCount = 0, rightCount = 0;
        foreach (var t in allTransforms)
        {
            if (t.name.Contains("Left") && (t.name.Contains("Controller") || t.name.Contains("Hand")))
                leftCount++;
            if (t.name.Contains("Right") && (t.name.Contains("Controller") || t.name.Contains("Hand")))
                rightCount++;
        }
        Debug.Log($"[Controllers Found]: Left={leftCount}, Right={rightCount}");

        // Check Players
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        Debug.Log($"[Players Found]: {players.Length}");
        foreach (var player in players)
        {
            Debug.Log($"  - {player.gameObject.name}");
        }

        // Check PingPongManager
        var manager = FindFirstObjectByType<PingPongManager>();
        if (manager != null)
        {
            Debug.Log($"[PingPongManager]: Found");
        }
        else
        {
            Debug.LogError("❌ PingPongManager NOT FOUND!");
        }

        Debug.Log("========================================");
    }
}
