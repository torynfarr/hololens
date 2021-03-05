using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

/// <summary>
/// This class is attached to the slate prefab.
/// </summary>
public class QueryLitecoinPool : MonoBehaviour
{
    /// <summary>
    /// The configuration object which contains the litecoinpool.org REST API settings.
    /// </summary>
    [Tooltip("litecoinpool.org REST API configuration settings.")]
    public LitecoinPoolConfig LitecoinPoolConfiguration;

    /// <summary>
    /// GameObject used as a container for the TextMeshPro labels and fields.
    /// </summary>
    [Tooltip("Container for the text labels and fields.")]
    public GameObject MiningStats;

    /// <summary>
    /// GameObject used as a container for the rotating orbs progress indicator.
    /// </summary>
    [Tooltip("Container for the rotating orbs progress indicator.")]
    public GameObject ProgressIndicator;

    /// <summary>
    /// The TextMeshPro text used to display current status / error message.
    /// </summary>
    [Tooltip("TextMeshPro text used to display current status.")]
    public TextMeshPro Status;

    /// <summary>
    /// The TextMeshPro text used to display the current hash rate.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the current hash rate.")]
    public TextMeshPro HashRate;

    /// <summary>
    /// The TextMeshPro text used to display the total rewards paid out.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the total rewards paid out.")]
    public TextMeshPro PaidRewards;

    /// <summary>
    /// The TextMeshPro text used to display the rewards yet to be paid.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the rewards yet to be paid.")]
    public TextMeshPro UnpaidRewards;

    /// <summary>
    /// The TextMeshPro text used to display the total work performed.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the total work performed.")]
    public TextMeshPro TotalWork;

    /// <summary>
    /// The TextMeshPro text used to display the number of blocks found.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the number of blocks found")]
    public TextMeshPro Blocks;

    /// <summary>
    /// The TextMeshPro text used to display the current PPS ratio.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the current PPS ratio.")]
    public TextMeshPro Ratio;

    /// <summary>
    /// The TextMeshPro text used to display the current network difficulty.
    /// </summary>
    [Tooltip("TextMeshPro text used to display the current network difficulty.")]
    public TextMeshPro Difficulty;

    /// <summary>
    /// A queue of actions that will be executed on the main thread.
    /// </summary>
    private readonly Queue<Action> queue = new Queue<Action>();

    /// <summary>
    /// The HttpClient used to make an http GET request from the litecoinpool.org REST API.
    /// </summary>
    private HttpClient client;

    /// <summary>
    /// The duration of time between loop iterations for the timer based loop which updates the slate. 
    /// </summary>
    private float refreshRate = 5f;

    /// <summary>
    /// The timer incremented in the "Update" method used by the loop which updates the slate.
    /// </summary>
    private float refreshTimer = 0f;

    /// <summary>
    /// The selected currency which will be used in the slate when displaying Litecoin mining rewards (LTC converted to the selected currency). 
    /// </summary>
    private string currency;

    /// <summary>
    /// A flag which is set to true while the HTTP request is being performed.
    /// </summary>
    private bool updating;

    /// <summary>
    /// The progress indicator script component which controls the rotating orbs progress indicator on the slate.
    /// </summary>
    private IProgressIndicator indicator;

    /// <summary>
    /// The GameObject for the Currency Hand Menu.
    /// </summary>
    private GameObject currencyHandMenu;

