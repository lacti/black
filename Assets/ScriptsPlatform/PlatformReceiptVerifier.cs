﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlatformReceiptVerifier : MonoBehaviour
{
    public static PlatformReceiptVerifier instance;

    static readonly string VERIFIER_URL =
        "https://hdsrvmrilf.execute-api.ap-northeast-2.amazonaws.com/default/verifyGoogleReceipt";

    static readonly string VERIFIER_API_KEY = "3dEvKBAk0TqpPfObOzDu2S03JQbdvFJ79yLM9YN9";

    [SerializeField]
    PlatformInterface platformInterface;

    static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public async Task<bool> Verify(string receipt)
    {
        try
        {
            using var httpClient = new HttpClient();
            PlatformInterface.instance.logger.Log($"HttpClient GET TO {VERIFIER_URL}...");
            httpClient.DefaultRequestHeaders.Add("x-api-key", VERIFIER_API_KEY);
            httpClient.DefaultRequestHeaders.Add("receipt-to-be-verified", Base64Encode(receipt));
            var getTask = await httpClient.GetAsync(new Uri(VERIFIER_URL));

            PlatformInterface.instance.logger.Log($"HttpClient Result: {getTask.ReasonPhrase}");

            if (getTask.IsSuccessStatusCode)
            {
                PlatformInterface.instance.logger.Log("Receipt verification result - success.");
                return true;
            }

            if (Application.isEditor)
            {
                Debug.Log(
                    $"Receipt verification result - failed: status code={getTask.StatusCode} (ALWAYS FAIL IN EDITOR MODE. NOT AN ERROR)");
                return true;
            }

            Debug.LogError($"Receipt verification result - failed: status code={getTask.StatusCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Receipt verification exception: {e}");
        }

        return false;
    }
}