using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnable
{
    /// <summary>
    /// Make the object spawn.
    /// </summary>
    /// <param name="position">Spawn position.</param>
    /// <param name="rotation">Spawn rotation.</param>
    public void Spawn(Vector3 position, Quaternion rotation);

    /// <summary>
    /// Make the object despawn.
    /// </summary>
    public void Despawn();
}
