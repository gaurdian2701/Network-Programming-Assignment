using System;
using Unity.Netcode;
using UnityEngine;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private float _projectileSpeed;
    private void Update()
    {
        transform.position += _projectileSpeed * Time.deltaTime * transform.right;
    }
}
