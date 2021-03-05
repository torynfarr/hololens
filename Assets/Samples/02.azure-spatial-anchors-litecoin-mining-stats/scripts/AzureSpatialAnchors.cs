using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR.WSA;

/// <summary>
/// This class is attached to the Main Hand Menu.
/// </summary>
public class AzureSpatialAnchors : MonoBehaviour
{
    /// <summary>
    /// The configuration object which contains the Azure Spatial Anchors settings.
    /// </summary>
    [Tooltip("Azure Spatial Anchor configuration settings.")]
    public SpatialAnchorConfig AzureSpatialAnchorsConfiguration;

    /// <summary>
    /// The Litecoin currency symbol prefab.
    /// </summary>
    [Tooltip("Litecoin currency symbol prefab.")]
    public GameObject LitecoinPrefab;

    /// <summary>
    /// The slate prefab.
    /// </summary>
    [Tooltip("Slate prefab.")]
    public GameObject SlatePrefab;

    /// <summary>
    /// The GameObject for the Currency Hand Menu.
    /// </summary>
    [Tooltip("Currency Hand Menu GameObject in the scene.")]
    public GameObject CurrencyHandMenu;

    /// <summary>
    /// The TextMeshPro in the Instructions NearMenu used to display text showing instructions and the current status.
    /// </summary>
    [Tooltip("TextMeshPro used to display instructions and status.")]
    public TextMeshPro Instructions;

    /// <summary>
    /// Indicates if an Azure Spatial Anchor has been created and the slate is active and visible in the scene.
    /// </summary>
    [Tooltip("Indicates if the slate is active in the scene.")]
    public bool IsSlateActive = false;

    /// <summary>
    /// A queue of actions that will be executed on the main thread.
    /// </summary>
    private readonly Queue<Action> queue = new Queue<Action>();

    /// <summary>
    /// The Azure Spatial Anchor session.
    /// </summary>
    protected CloudSpatialAnchorSession cloudSpatialAnchorSession;

    /// <summary>
    /// The Azure Spatial Anchor that we either placed and are saving or just located.
    /// </summary>
    protected CloudSpatialAnchor currentCloudAnchor;

    /// <summary>
    /// The ID of the Azure Spatial Anzhor that was saved. Use it to find the CloudSpatialAnchor again.
    /// </summary>
    protected string cloudSpatialAnchorId;

    /// <summary>
    /// Indicate if we are ready to save an anchor. We can save an anchor when value is greater than 1.
    /// </summary>
    protected float recommendedForCreate = 0;

