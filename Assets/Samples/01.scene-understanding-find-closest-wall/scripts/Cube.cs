using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// This class is attached to the "Cube" prefab and handles poke and grab interactions with the cube.
/// </summary>
public class Cube : MonoBehaviour, IMixedRealityPointerHandler
{
    /// <summary>
    /// When activated, this property will be set to the game object that is the scene understanding manager in order to access the script components attached to it.
    /// </summary>
    [Tooltip("Scene Understanding Manager game object.")]
    public GameObject SceneUnderstandingManager;

    /// <summary>
    /// Material used when the cube is being touched (poked).
    /// </summary>
    [Tooltip("Material to render on cube when it's being touched (poked).")]
    public Material TouchedMaterial;

    /// <summary>
    /// Material used when the cube is being grabbed.
    /// </summary>
    [Tooltip("Material to render on cube when it's being grabbed.")]
    public Material GrabbedMaterial;

    /// <summary>
    /// Material used when the cube is at rest (not being touched).
    /// </summary>
    [Tooltip("Material to render on cube when it's not being touched or grabbed.")]
    public Material NotBeingTouchedMaterial;

    /// <summary>
    /// The speed at which the cube should move towards the target wall.
    /// </summary>
    [Tooltip("The speed at which the cube should move towards the target wall.")]
    public float speed = 1.0f;

    private float Timer = 0f;
    private Transform target;
    private bool inert;
    private bool held;

    void Start()
    {
        SceneUnderstandingManager = GameObject.Find("SceneUnderstandingManager");
        TouchedMaterial = TouchedMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Standard_GlowingCyan") : TouchedMaterial;
        GrabbedMaterial = GrabbedMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Standard_GlowingOrange") : GrabbedMaterial;
        NotBeingTouchedMaterial = NotBeingTouchedMaterial == null ? Resources.Load<Material>("Assets/Samples/01.scene-understanding-find-closest-wall/materials/MRTK_Standard_TransparentCyan") : NotBeingTouchedMaterial;
    }

    void Update()
    {
        if (held)
        {
            target = null;

            Timer += Time.deltaTime;

            if (Timer > 0f)
            {
                Timer += Time.deltaTime;

                // Leave the cube marked as held for 1.5 seconds. This prevents accidental tapping of the cube after grabbing and releasing it.
                if (Timer >= 1.5f)
                {
                    held = false;
                    Timer = 0f;
                }
            }

            return;
        }

        if (target != null)
        {
            float step = speed * Time.deltaTime; // Calculate the distance to move.
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
            

            // Check if the position of the cube and the target destination are approximately equal (stop before having the cube enter half way into the wall).
            if (Vector3.Distance(transform.position, target.position) <= (transform.localScale.z / 2))
            {
                // Rotate the cube to align with the wall.
                transform.rotation = target.rotation;
                target = null;
                inert = true; // Mark the cube as inert after it arrives at the target. It will only become not inert after it's grabbed and moved.
            }
        }
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is PokePointer)
        {
            if (!held && !inert)
            {
                eventData.Pointer.Result.CurrentPointerTarget.GetComponent<Renderer>().material = NotBeingTouchedMaterial;
                target = SceneUnderstandingManager.GetComponent<SceneUnderstandingFindSceneObject>().ClosestWall();
            }
        }
    }

    void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is SpherePointer pointer)
        {
            eventData.Pointer.Result.CurrentPointerTarget.GetComponent<Renderer>().material = GrabbedMaterial;
            Debug.Log($"You started grabbing the cube!");
            transform.parent = pointer.transform;
            held = true;
        }

        if (eventData.Pointer is PokePointer)
        {
            if (!held && !inert)
            {
                eventData.Pointer.Result.CurrentPointerTarget.GetComponent<Renderer>().material = TouchedMaterial;
                Debug.Log($"You're touching the cube.");
            }
        }
    }

    void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is SpherePointer pointer)
        {
            Debug.Log($"You're moving the cube!");
            held = true;
            inert = false;
            transform.parent = pointer.transform;
        }
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is SpherePointer)
        {
            eventData.Pointer.Result.CurrentPointerTarget.GetComponent<Renderer>().material = NotBeingTouchedMaterial;
            Debug.Log($"You stopped grabbing the cube!");
            transform.parent = null;
        }

        if (eventData.Pointer is PokePointer)
        {
            if (!held && !inert)
            {
                eventData.Pointer.Result.CurrentPointerTarget.GetComponent<Renderer>().material = NotBeingTouchedMaterial;
                Debug.Log($"You stopped touching the cube.");
            }
        }
    }
}