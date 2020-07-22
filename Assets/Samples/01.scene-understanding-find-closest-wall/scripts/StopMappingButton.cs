using Microsoft.MixedReality.SceneUnderstanding.Samples.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StopMappingButton : MonoBehaviour
{
    /// <summary>
    /// Scene Understanding data provider component.
    /// </summary>
    [Tooltip("Scene Understanding data provider component.")]
    public SceneUnderstandingDataProvider SUDataProvider = null;

    /// <summary>
    /// Scene Understanding display manager component.
    /// </summary>
    [Tooltip("Scene Understanding display manager component.")]
    public SceneUnderstandingDisplayManager SUDisplayManager = null;

    /// <summary>
    /// Scene Understanding find scene object component.
    /// </summary>
    [Tooltip("Scene Understanding Manager find scene object component.")]
    public SceneUnderstandingFindSceneObject SUFindSceneObject = null;

    /// <summary>
    /// Material used to let the SUDisplay Manager render the scene objects, but have them be invisible.
    /// </summary>
    [Tooltip("Invisible material which will prevent scene objects from appearing on the device.")]
    public Material SceneObjectInvisibleMaterial;

    /// <summary>
    /// Material used to let the SUDisplay Manager render the scene objects, but have them be invisible.
    /// </summary>
    [Tooltip("Visible wirefrake material which render the scene objects visible.")]
    public Material SceneObjectWireframeMaterial;

    public GameObject ButtonCollection1;
    public GameObject ToggleWallsButton;
    public GameObject SpawnNewCubeButton;
    public TextMeshPro Instructions;

    private float Timer = 0f;

    private void Start()
    {
        SUDataProvider = SUDataProvider == null ? gameObject.GetComponent<SceneUnderstandingDataProvider>() : SUDataProvider;
        SUDisplayManager = SUDisplayManager == null ? gameObject.GetComponent<SceneUnderstandingDisplayManager>() : SUDisplayManager;
        SUFindSceneObject = SUFindSceneObject == null ? gameObject.GetComponent<SceneUnderstandingFindSceneObject>() : SUFindSceneObject;
        SceneObjectInvisibleMaterial = SceneObjectInvisibleMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Invisible") : SceneObjectInvisibleMaterial;
        SceneObjectWireframeMaterial = SceneObjectWireframeMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Wireframe") : SceneObjectWireframeMaterial;
    }

    private void Update()
    {
        if (Timer > 0f)
        {
            Timer += Time.deltaTime;

            // If the button this script is attached to has been clicked wait for half a second before proceeding further.
            // This transition delay allows the MRTK_ButtonUnpress audio clip to finish playing and for the scene understanding display manager to finish displaying the scene objects.
            if (Timer >= 0.5f)
            {
                // DEV NOTE: It would be much more ideal if the "_displayInProgress" property in the Scene Understanding Display Manager class were public rather than private.
                // Since it is not and part of the point of this sample extension is to not make any changes to the SU SDK, we'll instead assume that half a second is sufficient time for the scene objects to be displayed.
                if (SUFindSceneObject.Walls.Count == 0)
                {
                    SUFindSceneObject.FindWalls();
                }

                // Disable each of the wall scene objects and then reset their material back to the visible wireframe.
                foreach (GameObject wall in SUFindSceneObject.Walls)
                {
                    wall.SetActive(false);
                    wall.GetComponent<Renderer>().material = SceneObjectWireframeMaterial;
                }

                ToggleWallsButton.SetActive(true);
                SpawnNewCubeButton.SetActive(true);
                Instructions.text = "Tap the button below to spawn a new cube. If you poke the cube with your index finger, it will float to the wall that is closest to you.";
                Timer = 0f;
                ButtonCollection1.SetActive(false);
            }
        }
    }

    public void OnClick()
    {
        // Remove the world mesh from the scene.
        Destroy(GameObject.Find("World"));

        SUDisplayManager.SceneObjectWireframeMaterial = SceneObjectInvisibleMaterial;

        // Adjust settings and stop spatially mapping the room.
        SUDisplayManager.RenderWorldMesh = false;
        SUDisplayManager.RenderSceneObjects = true;
        SUDisplayManager.StopAutoRefresh();

        // Have Scene Understanding Display Manager create the game objects for each scene object that is identified in the scene data.
        SUDisplayManager.StartDisplay();

        Timer += Time.deltaTime;
    }
}