    /// <summary>
    /// Queries the Litecoin Pool REST API asynchronously via https request and updates the slate with the current stats.
    /// </summary>
    /// <returns>The asynchronous task which should be awaited the result of which is the http response message.</returns>
    public async void QueryRESTAPI()
    {
        // If we're already in the middle of querying the REST API, don't query it again.
        if (updating)
        {
            // Set the refresh timer back to 0.
            refreshTimer = 0f;
            return;
        }

        updating = true;

        if (Status.gameObject.activeSelf)
        {
            // Allow a brief moment so the status text is viewable on the first run or after an error.
            await Task.Delay(1000);
        }

        // Don't show the progress indicator on the first run (the mining stats will not be active on the first run or after an error).
        if (MiningStats.gameObject.activeSelf)
        {
            QueueOnUpdate(() =>
            {
                ProgressIndicator.SetActive(true);
            });

            await indicator.AwaitTransitionAsync();
            UpdateProgressIndicator();
        }

        client.CancelPendingRequests();

        // Get the currently selected currency from the currency hand menu.
        currency = currencyHandMenu.GetComponent<CurrencySelection>().Currency;

        HttpResponseMessage response = await client.GetAsync(client.BaseAddress).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            JObject jobject = (JObject)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

            QueueOnUpdate(() =>
            {
                // The hash rate is returned as kilohash. Convert it to a size appropriate unit of measure formatted for easy reading.
                HashRate.text = $"{FormatHash((decimal)jobject["user"]["hash_rate"] * 1000, 2)}/s";
            });

            QueueOnUpdate(() =>
            {
                // Format the paid rewards to two decimal points and include a currency conversion for the currently selected currency.
                PaidRewards.text = $"{Math.Round((decimal)jobject["user"]["paid_rewards"], 2)} LTC ({ConvertCurrency((decimal)jobject["user"]["paid_rewards"], (decimal)jobject["market"][currency])})";
            });

            QueueOnUpdate(() =>
            {
                // Format the unpaid rewards to six decimal points and include a currency conversion for the currently selected currency.
                UnpaidRewards.text = $"{Math.Round((decimal)jobject["user"]["unpaid_rewards"], 6)} LTC ({ConvertCurrency((decimal)jobject["user"]["unpaid_rewards"], (decimal)jobject["market"][currency])})";
            });

            QueueOnUpdate(() =>
            {
                // The total work is returned as hash (lowest unit of measure). Convert it to a size appropriate unit of measure formatted for easy reading.
                TotalWork.text = FormatHash((decimal)jobject["user"]["total_work"] / 10, 0);
            });

            QueueOnUpdate(() =>
            {
                Blocks.text = (string)jobject["user"]["blocks_found"];
            });

            QueueOnUpdate(() =>
            {
                // Format the price per share ratio as a percentage with no decimal points.
                Ratio.text = $"{Math.Round((decimal)jobject["pool"]["pps_ratio"] * 100, 0)}%";
            });

            QueueOnUpdate(() =>
            {
                // Format the network difficulty to five decimal points and add standard numerical comma notation.
                Difficulty.text = Math.Round((decimal)jobject["network"]["difficulty"], 5).ToString("#,##0.00000");
            });

            QueueOnUpdate(() =>
            {
                Status.gameObject.SetActive(false);
                MiningStats.SetActive(true);
            });
        }
        else
        {
            QueueOnUpdate(() =>
            {
                MiningStats.SetActive(false);
                Status.gameObject.SetActive(true);
                ProgressIndicator.SetActive(false);
                Status.text = "Error communicating with litecoinpool.org";
            });

            // Allow a brief moment so the status text to be viewable.
            await Task.Delay(2000);
        }

        updating = false;

