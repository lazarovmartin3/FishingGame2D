using Unity.Netcode;
using UnityEngine;

public class FishingRodController : NetworkBehaviour
{
    public int playerId;
    public bool isLocalPlayer;

    [SerializeField] private Transform fishingRod;
    [SerializeField] private Transform hook;
    [SerializeField] private LineRenderer lineRenderer;

    private NetworkVariable<bool> isCasting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isRetracting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isThrowing = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> hookSpeed = new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector2> hookTargetPos = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> colliderEnabled = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<Vector3> lineStartPoint = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> lineEndPoint = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        transform.position = GameManager.instance.GetPlayerPosition().position;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            lineRenderer.SetPosition(0, lineStartPoint.Value);
            lineRenderer.SetPosition(1, lineEndPoint.Value);
            hook.GetComponent<BoxCollider2D>().enabled = colliderEnabled.Value;
            return;
        }

        if (Input.GetMouseButtonDown(0) && !isCasting.Value && !isRetracting.Value)
        {
            StartDrawingLine();
            DrawLine();
        }
        else if (Input.GetMouseButtonDown(1) && isCasting.Value && !isRetracting.Value)
        {
            TryRetract();
        }

        if (isRetracting.Value)
        {
            RetractHook();
        }

        if (isThrowing.Value)
        {
            ThrowHook();
        }
    }

    private void StartDrawingLine()
    {
        isCasting.Value = true;
        lineRenderer.enabled = true;
        lineStartPoint.Value = fishingRod.position;
        lineRenderer.SetPosition(0, lineStartPoint.Value);
        lineRenderer.SetPosition(1, lineEndPoint.Value);
    }

    private void DrawLine()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.y -= 0.4f;
        hookTargetPos.Value = mousePosition;
        mousePosition.z = -10;
        lineEndPoint.Value = mousePosition;
        lineRenderer.SetPosition(1, lineEndPoint.Value);
        isThrowing.Value = true;
    }

    private void TryRetract()
    {
        GameObject fish = hook.GetComponent<Hook>().IsFishing();
        if (fish)//if there is a fish , try catch it
        {
            if (!FishingManager.instance.AttemptToCatchFish(fish.GetComponent<Fish>().GetFishType()))
                hook.GetComponent<Hook>().ReleaseFish();
            else
                hook.GetComponent<Hook>().Retracted();
        }
        isRetracting.Value = true;
    }

    private void RetractHook()
    {
        colliderEnabled.Value = false;
        hook.GetComponent<BoxCollider2D>().enabled = colliderEnabled.Value;
        hook.position = Vector3.MoveTowards(hook.position, fishingRod.position, hookSpeed.Value * Time.deltaTime);
        Vector3 pos = hook.position;
        pos.z = -10;
        lineEndPoint.Value = pos;
        lineRenderer.SetPosition(1, lineEndPoint.Value);

        if (Vector3.Distance(hook.position, fishingRod.position) < 0.05f)
        {
            // Hook has reached the rod, stop retracting
            isCasting.Value = false;
            isRetracting.Value = false;
            lineRenderer.enabled = false;
        }
    }

    private void ThrowHook()
    {
        colliderEnabled.Value = false;
        hook.GetComponent<BoxCollider2D>().enabled = colliderEnabled.Value;
        hook.position = Vector3.MoveTowards(hook.position, hookTargetPos.Value, hookSpeed.Value * Time.deltaTime);
        Vector3 pos = hook.position;
        pos.z = -10;
        lineEndPoint.Value = pos;
        lineRenderer.SetPosition(1, lineEndPoint.Value);
        lineRenderer.enabled = true;
        if (Vector3.Distance(hook.position, hookTargetPos.Value) < 0.05f)
        {
            isThrowing.Value = false;
            colliderEnabled.Value = true;
            hook.GetComponent<BoxCollider2D>().enabled = colliderEnabled.Value;
        }
    }
}