using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class NoClipGrabStable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private Rigidbody rb;
    private Vector3 lastSafePosition;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        currentInteractor = args.interactorObject;
        lastSafePosition = rb.position;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        currentInteractor = null;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        base.ProcessInteractable(phase);

        if (currentInteractor == null)
            return;

        if (phase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            return;

        // Position cible = interactor + attach transform
        Transform attach = GetAttachTransform(currentInteractor);
        Vector3 desiredPos = attach.position;

        Vector3 displacement = desiredPos - rb.position;
        float distance = displacement.magnitude;

        if (distance < 0.0001f)
            return;

        Vector3 direction = displacement.normalized;

        // On fait un SweepTest pour voir si le mouvement collide
        if (rb.SweepTest(direction, out RaycastHit hit, distance))
        {
            // Collision → on bloque l'objet
            rb.MovePosition(lastSafePosition);
        }
        else
        {
            // Pas de collision → on laisse bouger
            rb.MovePosition(desiredPos);
            lastSafePosition = desiredPos;
        }
    }
}
