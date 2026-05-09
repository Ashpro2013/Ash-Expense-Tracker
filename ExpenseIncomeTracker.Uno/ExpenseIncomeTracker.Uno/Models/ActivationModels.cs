namespace ExpenseIncomeTracker.Uno.Models;

public record ActivationRequest(
    string? Email, 
    string? LicenseKey, 
    string? DeviceId, 
    string? DeviceIdName, 
    string? ProductCode
);

public record ActivationResponse(
    string? licenseKey, 
    string? DeviceId, 
    string? licenseType, 
    string? startDate, 
    string? expiryDate, 
    int ForDays
);

public record ActivationResult(
    bool Success, 
    string? Message,
    ActivationResponse? Data = null
);
