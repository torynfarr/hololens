using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This class is attached to the Currency Hand Menu.
/// </summary>
public class CurrencySelection : MonoBehaviour
{
    /// <summary>
    /// The GameObject for the Main Hand Menu.
    /// </summary>
    [Tooltip("Main Hand Menu GameObject in the scene.")]
    public GameObject MainHandMenu;

    /// <summary>
    /// The selected currency which will be used in the slate when displaying Litecoin mining rewards (LTC converted to the selected currency). 
    /// </summary>
    [Tooltip("Currency in which mining rewards will be displayed.")]
    public string Currency = "ltc_usd";

    /// <summary>
    /// Sets the currency to the value supplied by the button pressed in the Currency Hand Menu.
    /// </summary>
    /// <param name="currency">The currency in which mining rewards will be displayed.</param>
    public async void OnClickAsync(string currency)
    {
        // Allow a brief moment for the button press sound effect to finish playing.
        await Task.Delay(300);

        Currency = currency;

        // Toggles the visuals off for the currency hand menu and on for the main hand menu.
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        MainHandMenu.transform.GetChild(1).gameObject.SetActive(true);
    }
}
