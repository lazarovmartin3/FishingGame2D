using UnityEngine;

public class Hook : MonoBehaviour
{
    private GameObject hookedFish = null;

    public void Retracted()
    {
        if (hookedFish != null)
        {
            //hookedFish.GetComponent<Fish>().DestroyServerRpc();
            hookedFish = null;
        }
    }

    public void ReleaseFish()
    {
        hookedFish.GetComponent<Fish>().Escape();
        hookedFish = null;
    }

    public GameObject IsFishing()
    {
        return hookedFish;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fish"))
        {
            // Handle catching the fish
            WebSocketManager.instance.CatchFish(other.gameObject.GetComponent<Fish>().id);
            if (hookedFish)
            {
                hookedFish.GetComponent<Fish>().Escape();
                return;
            }
            hookedFish = other.gameObject;
            if (hookedFish.GetComponent<Fish>().TryHook())
            {
                hookedFish.GetComponent<Fish>().Hooked(this.transform);
            }
            else
            {
                hookedFish = null;
            }
        }
    }
}