        await indicator.AwaitTransitionAsync();
        await indicator.CloseAsync();
    }

    /// <summary>
    /// Sets the refresh rate to the specified value,
    /// Used by the eye tracking target script to start and stop the timer based loops which updates the slate.
    /// </summary>
    /// <param name="rate">The duration of time between loop iterations.</param>
    public void RefreshRate(float rate)
    {
        refreshRate = rate;
        refreshTimer += Time.deltaTime;
    }

    /// <summary>
    /// Starts a new REST client used to access data from litecoinpool.org.
    /// </summary>
    private void Start()
    {
        if (string.IsNullOrEmpty(LitecoinPoolConfiguration.HostName))
        {
            Debug.LogError("Litecoin Pool host name is not set.");
            return;
        }

        if (string.IsNullOrEmpty(LitecoinPoolConfiguration.Path))
        {
            Debug.LogError("Litecoin Pool REST API path is not set.");
            return;
        }

        if (string.IsNullOrEmpty(LitecoinPoolConfiguration.APIKey))
        {
            Debug.LogError("Litecoin Pool REST API key is not set.");
            return;
        }

        currencyHandMenu = GameObject.FindGameObjectWithTag("Currency Hand Menu");
        currency = currencyHandMenu.GetComponent<CurrencySelection>().Currency;

        indicator = ProgressIndicator.GetComponent<IProgressIndicator>();

        // Configure the http client.
        client = new HttpClient
        {
            BaseAddress = new Uri($"https://{LitecoinPoolConfiguration.HostName}/{LitecoinPoolConfiguration.Path}={LitecoinPoolConfiguration.APIKey}")
        };

        AuthenticationHeaderValue header = new AuthenticationHeaderValue("Basic");
        client.DefaultRequestHeaders.Authorization = header;

        QueryRESTAPI();
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

        // Every three seconds the litecoinpool.org REST API will be queried and the data shown in the slate will be refreshed.
        if (refreshRate > 0f && !updating)
        {
            refreshTimer += Time.deltaTime;

            if (refreshTimer >= refreshRate)
            {
                QueryRESTAPI();
                refreshTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Animates the rotating orbs progress indicator.
    /// </summary>
    private async void UpdateProgressIndicator()
    {
        await indicator.OpenAsync();

        float timeStarted = Time.time;

        while (Time.time < timeStarted + 2f)
        {
            float normalizedProgress = Mathf.Clamp01((Time.time - timeStarted) / 2f);
            indicator.Progress = normalizedProgress;

            await Task.Yield();

            switch (indicator.State)
            {
                case ProgressIndicatorState.Open:
                    break;

                default:
                    // The indicator was closed.
                    return;
            }
        }

        await indicator.CloseAsync();
    }

    /// <summary>
    /// Converts a hash value from hash to a size appropriate unit of measure.
    /// </summary>
    /// <param name="value">The hash value in hash format.</param>
    /// <param name="decimals">The number of decimal places to round the value to.</param>
    /// <returns>A string with the hash value converted to a value appopriate for its size and formatted with the unit of measure.</returns>
    private string FormatHash(decimal value, int decimals)
    {
        // Format the hash as zettahash if it's greather than or equal to one sextillion hash.
        if (value >= (decimal)BigInteger.Parse("1000000000000000000000"))
        {
            return $"{Math.Round(value / 100000000000000000, decimals):#,###} ZH";
        }

        // Format the hash as exahash if it's less than one sextillion hash, but greather than or equal to one quintillion hash.
        if (value < (decimal)BigInteger.Parse("1000000000000000000000") && value >= (decimal)BigInteger.Parse("1000000000000000000"))
        {
            return $"{Math.Round(value / 1000000000000000, decimals):#,###} EH";
        }

        // Format the hash as petahash if it's less than one quintillion hash, but greather than or equal to one quadrillion hash.
        if (value < 1000000000000000000 && value >= 1000000000000000)
        {
            return $"{Math.Round(value / 10000000000000, decimals):#,###} PH";
        }

        // Format the hash as terahash if it's less than one quadrillion hash, but greather than or equal to one trillion hash.
        if (value < 1000000000000000 && value >= 1000000000000)
        {
            return $"{Math.Round(value / 100000000000, decimals):#,###} TH";
        }

        // Format the hash as gigahash if it's less than one trillion hash, but greater than or equal to one billion hash.
        if (value < 1000000000000 && value >= 1000000000)
        {
            return $"{Math.Round(value / 1000000000, decimals):#,###} GH";
        }

        // Format the hash as megahash if it's less than one billion hash, but greater than or equal to one hundred million hash.
        if (value < 1000000000 && value >= 100000000)
        {
            return $"{Math.Round(value / 1000000, decimals):#,###} MH";
        }

        // Format the hash as kilohash if it's less than one million hash, but greater than or equal to one thousand hash.
        if (value < 100000000 && value >= 1000)
        {
            return $"{Math.Round(value / 1000, decimals):#,###} kH";
        }

        // Format the hash as just hash if it's less than one thousand hash.
        if (value < 1000)
        {
            return $"{Math.Round(value, decimals):#,###} H";
        }

        return "0 H";
    }

    /// <summary>
    /// Converts rewards from LTC to the currently selected currency.
    /// </summary>
    /// <param name="rewards">A decimal value representing a quantity of Litecoin.</param>
    /// <param name="rate">The exchange rate from LTC to the selected currency.</param>
    /// <returns>A string with the currency conversion along with the currency symbol and abbreviation.</returns>
    private string ConvertCurrency(decimal rewards, decimal rate)
    {
        decimal conversion = Math.Round(rewards * rate, 2);

        switch (currency)
        {
            case "ltc_btc":
            {
                return $"{conversion} BTC";
            }
            
            case "ltc_usd":
            default:
            {
                return $"${conversion} USD";
            }

            case "ltc_cad":
            {
                return $"${conversion} CAD";
            }

            case "ltc_eur":
            {
                return $"€{conversion} EUR";
            }

            case "ltc_gbp":         
            {
                return $"£{conversion} GBP";
            }

            case "ltc_rub":
            {
                return $"{conversion} RUB";
            }

            case "ltc_cny":
            {
                return $"¥{conversion} CNY";
            }

            case "ltc_aud":
            {
                return $"${conversion} AUD";
            }

            case "ltc_zar":            
            {
                return $"R{conversion} ZAR";
            }
        }
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
