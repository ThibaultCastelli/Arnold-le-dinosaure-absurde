using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentCactus : MonoBehaviour, ISpawnable
{
    private List<Vector3> _childPosition = new List<Vector3>();
    private List<Quaternion> _childRotation = new List<Quaternion>();

    private void Awake()
    {
        // Get the initial position and rotation of all the cactus child
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            _childPosition.Add(childTransform.position);
            _childRotation.Add(childTransform.rotation);
        }
    }

    public void Despawn()
    {
        // Despawn all child cactus
        //for (int i = 0; i < transform.childCount; i++)
        //{
        //    transform.GetChild(i).GetComponent<Cactus>().Despawn();
        //}

        // Despawn parent cactus
        gameObject.SetActive(false);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        // Spawn parent cactus
        gameObject.SetActive(true);

        // For each cactus child
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            // Spawn the child cactus
            Cactus childCactus = childTransform.GetComponent<Cactus>();
            childCactus.Spawn(_childPosition[i], _childRotation[i]);

            // If the speed of the cactus is different than the global speed at spawn, update it
            AutoSpeedHorizontal childScroll = childTransform.GetComponent<AutoSpeedHorizontal>();
            CactusSpawner cactusSpawner = transform.parent.GetComponent<CactusSpawner>();
            if (cactusSpawner.CurrentCactusSpeed > childScroll.Speed)
            {
                childScroll.Speed = cactusSpawner.CurrentCactusSpeed;
            }
        }
    }

    /// <summary>
    /// Check if all its children are despawn, if so, despawn itself.
    /// </summary>
    public void CheckDespawn()
    {
        bool allChildDespawn = true;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                allChildDespawn = false;
            }
        }

        if (allChildDespawn)
        {
            Despawn();
        }
    }
}
