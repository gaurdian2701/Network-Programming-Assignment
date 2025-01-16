using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private float _projectileSpeed;
    private Vector3 _enemyTarget;
    private void Update()
    {
        if (Vector2.Distance(_enemyTarget, transform.position) < 0.1f)
        {
            Debug.Log("hit");
            Destroy(gameObject);
        }
        Vector3 newPos = _projectileSpeed * Time.deltaTime * transform.right;
        newPos.z = 0;
        transform.position += newPos;
    }
    public void SetTarget(Vector3 target) => _enemyTarget = target;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
            return;
        Destroy(gameObject);
    }
}
