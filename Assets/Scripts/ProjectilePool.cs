using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : GenericObjectPool<ProjectileController>
{
    private ProjectileController _projectileController;
    
    public ProjectilePool(ProjectileController projectileController)
    {
        _projectileController = projectileController;
        maxObjectsInPool = 20;
    }
    
    public ProjectileController GetProjectileFromPool()
    {
        return GetObjectFromPool();
    }

    protected override ProjectileController CreateNewObject()
    {
        return GameObject.Instantiate(_projectileController);
    }
}