    /// <summary>
    /// Creates an Azure Spatial Anchor at the position of the local World Anchor attached to the Litecoin symbol game object. Destroys the Litecoin symbol and instantiates the slate prefab.
    /// </summary>
    public void CreateAzureSpatialAnchor()
    {
#if UNITY_EDITOR
        if (!IsSlateActive)
        {
            GameObject litecoin = GameObject.FindGameObjectWithTag("Litecoin");
            GameObject slate = Instantiate(SlatePrefab, litecoin.transform.position, litecoin.transform.rotation);
            Destroy(litecoin);

            IsSlateActive = true;

            Instructions.text = "Azure Spatial Anchor sessions can't be started when running in the Unity Editor. Build and deploy this sample to a HoloLens device to be able to create and find Azure Spatial Anchors.\n\nLook at the slate and it will detect your gaze and update itself.";
        }
        else
        {
            Instructions.text = "Azure Spatial Anchor sessions can't be started when running in the Unity Editor. Build and deploy this sample to a HoloLens device to be able to create and find Azure Spatial Anchors.\n\nDelete the current anchor before creating a new one.\n\nLook at the slate and it will detect your gaze and update itself.";
        }

        return;
#else
        if (!IsSlateActive)
        {
            GameObject litecoin = GameObject.FindGameObjectWithTag("Litecoin");
            litecoin.AddComponent<WorldAnchor>();

            // Create the CloudSpatialAnchor.
            currentCloudAnchor = new CloudSpatialAnchor();

            // Set the LocalAnchor property of the CloudSpatialAnchor to the WorldAnchor component of the Litecoin symbol.
            WorldAnchor worldAnchor = litecoin.GetComponent<WorldAnchor>();

            if (worldAnchor == null)
            {
                throw new Exception("Couldn't get the local anchor pointer.");
            }

            // Save the CloudSpatialAnchor to the cloud.
            currentCloudAnchor.LocalAnchor = worldAnchor.GetNativeSpatialAnchorPtr();

            Task.Run(async () =>
            {
                // Wait for enough data about the environment.
                while (recommendedForCreate < 1.0F)
                {
                    await Task.Delay(330);
                }

                bool success = false;

                try
                {
                    QueueOnUpdate(() =>
                    {
                        Instructions.text = "Saving the Azure Spatial Anchor...";
                    });

                    await cloudSpatialAnchorSession.CreateAnchorAsync(currentCloudAnchor);
                    success = currentCloudAnchor != null;

                    if (success)
                    {
                        // Record the identifier of the Azure Spatial Anchor.
                        cloudSpatialAnchorId = currentCloudAnchor.Identifier;

                        QueueOnUpdate(() =>
                        {
                            Instantiate(SlatePrefab, litecoin.transform.position, litecoin.transform.rotation);
                            Destroy(litecoin);

                            IsSlateActive = true;

                            // Save the Azure Spatial Anchor ID so it persists even after the app is closed.
                            PlayerPrefs.SetString("Anchor ID", cloudSpatialAnchorId);

                            Instructions.text = "The Azure Spatial Anchor has been created!\n\nLook at the slate and it will detect your gaze and update itself.";
                        });
                    }
                    else
                    {
                        QueueOnUpdate(() =>
                        {
                            Instructions.text = "An error occured while creating the Azure Spatial Anchor.\n\nPlease try again.";
                        });
                    }
                }
                catch (Exception ex)
                {
                    QueueOnUpdate(() =>
                    {
                        Instructions.text = $"An error occured while creating the Azure Spatial Anchor.\n\nPlease try again.\n\nException: {ex.Message}";
                    });
                }
            });
        }
        else
        {
            Instructions.text = "An Azure Spatial Anchor already exists. Delete the current anchor before creating a new one.\n\nLook at the slate and it will detect your gaze and update itself.";
        }
#endif
    }

    /// <summary>
    /// Searches for an Azure Spatial Anchor if one exists.
    /// </summary>
    public async void FindAzureSpatialAnchorAsync()
    {
#if UNITY_EDITOR
        if (IsSlateActive)
        {
            Instructions.text = "Azure Spatial Anchor sessions can't be started when running in the Unity Editor. Build and deploy this sample to a HoloLens device to be able to create and find Azure Spatial Anchors.\n\nLook at the slate and it will detect your gaze and update itself.";
        }
        else
        {
            Instructions.text = "Azure Spatial Anchor sessions can't be started when running in the Unity Editor. Build and deploy this sample to a HoloLens device to be able to create and find Azure Spatial Anchors.\n\nUse the ray coming from either hand to position the Litecoin currency symbol. Air tap to place the symbol. This will be the location where your mining stats will be displayed.\n\nWhen you're ready, turn either hand so that your palm is facing you to access the hand menu. Tap the \"Create Spatial Anchor\" button to save the Azure Spatial Anchor.";
        }

        // This asynchronous task just prevents a warning due to this method being asynchronous and otherwise lacking an await operator.
        await Task.Delay(300);

        return;
#else
        // An Azure Spatial Anchor exists.
        if (!string.IsNullOrEmpty(cloudSpatialAnchorId))
        {
            if (IsSlateActive)
            {
                Destroy(GameObject.FindGameObjectWithTag("Slate"));
                IsSlateActive = false;
            }
            else
            {
                Destroy(GameObject.FindGameObjectWithTag("Litecoin"));
            }

            Instructions.text = "An Azure Spatial Anchor has been previously created and saved. Look around to find the the anchor.";
            await Task.Delay(3000);

            StopSession(() =>
            {
                StartSession();

                // Create a Watcher to look for the Azure Spatial Anchor.
                AnchorLocateCriteria criteria = new AnchorLocateCriteria
                {
                    Identifiers = new string[] { cloudSpatialAnchorId }
                };

                cloudSpatialAnchorSession.CreateWatcher(criteria);
            });
        }
        // An Azure Spatial Anchor does not exisit.
        else
        {
            // It shouldn't be possible for the slate to be active on HoloLens without an Azure Spatial Anchor having been created.
            if (IsSlateActive)
            {
                Destroy(GameObject.FindGameObjectWithTag("Slate"));
                IsSlateActive = false;
                DisplayLitecoinSymbol();
            }
            else
            {
                Instructions.text = "Before you can search for an Azure Spatial Anchor, you must first create one.";
                await Task.Delay(3000);

                // The Litecoin symbol should already be in the scene, so we won't instantiate a new one.
                Instructions.text = "Use the ray coming from either hand to position the Litecoin currency symbol. Air tap to place the symbol. This will be the location where your mining stats will be displayed.\n\nWhen you're ready, turn either hand so that your palm is facing you to access the hand menu. Tap the \"Create Spatial Anchor\" button to save the Azure Spatial Anchor.";
            }
        }
#endif
    }

