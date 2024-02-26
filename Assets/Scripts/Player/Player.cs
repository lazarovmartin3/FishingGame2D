using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform fishingRod;
    [SerializeField] private Transform hook;
    [SerializeField] private LineRenderer lineRenderer;
    private int id;
    private float hookSpeed = 5f;
    private Vector3 hookTargetPos;
    private Vector3 hookInitPos;
    private bool isThrowing;
    private bool isCasting;
    private bool isRetracting;

    public bool DisabledInput;

    public void SetID(int id)
    {
        this.id = id;
    }

    public int GetID()
    {
        return id;
    }

    private void Update()
    {
        if (!DisabledInput && Input.GetMouseButtonDown(0) && !isCasting && !isRetracting)
        {
            hookTargetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            WebSocketManager.instance.SendMessageToServer(CreateJSON("ThrowFishingRod", hookTargetPos, fishingRod.position, true, false, true));
        }
        else if (!DisabledInput && Input.GetMouseButtonDown(1) && isCasting && !isRetracting)
        {
            TryRetract();
        }

        if (isThrowing)
        {
            ThrowHook();
        }

        if (isRetracting)
        {
            RetractHook();
        }
    }

    public IEnumerator UpdatePlayerData(WebSocketManager.ServerData data)
    {
        isCasting = data.isCasting;
        isRetracting = data.isRetracting;
        hookTargetPos = data.targetPosition;
        hookInitPos = data.hookInitPosition;
        isThrowing = data.isThrowing;

        if (isCasting && !isRetracting)
        {
            lineRenderer.SetPosition(0, hookInitPos);
            lineRenderer.SetPosition(1, hookTargetPos);
        }

        yield return null;
    }

    private void ThrowHook()
    {
        hook.GetComponent<BoxCollider2D>().enabled = false;
        hook.position = Vector3.MoveTowards(hook.position, hookTargetPos, hookSpeed * Time.deltaTime);
        Vector3 lineTarget = hook.position;
        lineTarget.z = -10;
        lineRenderer.SetPosition(1, lineTarget);
        lineRenderer.enabled = true;
        if (Vector3.Distance(hook.position, hookTargetPos) < 0.05f)
        {
            //isThrowing = false;
            hook.GetComponent<BoxCollider2D>().enabled = true;
            hook.position = new Vector3(hook.position.x, hook.position.y, 0);
            WebSocketManager.instance.SendMessageToServer(CreateJSON("ThrowFishingRod", hookTargetPos, fishingRod.position, true, false, false));
        }
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
        WebSocketManager.instance.SendMessageToServer(CreateJSON("ThrowFishingRod", fishingRod.position, fishingRod.position, false, true, false));
    }

    private void RetractHook()
    {
        hook.GetComponent<BoxCollider2D>().enabled = false;
        hook.position = Vector3.MoveTowards(hook.position, hookTargetPos, hookSpeed * Time.deltaTime);
        Vector3 lineTarget = hookTargetPos;
        lineTarget.z = -10;
        lineRenderer.SetPosition(1, lineTarget);

        if (Vector3.Distance(hook.position, hookTargetPos) < 0.05f)
        {
            hook.GetComponent<BoxCollider2D>().enabled = true;
            hook.position = new Vector3(hook.position.x, hook.position.y, 0);
            WebSocketManager.instance.SendMessageToServer(CreateJSON("ThrowFishingRod", hookTargetPos, fishingRod.position, false, false, false));
        }
    }

    private string CreateJSON(string action, Vector3 hookTargetPos, Vector3 hookInitPos, bool isCasting, bool isRetracting, bool isThrowing)
    {
        WebSocketManager.ServerData data = new WebSocketManager.ServerData();
        data.playerId = id;
        data.targetPosition = hookTargetPos;
        data.hookInitPosition = hookInitPos;
        data.action = action;
        data.gameId = GameManager.instance.GameID;
        data.isCasting = isCasting;
        data.isRetracting = isRetracting;
        data.isThrowing = isThrowing;
        string json = JsonUtility.ToJson(data);
        return json;
    }

}
