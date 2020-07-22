using Microsoft.MixedReality.SceneUnderstanding.Samples.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToggleWallsButton : MonoBehaviour
{
    /// <summary>
    /// Scene Understanding find scene object component.
    /// </summary>
    [Tooltip("Scene Understanding Manager find scene object component.")]
    public SceneUnderstandingFindSceneObject SUFindSceneObject = null;
    public TextMeshPro ButtonText;
    public bool WallsVisible;

    private void Start()
    {
        SUFindSceneObject = SUFindSceneObject == null ? gameObject.GetComponent<SceneUnderstandingFindSceneObject>() : SUFindSceneObject;
    }
    public void ToggleWalls()
    {
        if (SUFindSceneObject.Walls.Count == 0)
        {
            SUFindSceneObject.FindWalls();
        }

        if (!WallsVisible)
        {
            // Iterate through the list of detected walls, making them all visible.
            foreach (GameObject wall in SUFindSceneObject.Walls)
            {
                wall.SetActive(true);
            }

            ButtonText.text = "Hide Walls";
            WallsVisible = true;
        }
        else
        {
            // Iterate through the list of detected walls, making them all invisible.
            foreach (GameObject wall in SUFindSceneObject.Walls)
            {
                wall.SetActive(false);
            }

            ButtonText.text = "Show Walls";
            WallsVisible = false;
        }
    }
}
