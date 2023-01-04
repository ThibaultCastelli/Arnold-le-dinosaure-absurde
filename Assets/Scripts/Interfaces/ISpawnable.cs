using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnable
{
    public void Spawn(Vector3 position, Quaternion rotation);

    public void Despawn();
}
