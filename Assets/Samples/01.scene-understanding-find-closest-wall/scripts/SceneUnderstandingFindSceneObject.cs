using Microsoft.MixedReality.SceneUnderstanding.Samples.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SceneUnderstandingDataProvider))]
public class SceneUnderstandingFindSceneObject : MonoBehaviour
{
    /// <summary>
    /// Scene Understanding data provider component.
    /// </summary>
    [Tooltip("Scene Understanding data provider component.")]
    public SceneUnderstandingDataProvider SUDataProvider = null;

    /// <summary>
    /// Wireframe material to display on the walls when running on PC.
    /// </summary>
    [Tooltip("Wireframe material to display on the walls when running on PC. Not used when running on the device.")]
    public Material SceneObjectWireframeMaterial;

    /// <summary>
    /// Material used to highlight the closest wall.
    /// </summary>
    [Tooltip("Material to render on the closest wall.")]
    public Material SceneObjectClosestWallMaterial;

    /// <summary>
    /// GameObject that will be the parent of all Scene Understanding related game objects.
    /// </summary>
    [Tooltip("GameObject that will be the parent of all Scene Understanding related game objects.")]
    public GameObject SceneRoot;

    /// <summary>
    /// A list of walls identified and created as game objects by the SUDisplayManager.
    /// </summary>
    [Tooltip("A list of walls indentified via scene understanding.")]
    public List<GameObject> Walls = new List<GameObject>();

    /// <summary>
    /// GameObject that will be the parent of all Scene Understanding related game objects.
    /// </summary>
    [Tooltip("Toggles highlighting the closest detected wall by changing its material to 'SceneObjectClosestWallMaterial'")]
    public bool HighlightClosestWall;

    /// <summary>
    /// GameObject that will be the parent of all Scene Understanding related game objects.
    /// </summary>
    [Tooltip("The Toggle Walls button, so that we can determine if we should show or hide walls.")]
    public ToggleWallsButton ToggleWallsButton;

    private GameObject ClosestWallGameObject;

    private void Start()
    {
        SUDataProvider = SUDataProvider == null ? gameObject.GetComponent<SceneUnderstandingDataProvider>() : SUDataProvider;
        ToggleWallsButton = ToggleWallsButton == null ? gameObject.GetComponent<ToggleWallsButton>() : ToggleWallsButton;
        SceneObjectWireframeMaterial = SceneObjectWireframeMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Wireframe") : SceneObjectWireframeMaterial;
        SceneObjectClosestWallMaterial = SceneObjectClosestWallMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Standard_Purple") : SceneObjectClosestWallMaterial;
    }

    /// <summary>
    /// Collects a list of walls that have been identified by the Scene Understanding Display Manager.
    /// </summary>
    public void FindWalls()
    {
        Walls.Clear();

        // Iterate through all objects in the scene root and find the child objects of any objects named "Wall"
        foreach (Transform child in SceneRoot.transform)
        {
            if (child.name == "Wall")
            {
                if (child.childCount == 1)
                {
                    if (child.GetChild(0).name == "Wall")
                    {
                        Walls.Add(child.GetChild(0).gameObject);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Used to identify the wall closest to the camera.
    /// </summary>
    /// <returns>The transform attached to the scene object of the closest wall.</returns>
    public Transform ClosestWall()
    {
        if (Walls.Count == 0)
        {
            FindWalls();
        }

        // If the walls are not being displayed disable the previously identified closest wall and reset the material to the wireframe. If they are being displayed, just reset the material to the wireframe.
        if (!ToggleWallsButton.WallsVisible)
        {
            if (ClosestWallGameObject != null)
            {
                ClosestWallGameObject.SetActive(false);
                ClosestWallGameObject.GetComponent<Renderer>().material = SceneObjectWireframeMaterial;
            }
        }
        else
        {
            if (ClosestWallGameObject != null)
            {
                ClosestWallGameObject.GetComponent<Renderer>().material = SceneObjectWireframeMaterial;
            }
        }

        if (Walls.Count > 0)
        {
            List<Tuple<int, float>> distances = new List<Tuple<int, float>>();

            foreach (GameObject wall in Walls)
            {
                distances.Add(Tuple.Create(Walls.IndexOf(wall), Vector3.Distance(Camera.main.transform.position, wall.GetComponent<Renderer>().bounds.center)));
            }

            distances.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            ClosestWallGameObject = Walls[distances.First().Item1];

            if (HighlightClosestWall)
            {
                ClosestWallGameObject.GetComponent<Renderer>().material = SceneObjectClosestWallMaterial;
            }

            ClosestWallGameObject.SetActive(true);

            Transform target = ClosestWallGameObject.transform;
            target.transform.position = new Vector3(ClosestWallGameObject.GetComponent<Renderer>().bounds.center.x, Camera.main.transform.position.y - 0.05f, ClosestWallGameObject.GetComponent<Renderer>().bounds.center.z);
            return target;
        }
        else
        {
            Debug.Log("No walls were found. Either something went wrong or you're outside?");
            return null;
        }
    }
}
