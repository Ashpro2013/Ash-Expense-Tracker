using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using ExpenseIncomeTracker.Uno.Models;

namespace ExpenseIncomeTracker.Uno.Services;

public interface IActivationService
{
    bool IsActivated { get; }
    Task<ActivationResult> ActivateAsync(string email, string licenseKey);
}

public class ActivationService : IActivationService
{
    private const string ActivationKey = "IsAppActivated";
    private const string LicenseEmailKey = "ActivatedEmail";
    private const string LicenseKeyKey = "ActivatedLicenseKey";
    private const string DeviceIdKey = "ActivatedDeviceId";
    private const string LicenseTypeKey = "ActivatedLicenseType";
    private const string ExpiryDateKey = "ActivatedExpiryDate";
    private const string ApiUrl = "https://ashprotechnology.com/api/activation";
    private readonly HttpClient _httpClient = new();

    public bool IsActivated => ApplicationData.Current.LocalSettings.Values.ContainsKey(ActivationKey) && 
                               (bool)ApplicationData.Current.LocalSettings.Values[ActivationKey];

    public async Task<ActivationResult> ActivateAsync(string email, string licenseKey)
    {
        try
        {
            var deviceInfo = new EasClientDeviceInformation();
            var deviceId = deviceInfo.Id.ToString();
            var deviceName = deviceInfo.FriendlyName;

            var request = new ActivationRequest(
                Email: email,
                LicenseKey: licenseKey,
                DeviceId: deviceId,
                DeviceIdName: deviceName,
                ProductCode: "EXPENSE_TRACKER_PRO" // Example product code
            );

            var response = await _httpClient.PostAsJsonAsync(ApiUrl, request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ActivationResponse>();
                if (result != null && !string.IsNullOrWhiteSpace(result.licenseKey))
                {
                    SaveActivation(email, result);
                    return new ActivationResult(true, "Activated successfully.", result);
                }
                return new ActivationResult(false, "Failed to parse response or invalid license key.");
            }
            
            return new ActivationResult(false, $"Server returned error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return new ActivationResult(false, $"An error occurred: {ex.Message}");
        }
    }

    private void SaveActivation(string email, ActivationResponse response)
    {
        var settings = ApplicationData.Current.LocalSettings.Values;
        settings[ActivationKey] = true;
        settings[LicenseEmailKey] = email;
        settings[LicenseKeyKey] = response.licenseKey ?? string.Empty;
        settings[DeviceIdKey] = response.DeviceId ?? string.Empty;
        settings[LicenseTypeKey] = response.licenseType ?? string.Empty;
        settings[ExpiryDateKey] = response.expiryDate ?? string.Empty;
    }
}