    /// <summary>
    /// Deletes the currently loaded Azure Spatial Anchor. Destroys the slate game object and displays the Litecoin symbol.
    /// </summary>
    public async void DeleteAzureSpatialAnchorAsync()
    {
#if UNITY_EDITOR
        if (IsSlateActive)
        {
            Destroy(GameObject.FindGameObjectWithTag("Slate"));
            IsSlateActive = false;

            // This asynchronous task just prevents a warning due to this method being asynchronous and otherwise lacking an await operator.
            await Task.Delay(300);

            DisplayLitecoinSymbol();
        }

        return;
#else
        if (IsSlateActive && currentCloudAnchor != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("Slate"));
            IsSlateActive = false;

            Instructions.text = $"Deleting Azure Spatial Anchor...\n\nIdentifier: {currentCloudAnchor.Identifier}";
            await Task.Delay(3000);

            try
            {
                await cloudSpatialAnchorSession.DeleteAnchorAsync(currentCloudAnchor);
                Instructions.text = "Azure Spatial Anchor deleted.";
                await Task.Delay(3000);

            }
            catch (Exception ex)
            {
                Instructions.text = $"Error deleting Azure Spatial Anchor.\n\n{ex.Message}";
                await Task.Delay(3000);
            }

            cloudSpatialAnchorId = null;
            currentCloudAnchor = null;
            PlayerPrefs.SetString("Anchor ID", null);
            DisplayLitecoinSymbol();
        }
        else
        {
            Instructions.text = "No Azure Spatial Anchors exists. Please create an anchor before attempting to delete one.";
            await Task.Delay(3000);
            Instructions.text = "Use the ray coming from either hand to position the Litecoin currency symbol. Air tap to place the symbol. This will be the location where your mining stats will be displayed.\n\nWhen you're ready, turn either hand so that your palm is facing you to access the hand menu. Tap the \"Create Spatial Anchor\" button to save the Azure Spatial Anchor.";
        }
#endif
    }

    /// <summary>
    /// Displays the currency hand menu.
    /// </summary>
    public async void CurrencyButtonOnClickAsync()
    {
        // Allow a brief moment for the button press sound effect to finish playing.
        await Task.Delay(300);

        // Toggles the visuals on for the currency hand menu and off for the main hand menu.
        CurrencyHandMenu.transform.GetChild(0).gameObject.SetActive(true);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets and stops the Azure Spatial Anchor session.
    /// </summary>
    public void StopSession(Action completionRoutine = null)
    {
#if !UNITY_EDITOR
        cloudSpatialAnchorSession.Reset();

        lock (queue)
        {
            queue.Enqueue(() =>
            {
                if (cloudSpatialAnchorSession != null)
                {
                    cloudSpatialAnchorSession.Stop();
                    cloudSpatialAnchorSession.Dispose();
                    completionRoutine?.Invoke();
                }
            });
        }
#endif
    }

    /// <summary>
    /// Starts a new Azure Spatial Anchor session and checks to see if an anchor was previously created and saved.
    /// </summary>
    private void Start()
    {
        StartSession();

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("Anchor ID")))
        {
            // An Azure Spatial Anchor hasn't been created yet, so instantiate the Litecoin symbol prefab and guide the user to position it and create an ASA.
            DisplayLitecoinSymbol();
        }
        else
        {
            // An Azure Spatial Anchor was previous created. Guide the user to look around and find it.
            cloudSpatialAnchorId = PlayerPrefs.GetString("Anchor ID");
            Instructions.text = "An Azure Spatial Anchor has been previously created and saved. Look around to find the the anchor.";
            FindAzureSpatialAnchorAsync();
        }
    }

    /// <summary>
    /// Dequeues and runs an action in the queue.
    /// </summary>
    private void Update()
    {
        lock (queue)
        {
            if (queue.Count > 0)
            {
                queue.Dequeue()();
            }
        }
    }

    /// <summary>
    /// Cleans up the scene and stops the Azure Spatial Anchor session on exit.
    /// </summary>
    private void OnApplicationQuit()
    {
        Destroy(GameObject.FindGameObjectWithTag("Slate"));
        Destroy(GameObject.FindGameObjectWithTag("Litecoin"));
        StopSession();
    }

    /// <summary>
    /// Starts a new Azure Spatial Anchor session.
    /// </summary>
    private void StartSession()
    {
#if !UNITY_EDITOR
        Debug.Log("Initializing Azure Spatial Anchor session.");

        if (string.IsNullOrEmpty(AzureSpatialAnchorsConfiguration.SpatialAnchorsAccountId))
        {
            Debug.LogError("Azure Spatial Anchor account ID is not set.");
            return;
        }

        if (string.IsNullOrEmpty(AzureSpatialAnchorsConfiguration.SpatialAnchorsAccountKey))
        {
            Debug.LogError("Azure Spatial Anchor account key is not set.");
            return;
        }

        if (string.IsNullOrEmpty(AzureSpatialAnchorsConfiguration.SpatialAnchorsAccountDomain))
        {
            Debug.LogError("Azure Spatial Anchor account domain is not set.");
            return;
        }

        cloudSpatialAnchorSession = new CloudSpatialAnchorSession();

        cloudSpatialAnchorSession.Configuration.AccountId = AzureSpatialAnchorsConfiguration.SpatialAnchorsAccountId.Trim();
        cloudSpatialAnchorSession.Configuration.AccountKey = AzureSpatialAnchorsConfiguration.SpatialAnchorsAccountKey.Trim();
        cloudSpatialAnchorSession.Configuration.AccountDomain = AzureSpatialAnchorsConfiguration.SpatialAnchorsAccountDomain.Trim();

        cloudSpatialAnchorSession.LogLevel = SessionLogLevel.All;

        cloudSpatialAnchorSession.Error += CloudSpatialAnchorSession_Error;
        cloudSpatialAnchorSession.OnLogDebug += CloudSpatialAnchorSession_OnLogDebug;
        cloudSpatialAnchorSession.SessionUpdated += CloudSpatialAnchorSession_SessionUpdated;
        cloudSpatialAnchorSession.AnchorLocated += CloudSpatialAnchorSession_AnchorLocated;
        cloudSpatialAnchorSession.LocateAnchorsCompleted += CloudSpatialAnchorSession_LocateAnchorsCompleted;

        cloudSpatialAnchorSession.Start();
#else
        Debug.Log("Azure Spatial Anchor session can't be started when running in the Unity Editor.");
#endif
    }

    /// <summary>
    /// Instantiates the Litecoin symbol prefab and positions at eye level and slightly in-front of the user.
    /// </summary>
    private void DisplayLitecoinSymbol()
    {
        Vector3 position = new Vector3
        {
            x = Camera.main.transform.position.x,
            y = Camera.main.transform.position.y,
            z = Camera.main.transform.position.z + 1.5f
        };

        Instantiate(LitecoinPrefab, position, Camera.main.transform.rotation);

#if UNITY_EDITOR
        Instructions.text = "Azure Spatial Anchor sessions can't be started when running in the Unity Editor. Build and deploy this sample to a HoloLens device to be able to create and find Azure Spatial Anchors.\n\nUse the ray coming from either hand to position the Litecoin currency symbol. Air tap to place the symbol. This will be the location where your mining stats will be displayed.\n\nWhen you're ready, turn either hand so that your palm is facing you to access the hand menu. Tap the \"Create Spatial Anchor\" button to save the Azure Spatial Anchor.";
#else
        Instructions.text = "Use the ray coming from either hand to position the Litecoin currency symbol. Air tap to place the symbol. This will be the location where your mining stats will be displayed.\n\nWhen you're ready, turn either hand so that your palm is facing you to access the hand menu. Tap the \"Create Spatial Anchor\" button to save the Azure Spatial Anchor.";
#endif
    }

    /// <summary>
    /// Handles error delegate calls for the Azure Spatial Anchor session.
    /// </summary>
    private void CloudSpatialAnchorSession_Error(object sender, SessionErrorEventArgs args)
    {
        Debug.LogError($"Azure Spatial Anchor error: {args.ErrorMessage}");
    }

    /// <summary>
    /// Handles debug log delegate calls for the Azure Spatial Anchor session.
    /// </summary>
    private void CloudSpatialAnchorSession_OnLogDebug(object sender, OnLogDebugEventArgs args)
    {
        Debug.Log($"Azure Spatial Anchor log: {args.Message}");
        System.Diagnostics.Debug.WriteLine($"Azure Spatial Anchor log: {args.Message}");
    }

    /// <summary>
    /// Handles session update delegate calls for the Azure Spatial Anchor session.
    /// </summary>
    private void CloudSpatialAnchorSession_SessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
        Debug.Log($"Azure Spatial Anchor log: {args.Status.RecommendedForCreateProgress}");
        recommendedForCreate = args.Status.RecommendedForCreateProgress;
    }

    /// <summary>
    /// Handles anchor located delegate calls for the Azure Spatial Anchor Watcher.
    /// </summary>
    private void CloudSpatialAnchorSession_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        switch (args.Status)
        {
            case LocateAnchorStatus.Located:
                {
                    Debug.Log($"Azure Spatial Anchor located! Identifier: {args.Identifier}");
                    currentCloudAnchor = args.Anchor;

                    QueueOnUpdate(() =>
                    {
                        GameObject slate = Instantiate(SlatePrefab, Vector3.zero, Quaternion.identity);
                        slate.AddComponent<WorldAnchor>();

                        // Set the World Anchor to the Azure Spatial Anchor to position the slate.
                        slate.GetComponent<WorldAnchor>().SetNativeSpatialAnchorPtr(currentCloudAnchor.LocalAnchor);
                        
                        IsSlateActive = true;
                        Instructions.text = "Azure Spatial Anchor found!\n\nLook at the slate and it will detect your gaze and update itself.";
                    });

                    break;
                }

            case LocateAnchorStatus.AlreadyTracked:
                {
                    // This shouldn't be a reachable case in this sample, because we never search for an anchor unless we know we've created one.
                    Debug.Log($"Azure Spatial Anchor already tracked. Identifier: {args.Identifier}");
                    break;
                }

            case LocateAnchorStatus.NotLocated:
                {
                    Instructions.text = "Unable to locate an Azure Spatial Anchor.\n\nPlease try again.";
                    Debug.Log($"Unable to locate an Azure Spatial Anchor with identifier: {args.Identifier}");
                    break;
                }

            case LocateAnchorStatus.NotLocatedAnchorDoesNotExist:
                {
                    // This shouldn't be a reachable case in this sample, because we never search for an anchor unless we know we've created one.
                    Debug.LogError($"Unable to locate an Azure Spatial Anchor.\n\nAn anchor with the identifier \"{args.Identifier}\" does not exist.");
                    break;
                }
        }
    }

    /// <summary>
    /// Handles delegate calls for the Azure Spatial Anchor Watcher when the Watcher is finished locating anchors. 
    /// </summary>
    private void CloudSpatialAnchorSession_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
        Debug.Log($"Azure Spatial Anchor info: Locate anchors completed.\n\nWatcher identifier: {args.Watcher.Identifier}");
    }

    /// <summary>
    /// Queues the specified <see cref="Action"/> on update.
    /// </summary>
    /// <param name="action">The action to add to the queue.</param>
    protected void QueueOnUpdate(Action action)
    {
        lock (queue)
        {
            queue.Enqueue(action);
        }
    }
}
