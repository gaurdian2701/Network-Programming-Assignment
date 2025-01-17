
using UnityEngine.UIElements;

public class EventService
{
    public delegate void ZeroParamDelegate();
    public delegate void OneParamDelegate(ulong value);

    public event OneParamDelegate OnPlayerDiedEvent;
    public void InvokeOnPlayerDiedEvent(ulong networkObjectID) => OnPlayerDiedEvent?.Invoke(networkObjectID);
}
