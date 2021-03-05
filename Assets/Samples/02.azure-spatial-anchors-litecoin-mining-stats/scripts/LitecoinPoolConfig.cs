using UnityEngine;

/// <summary>
/// This menu item generates an optional configuration file which can be
/// excluded from source control to avoid committing credentials there.
/// </summary>
[CreateAssetMenu(fileName = "LitecoinPoolConfig", menuName = "Litecoin Pool/Configuration")]

public class LitecoinPoolConfig : ScriptableObject
{
    [Header("Litecoin Pool REST API Settings")]
    [SerializeField]
    [Tooltip("The host name of the Litecoin Pool site.")]
    protected string hostName = "";
    public string HostName => hostName;

    [SerializeField]
    [Tooltip("The path expected by the Litecoin Pool REST API.")]
    protected string path = "";
    public string Path => path;

    [SerializeField]
    [Tooltip("The Litecoin Pool API key (unique to your user account).")]
    protected string apiKey = "";
    public string APIKey => apiKey;
}
