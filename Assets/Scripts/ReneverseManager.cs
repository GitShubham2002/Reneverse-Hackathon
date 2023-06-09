using Rene.Sdk;
using Rene.Sdk.Api.Game.Data;
using ReneVerse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReneverseManager : MonoBehaviour
{
    public static string EmailHandler;

    public GameObject Email;
    public TextMeshProUGUI Timer;
    public GameObject LogInPanel;
    public GameObject CountdownPanel;
    private bool LoginStatus;
    public static API ReneAPI;

    public Button signInButton; // Reference to the UI button

    private void Start()
    {
        signInButton.onClick.AddListener(SignIn); // Assign the SignIn method to the button's click event
    }

    public async void SignIn()
    {
        await ConnectUser();
    }

    async Task ConnectUser()
    {
        ReneAPI = ReneAPIManager.API();
        EmailHandler = Email.GetComponent<TMP_InputField>().text;
        bool connected = await ReneAPI.Game().Connect(EmailHandler);
        Debug.Log(connected);
        if (!connected) return;
        StartCoroutine(ConnectReneService(ReneAPI));
    }

    private IEnumerator ConnectReneService(API reneApi)
    {
        CountdownPanel.SetActive(true);
        var counter = 30;
        var userConnected = false;
        var secondsToDecrement = 1;
        while (counter >= 0 && !userConnected)
        {
            Timer.text = counter.ToString();
            if (reneApi.IsAuthorized())
            {
                CountdownPanel.SetActive(false);
                LogInPanel.SetActive(false);
                yield return GetUserAssetsAsync(reneApi);
                userConnected = true;
                LoginStatus = true;
            }

            yield return new WaitForSeconds(secondsToDecrement);
            counter -= secondsToDecrement;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        CountdownPanel.SetActive(false);
    }

    private async Task GetUserAssetsAsync(API reneApi)
    {
        AssetsResponse.AssetsData userAssets = await reneApi.Game().Assets();
        userAssets?.Items.ForEach(asset =>
        {
            string assetName = asset.Metadata.Name;
            string assetImageUrl = asset.Metadata.Image;
            string assetColor = "";
            asset.Metadata?.Attributes?.ForEach(attribute =>
            {
                if (attribute.TraitType == "Color")
                {
                    assetColor = attribute.Value;
                }
                Debug.Log(assetName);
                Debug.Log(asset.AssetTemplateId);
            });

            Asset assetObj = new Asset(assetName, assetImageUrl, assetColor);
            // Add to a static list of Asset objects maybe
        });
    }
}

public class Asset
{
    public string AssetName { get; set; }
    public string AssetUrl { get; set; }
    public string AssetColor { get; set; }

    public Asset(string assetName, string assetUrl, string assetColor)
    {
        AssetName = assetName;
        AssetUrl = assetUrl;
        AssetColor = assetColor;
    }
}
