using UnityEngine;

public class HealthSystem
{
    private const int _maxHealth = 100;
    private ulong _networkObjectID;
    public int CurrentHealth { get; private set; }

    public HealthSystem(ulong networkObjectID)
    {
        CurrentHealth = _maxHealth;
        _networkObjectID = networkObjectID;
    }

    public void DecreaseHealth(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            GameManager.Instance.EventService.InvokeOnPlayerDiedEvent(_networkObjectID);
        }
    }
}
