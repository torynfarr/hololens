using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class handles interaction with the "Spawn New Cube" button, destroying any exisiting cubes and instantiating a new cube positioned above the floating near menu.
/// </summary>
public class SpawnCubeButton : MonoBehaviour
{
    public GameObject CubePrefab;
    public GameObject NearMenu;

    public void OnClick()
    {
        // Destroy the old cube, if it exists.
        GameObject oldCube = GameObject.FindGameObjectWithTag("Cube");
        
        if (oldCube != null)
        {
            Destroy(oldCube);
        }

        GameObject cube = Instantiate(CubePrefab);
        cube.transform.position = new Vector3(NearMenu.transform.position.x, NearMenu.transform.position.y + 0.1875f, NearMenu.transform.position.z);
        cube.transform.rotation = new Quaternion(0.0f, Camera.main.transform.rotation.x + 45.0f, 0.0f, 0.0f);
    }
}
