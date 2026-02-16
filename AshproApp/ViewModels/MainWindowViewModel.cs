using System.Collections.ObjectModel;
using System.Globalization;
using AshproApp.Models;
using AshproApp.Services;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AshproApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly RememberMeService _rememberMeService;
    private readonly AlbumService _albumService;
    private readonly FinanceEntryService _financeEntryService;
    private readonly DiaryEntryService _diaryEntryService;
    private readonly ActivityItemService _activityItemService;

    private int _currentUserId;
    private bool _isInitialized;
    private readonly List<AlbumImageCard> _albumViewerItems = new();
    private int _albumViewerIndex = -1;

    public MainWindowViewModel(
        AuthService authService,
        RememberMeService rememberMeService,
        AlbumService albumService,
        FinanceEntryService financeEntryService,
        DiaryEntryService diaryEntryService,
        ActivityItemService activityItemService)
    {
        _authService = authService;
        _rememberMeService = rememberMeService;
        _albumService = albumService;
        _financeEntryService = financeEntryService;
        _diaryEntryService = diaryEntryService;
        _activityItemService = activityItemService;

        NavigationItems = new ReadOnlyCollection<string>(new[] { "Dashboard", "Income", "Expense", "Activity", "Diary", "Albums", "Account" });
        StatusFilters = new ReadOnlyCollection<string>(new[] { "All", "Planned", "InProgress", "Done" });
        ActivityStatuses = new ReadOnlyCollection<ActivityStatus>(Enum.GetValues<ActivityStatus>());
        AlbumCategories = new ReadOnlyCollection<string>(new[] { "Personal", "Family" });
        AlbumFilters = new ReadOnlyCollection<string>(new[] { "All", "Personal", "Family" });

        IncomeEntries = new ObservableCollection<FinanceEntry>();
        ExpenseEntries = new ObservableCollection<FinanceEntry>();
        RecentEntries = new ObservableCollection<FinanceEntry>();
        DiaryEntries = new ObservableCollection<DiaryEntry>();
        Activities = new ObservableCollection<ActivityItem>();
        AlbumImages = new ObservableCollection<AlbumImageCard>();

        ResetIncomeForm();
        ResetExpenseForm();
        ResetDiaryForm();
        ResetActivityForm();
        ResetPasswordForm();
        ResetAlbumForm();
    }

    public ReadOnlyCollection<string> NavigationItems { get; }
    public ReadOnlyCollection<string> StatusFilters { get; }
    public ReadOnlyCollection<ActivityStatus> ActivityStatuses { get; }
    public ReadOnlyCollection<string> AlbumCategories { get; }
    public ReadOnlyCollection<string> AlbumFilters { get; }

    public ObservableCollection<FinanceEntry> IncomeEntries { get; }
    public ObservableCollection<FinanceEntry> ExpenseEntries { get; }
    public ObservableCollection<FinanceEntry> RecentEntries { get; }
    public ObservableCollection<DiaryEntry> DiaryEntries { get; }
    public ObservableCollection<ActivityItem> Activities { get; }
    public ObservableCollection<AlbumImageCard> AlbumImages { get; }

    [ObservableProperty] private bool isAuthenticated;
    [ObservableProperty] private bool isLoginMode = true;
    [ObservableProperty] private string currentUserEmail = string.Empty;
    public bool ShowAuthScreen => !IsAuthenticated;
    public bool ShowMainShell => IsAuthenticated;
    public bool ShowLoginPanel => IsLoginMode;
    public bool ShowRegisterPanel => !IsLoginMode;
    public bool IsRegisterMode => !IsLoginMode;

    [ObservableProperty] private string loginEmail = string.Empty;
    [ObservableProperty] private string loginPassword = string.Empty;
    [ObservableProperty] private bool rememberMe;
    [ObservableProperty] private string registerEmail = string.Empty;
    [ObservableProperty] private string registerPassword = string.Empty;
    [ObservableProperty] private string registerConfirmPassword = string.Empty;
    [ObservableProperty] private string authError = string.Empty;
    [ObservableProperty] private string authInfo = string.Empty;

    [ObservableProperty] private string currentPassword = string.Empty;
    [ObservableProperty] private string newPassword = string.Empty;
    [ObservableProperty] private string confirmNewPassword = string.Empty;
    [ObservableProperty] private string passwordChangeError = string.Empty;
    [ObservableProperty] private string passwordChangeInfo = string.Empty;

    [ObservableProperty] private string selectedAlbumCategory = "Personal";
    [ObservableProperty] private string selectedAlbumFilter = "All";
    [ObservableProperty] private string albumCaption = string.Empty;
    [ObservableProperty] private string albumError = string.Empty;
    [ObservableProperty] private string albumInfo = string.Empty;
    [ObservableProperty] private bool isAlbumViewerOpen;
    [ObservableProperty] private AlbumImageCard? albumViewerItem;
    [ObservableProperty] private Bitmap? albumViewerImage;
    [ObservableProperty] private string albumViewerCaption = string.Empty;
    [ObservableProperty] private string albumViewerMeta = string.Empty;
    [ObservableProperty] private double albumViewerZoom = 1.0;
    [ObservableProperty] private double albumViewerImageWidth;
    [ObservableProperty] private double albumViewerImageHeight;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "Please sign in to continue.";

    [ObservableProperty] private string selectedSection = "Dashboard";

    public bool ShowDashboard => SelectedSection == "Dashboard";
    public bool ShowIncome => SelectedSection == "Income";
    public bool ShowExpense => SelectedSection == "Expense";
    public bool ShowActivity => SelectedSection == "Activity";
    public bool ShowDiary => SelectedSection == "Diary";
    public bool ShowAlbums => SelectedSection == "Albums";
    public bool ShowAccount => SelectedSection == "Account";

    public bool IsDashboardSelected => SelectedSection == "Dashboard";
    public bool IsIncomeSelected => SelectedSection == "Income";
    public bool IsExpenseSelected => SelectedSection == "Expense";
    public bool IsActivitySelected => SelectedSection == "Activity";
    public bool IsDiarySelected => SelectedSection == "Diary";
    public bool IsAlbumsSelected => SelectedSection == "Albums";
    public bool IsAccountSelected => SelectedSection == "Account";

    [ObservableProperty] private decimal incomeTotal;
    [ObservableProperty] private decimal expenseTotal;
    [ObservableProperty] private decimal balance;
    [ObservableProperty] private int diaryEntriesCount;
    [ObservableProperty] private int openActivitiesCount;

    public string BalanceStatus => Balance >= 0 ? "Positive cash flow" : "Spending is above income";

    [ObservableProperty] private int? editingIncomeId;
    [ObservableProperty] private string incomeTitle = string.Empty;
    [ObservableProperty] private string incomeAmount = string.Empty;
    [ObservableProperty] private DateTimeOffset? incomeEntryDate;
    [ObservableProperty] private string incomeNote = string.Empty;
    [ObservableProperty] private string incomeFormError = string.Empty;
    public string IncomeHeader => EditingIncomeId is null ? "Add income" : "Edit income";

    [ObservableProperty] private int? editingExpenseId;
    [ObservableProperty] private string expenseTitle = string.Empty;
    [ObservableProperty] private string expenseAmount = string.Empty;
    [ObservableProperty] private DateTimeOffset? expenseEntryDate;
    [ObservableProperty] private string expenseNote = string.Empty;
    [ObservableProperty] private string expenseFormError = string.Empty;
    public string ExpenseHeader => EditingExpenseId is null ? "Add expense" : "Edit expense";

    [ObservableProperty] private int? editingDiaryId;
    [ObservableProperty] private string diaryTitle = string.Empty;
    [ObservableProperty] private string diaryContent = string.Empty;
    [ObservableProperty] private DateTimeOffset? diaryEntryDate;
    [ObservableProperty] private string diaryTags = string.Empty;
    [ObservableProperty] private string diaryMood = string.Empty;
    [ObservableProperty] private string diarySearch = string.Empty;
    [ObservableProperty] private string diaryFormError = string.Empty;
    public string DiaryHeader => EditingDiaryId is null ? "New entry" : "Edit entry";

    [ObservableProperty] private int? editingActivityId;
    [ObservableProperty] private string activityTitle = string.Empty;
    [ObservableProperty] private string activityCategory = string.Empty;
    [ObservableProperty] private ActivityStatus selectedActivityStatus = ActivityStatus.Planned;
    [ObservableProperty] private DateTimeOffset? activityStartDate;
    [ObservableProperty] private DateTimeOffset? activityDueDate;
    [ObservableProperty] private string activityDescription = string.Empty;
    [ObservableProperty] private bool activityIsImportant;
    [ObservableProperty] private string activityStatusFilter = "All";
    [ObservableProperty] private string activityFormError = string.Empty;
    public string ActivityHeader => EditingActivityId is null ? "Plan activity" : "Edit activity";

    public IEnumerable<DiaryEntry> FilteredDiaryEntries => DiaryEntries.Where(entry => MatchesDiarySearch(entry, DiarySearch));

    public IEnumerable<ActivityItem> FilteredActivities => Activities
        .Where(item => ActivityStatusFilter == "All" || item.Status.ToString() == ActivityStatusFilter)
        .OrderBy(item => item.Status)
        .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
        .ThenByDescending(item => item.Id);

    public IEnumerable<AlbumImageCard> FilteredAlbumImages => AlbumImages
        .Where(item => SelectedAlbumFilter == "All" || item.Category.Equals(SelectedAlbumFilter, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(item => item.CreatedAtUtc);

    public bool HasAlbumImages => FilteredAlbumImages.Any();
    public bool ShowNoAlbumImages => !HasAlbumImages;
    public bool CanViewPreviousAlbumImage => _albumViewerIndex > 0;
    public bool CanViewNextAlbumImage => _albumViewerIndex >= 0 && _albumViewerIndex < _albumViewerItems.Count - 1;
    public string AlbumViewerZoomText => $"{AlbumViewerZoom * 100:0}%";

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        var remembered = await _rememberMeService.LoadAsync();
        if (remembered is null)
        {
            return;
        }

        var user = await _authService.GetUserByIdAsync(remembered.UserId);
        if (user is null)
        {
            _rememberMeService.Clear();
            return;
        }

        RememberMe = true;
        await SignInUserAsync(user, isRememberedSession: true);
    }

    [RelayCommand]
    private void ShowLogin()
    {
        IsLoginMode = true;
        AuthError = string.Empty;
        AuthInfo = string.Empty;
    }

    [RelayCommand]
    private void ShowRegister()
    {
        IsLoginMode = false;
        AuthError = string.Empty;
        AuthInfo = string.Empty;
        RememberMe = false;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        AuthError = string.Empty;
        AuthInfo = string.Empty;

        var (success, error, user) = await _authService.SignInAsync(LoginEmail, LoginPassword);
        if (!success || user is null)
        {
            AuthError = error ?? "Sign in failed.";
            return;
        }

        await SignInUserAsync(user, isRememberedSession: false);
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        AuthError = string.Empty;
        AuthInfo = string.Empty;
        RememberMe = false;

        var (success, error, user) = await _authService.RegisterAsync(RegisterEmail, RegisterPassword, RegisterConfirmPassword);
        if (!success || user is null)
        {
            AuthError = error ?? "Sign up failed.";
            return;
        }

        await SignInUserAsync(user, isRememberedSession: false);
        StatusMessage = "Account created.";
    }

    [RelayCommand]
    private void SignOut()
    {
        _currentUserId = 0;
        IsAuthenticated = false;
        CurrentUserEmail = string.Empty;
        SelectedSection = "Dashboard";

        ClearData();
        ResetAuthInputs();
        ResetIncomeForm();
        ResetExpenseForm();
        ResetDiaryForm();
        ResetActivityForm();
        ResetPasswordForm();
        ResetAlbumForm();
        _rememberMeService.Clear();
        RememberMe = false;

        IsLoginMode = true;
        AuthInfo = "Signed out.";
        StatusMessage = "Please sign in to continue.";
    }

    [RelayCommand]
    private void Navigate(string? section)
    {
        if (string.IsNullOrWhiteSpace(section) || !NavigationItems.Contains(section))
        {
            return;
        }

        SelectedSection = section;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await ReloadAllAsync();
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        PasswordChangeError = string.Empty;
        PasswordChangeInfo = string.Empty;

        if (!EnsureAuthenticated())
        {
            PasswordChangeError = "Sign in required.";
            return;
        }

        var (success, error) = await _authService.ChangePasswordAsync(
            _currentUserId,
            CurrentPassword,
            NewPassword,
            ConfirmNewPassword);

        if (!success)
        {
            PasswordChangeError = error ?? "Unable to change password.";
            return;
        }

        ResetPasswordForm();
        PasswordChangeInfo = "Password updated successfully.";
    }

    public async Task UploadAlbumFilesAsync(IEnumerable<string> filePaths)
    {
        AlbumError = string.Empty;
        AlbumInfo = string.Empty;

        if (!EnsureAuthenticated())
        {
            AlbumError = "Sign in required.";
            return;
        }

        var paths = filePaths
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (paths.Count == 0)
        {
            AlbumError = "Please select at least one image file.";
            return;
        }

        var uploaded = 0;
        foreach (var path in paths)
        {
            var caption = paths.Count == 1 ? AlbumCaption : string.Empty;
            var saved = await _albumService.AddAsync(_currentUserId, path, SelectedAlbumCategory, caption);
            if (saved is not null)
            {
                uploaded++;
            }
        }

        if (uploaded == 0)
        {
            AlbumError = "No images were uploaded. Ensure the files are valid images.";
            return;
        }

        AlbumCaption = string.Empty;
        await LoadAlbumsAsync();
        AlbumInfo = uploaded == 1
            ? $"1 image uploaded to {SelectedAlbumCategory} album."
            : $"{uploaded} images uploaded to {SelectedAlbumCategory} album.";
    }

    [RelayCommand]
    private async Task DeleteAlbumImageAsync(AlbumImageCard? image)
    {
        if (image is null || !EnsureAuthenticated())
        {
            return;
        }

        AlbumError = string.Empty;
        AlbumInfo = string.Empty;

        var deleted = await _albumService.DeleteAsync(_currentUserId, image.Id);
        if (!deleted)
        {
            AlbumError = "Unable to delete image.";
            return;
        }

        await LoadAlbumsAsync();
        AlbumInfo = "Image deleted.";
    }

    [RelayCommand]
    private void OpenAlbumViewer(AlbumImageCard? image)
    {
        if (image is null || !EnsureAuthenticated())
        {
            return;
        }

        var visibleItems = FilteredAlbumImages.ToList();
        if (visibleItems.Count == 0)
        {
            return;
        }

        var index = visibleItems.FindIndex(item => item.Id == image.Id);
        if (index < 0)
        {
            return;
        }

        _albumViewerItems.Clear();
        _albumViewerItems.AddRange(visibleItems);
        _albumViewerIndex = index;
        IsAlbumViewerOpen = true;
        ShowAlbumViewerImage(resetZoom: true);
    }

    [RelayCommand]
    private void CloseAlbumViewer()
    {
        IsAlbumViewerOpen = false;
        _albumViewerItems.Clear();
        _albumViewerIndex = -1;

        AlbumViewerItem = null;
        AlbumViewerCaption = string.Empty;
        AlbumViewerMeta = string.Empty;
        AlbumViewerZoom = 1.0;
        AlbumViewerImageWidth = 0;
        AlbumViewerImageHeight = 0;

        AlbumViewerImage?.Dispose();
        AlbumViewerImage = null;

        OnPropertyChanged(nameof(CanViewPreviousAlbumImage));
        OnPropertyChanged(nameof(CanViewNextAlbumImage));
    }

    [RelayCommand]
    private void ShowPreviousAlbumImage()
    {
        if (!CanViewPreviousAlbumImage)
        {
            return;
        }

        _albumViewerIndex--;
        ShowAlbumViewerImage(resetZoom: false);
    }

    [RelayCommand]
    private void ShowNextAlbumImage()
    {
        if (!CanViewNextAlbumImage)
        {
            return;
        }

        _albumViewerIndex++;
        ShowAlbumViewerImage(resetZoom: false);
    }

    [RelayCommand]
    private void ZoomInAlbumImage()
    {
        SetAlbumViewerZoom(AlbumViewerZoom + 0.25);
    }

    [RelayCommand]
    private void ZoomOutAlbumImage()
    {
        SetAlbumViewerZoom(AlbumViewerZoom - 0.25);
    }

    [RelayCommand]
    private void ResetAlbumImageZoom()
    {
        SetAlbumViewerZoom(1.0);
    }

    [RelayCommand]
    private void DownloadAlbumImage(AlbumImageCard? image)
    {
        if (image is null || !EnsureAuthenticated())
        {
            return;
        }

        AlbumError = string.Empty;
        AlbumInfo = string.Empty;

        if (string.IsNullOrWhiteSpace(image.FilePath) || !File.Exists(image.FilePath))
        {
            AlbumError = "Unable to find the image file.";
            return;
        }

        try
        {
            var extension = Path.GetExtension(image.FilePath);
            var fileName = BuildDownloadFileName(image.Caption);
            var downloadsDirectory = GetDownloadsDirectory();
            Directory.CreateDirectory(downloadsDirectory);

            var destinationPath = GetUniqueFilePath(downloadsDirectory, fileName, extension);
            File.Copy(image.FilePath, destinationPath);

            AlbumInfo = $"Image downloaded to {destinationPath}";
        }
        catch
        {
            AlbumError = "Download failed. Try again.";
        }
    }

    [RelayCommand]
    private async Task SaveIncomeAsync()
    {
        IncomeFormError = string.Empty;

        if (!EnsureAuthenticated())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(IncomeTitle))
        {
            IncomeFormError = "Title is required.";
            return;
        }

        if (!TryParseAmount(IncomeAmount, out var amount) || amount <= 0)
        {
            IncomeFormError = "Amount must be greater than 0.";
            return;
        }

        var entry = new FinanceEntry
        {
            Id = EditingIncomeId ?? 0,
            Title = IncomeTitle.Trim(),
            Amount = amount,
            EntryDate = ToDate(IncomeEntryDate),
            Note = string.IsNullOrWhiteSpace(IncomeNote) ? null : IncomeNote.Trim(),
            Type = FinanceEntryType.Income
        };

        if (EditingIncomeId is null)
        {
            await _financeEntryService.CreateAsync(_currentUserId, entry);
        }
        else
        {
            await _financeEntryService.UpdateAsync(_currentUserId, entry);
        }

        await LoadFinanceAsync();
        ResetIncomeForm();
    }

    [RelayCommand]
    private void EditIncome(FinanceEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        EditingIncomeId = entry.Id;
        IncomeTitle = entry.Title;
        IncomeAmount = entry.Amount.ToString("0.##", CultureInfo.CurrentCulture);
        IncomeEntryDate = entry.EntryDate;
        IncomeNote = entry.Note ?? string.Empty;
        IncomeFormError = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteIncomeAsync(FinanceEntry? entry)
    {
        if (entry is null || !EnsureAuthenticated())
        {
            return;
        }

        await _financeEntryService.DeleteAsync(_currentUserId, entry.Id);
        await LoadFinanceAsync();

        if (EditingIncomeId == entry.Id)
        {
            ResetIncomeForm();
        }
    }

    [RelayCommand]
    private void CancelIncomeEdit()
    {
        ResetIncomeForm();
    }

    [RelayCommand]
    private async Task SaveExpenseAsync()
    {
        ExpenseFormError = string.Empty;

        if (!EnsureAuthenticated())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ExpenseTitle))
        {
            ExpenseFormError = "Title is required.";
            return;
        }

        if (!TryParseAmount(ExpenseAmount, out var amount) || amount <= 0)
        {
            ExpenseFormError = "Amount must be greater than 0.";
            return;
        }

        var entry = new FinanceEntry
        {
            Id = EditingExpenseId ?? 0,
            Title = ExpenseTitle.Trim(),
            Amount = amount,
            EntryDate = ToDate(ExpenseEntryDate),
            Note = string.IsNullOrWhiteSpace(ExpenseNote) ? null : ExpenseNote.Trim(),
            Type = FinanceEntryType.Expense
        };

        if (EditingExpenseId is null)
        {
            await _financeEntryService.CreateAsync(_currentUserId, entry);
        }
        else
        {
            await _financeEntryService.UpdateAsync(_currentUserId, entry);
        }

        await LoadFinanceAsync();
        ResetExpenseForm();
    }

    [RelayCommand]
    private void EditExpense(FinanceEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        EditingExpenseId = entry.Id;
        ExpenseTitle = entry.Title;
        ExpenseAmount = entry.Amount.ToString("0.##", CultureInfo.CurrentCulture);
        ExpenseEntryDate = entry.EntryDate;
        ExpenseNote = entry.Note ?? string.Empty;
        ExpenseFormError = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteExpenseAsync(FinanceEntry? entry)
    {
        if (entry is null || !EnsureAuthenticated())
        {
            return;
        }

        await _financeEntryService.DeleteAsync(_currentUserId, entry.Id);
        await LoadFinanceAsync();

        if (EditingExpenseId == entry.Id)
        {
            ResetExpenseForm();
        }
    }

    [RelayCommand]
    private void CancelExpenseEdit()
    {
        ResetExpenseForm();
    }

    [RelayCommand]
    private async Task SaveDiaryAsync()
    {
        DiaryFormError = string.Empty;

        if (!EnsureAuthenticated())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(DiaryTitle) || string.IsNullOrWhiteSpace(DiaryContent))
        {
            DiaryFormError = "Title and reflection are required.";
            return;
        }

        if (!TryParseMood(DiaryMood, out var mood))
        {
            DiaryFormError = "Mood must be between 0 and 5.";
            return;
        }

        var entry = new DiaryEntry
        {
            Id = EditingDiaryId ?? 0,
            Title = DiaryTitle.Trim(),
            Content = DiaryContent.Trim(),
            EntryDate = ToDate(DiaryEntryDate),
            TagsCsv = NormalizeTags(DiaryTags),
            Mood = mood
        };

        if (EditingDiaryId is null)
        {
            await _diaryEntryService.CreateAsync(_currentUserId, entry);
        }
        else
        {
            await _diaryEntryService.UpdateAsync(_currentUserId, entry);
        }

        await LoadDiaryAsync();
        ResetDiaryForm();
    }

    [RelayCommand]
    private void EditDiary(DiaryEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        EditingDiaryId = entry.Id;
        DiaryTitle = entry.Title;
        DiaryContent = entry.Content;
        DiaryEntryDate = entry.EntryDate;
        DiaryTags = entry.TagsCsv;
        DiaryMood = entry.Mood == 0 ? string.Empty : entry.Mood.ToString(CultureInfo.InvariantCulture);
        DiaryFormError = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteDiaryAsync(DiaryEntry? entry)
    {
        if (entry is null || !EnsureAuthenticated())
        {
            return;
        }

        await _diaryEntryService.DeleteAsync(_currentUserId, entry.Id);
        await LoadDiaryAsync();

        if (EditingDiaryId == entry.Id)
        {
            ResetDiaryForm();
        }
    }

    [RelayCommand]
    private void CancelDiaryEdit()
    {
        ResetDiaryForm();
    }

    [RelayCommand]
    private async Task SaveActivityAsync()
    {
        ActivityFormError = string.Empty;

        if (!EnsureAuthenticated())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ActivityTitle))
        {
            ActivityFormError = "Title is required.";
            return;
        }

        var item = new ActivityItem
        {
            Id = EditingActivityId ?? 0,
            Title = ActivityTitle.Trim(),
            Description = string.IsNullOrWhiteSpace(ActivityDescription) ? null : ActivityDescription.Trim(),
            Category = string.IsNullOrWhiteSpace(ActivityCategory) ? null : ActivityCategory.Trim(),
            Status = SelectedActivityStatus,
            StartDate = ActivityStartDate?.Date,
            DueDate = ActivityDueDate?.Date,
            IsImportant = ActivityIsImportant,
            CompletedAt = SelectedActivityStatus == ActivityStatus.Done ? DateTime.UtcNow : null
        };

        if (EditingActivityId is null)
        {
            await _activityItemService.CreateAsync(_currentUserId, item);
        }
        else
        {
            await _activityItemService.UpdateAsync(_currentUserId, item);
        }

        await LoadActivitiesAsync();
        ResetActivityForm();
    }

    [RelayCommand]
    private void EditActivity(ActivityItem? item)
    {
        if (item is null)
        {
            return;
        }

        EditingActivityId = item.Id;
        ActivityTitle = item.Title;
        ActivityDescription = item.Description ?? string.Empty;
        ActivityCategory = item.Category ?? string.Empty;
        SelectedActivityStatus = item.Status;
        ActivityStartDate = item.StartDate;
        ActivityDueDate = item.DueDate;
        ActivityIsImportant = item.IsImportant;
        ActivityFormError = string.Empty;
    }

    [RelayCommand]
    private async Task ToggleActivityDoneAsync(ActivityItem? item)
    {
        if (item is null || !EnsureAuthenticated())
        {
            return;
        }

        var nextStatus = item.Status == ActivityStatus.Done ? ActivityStatus.InProgress : ActivityStatus.Done;
        var updated = new ActivityItem
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Category = item.Category,
            Status = nextStatus,
            StartDate = item.StartDate,
            DueDate = item.DueDate,
            IsImportant = item.IsImportant,
            CompletedAt = nextStatus == ActivityStatus.Done ? DateTime.UtcNow : null
        };

        await _activityItemService.UpdateAsync(_currentUserId, updated);
        await LoadActivitiesAsync();
    }

    [RelayCommand]
    private async Task DeleteActivityAsync(ActivityItem? item)
    {
        if (item is null || !EnsureAuthenticated())
        {
            return;
        }

        await _activityItemService.DeleteAsync(_currentUserId, item.Id);
        await LoadActivitiesAsync();

        if (EditingActivityId == item.Id)
        {
            ResetActivityForm();
        }
    }

    [RelayCommand]
    private void CancelActivityEdit()
    {
        ResetActivityForm();
    }

    partial void OnIsLoginModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRegisterMode));
        OnPropertyChanged(nameof(ShowLoginPanel));
        OnPropertyChanged(nameof(ShowRegisterPanel));
    }

    partial void OnIsAuthenticatedChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowAuthScreen));
        OnPropertyChanged(nameof(ShowMainShell));
    }

    partial void OnSelectedSectionChanged(string value)
    {
        OnPropertyChanged(nameof(ShowDashboard));
        OnPropertyChanged(nameof(ShowIncome));
        OnPropertyChanged(nameof(ShowExpense));
        OnPropertyChanged(nameof(ShowActivity));
        OnPropertyChanged(nameof(ShowDiary));
        OnPropertyChanged(nameof(ShowAlbums));
        OnPropertyChanged(nameof(ShowAccount));

        OnPropertyChanged(nameof(IsDashboardSelected));
        OnPropertyChanged(nameof(IsIncomeSelected));
        OnPropertyChanged(nameof(IsExpenseSelected));
        OnPropertyChanged(nameof(IsActivitySelected));
        OnPropertyChanged(nameof(IsDiarySelected));
        OnPropertyChanged(nameof(IsAlbumsSelected));
        OnPropertyChanged(nameof(IsAccountSelected));
    }

    partial void OnBalanceChanged(decimal value)
    {
        OnPropertyChanged(nameof(BalanceStatus));
    }

    partial void OnEditingIncomeIdChanged(int? value)
    {
        OnPropertyChanged(nameof(IncomeHeader));
    }

    partial void OnEditingExpenseIdChanged(int? value)
    {
        OnPropertyChanged(nameof(ExpenseHeader));
    }

    partial void OnEditingDiaryIdChanged(int? value)
    {
        OnPropertyChanged(nameof(DiaryHeader));
    }

    partial void OnDiarySearchChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredDiaryEntries));
    }

    partial void OnEditingActivityIdChanged(int? value)
    {
        OnPropertyChanged(nameof(ActivityHeader));
    }

    partial void OnActivityStatusFilterChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredActivities));
    }

    partial void OnSelectedAlbumFilterChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredAlbumImages));
        OnPropertyChanged(nameof(HasAlbumImages));
        OnPropertyChanged(nameof(ShowNoAlbumImages));

        if (IsAlbumViewerOpen)
        {
            CloseAlbumViewer();
        }
    }

    partial void OnAlbumViewerZoomChanged(double value)
    {
        RefreshAlbumViewerDimensions();
        OnPropertyChanged(nameof(AlbumViewerZoomText));
    }

    private async Task ReloadAllAsync()
    {
        if (!EnsureAuthenticated())
        {
            return;
        }

        IsBusy = true;
        StatusMessage = "Loading data...";

        try
        {
            await LoadFinanceAsync();
            await LoadDiaryAsync();
            await LoadActivitiesAsync();
            await LoadAlbumsAsync();
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadFinanceAsync()
    {
        var allEntries = await _financeEntryService.GetAllAsync(_currentUserId);
        var income = allEntries.Where(entry => entry.Type == FinanceEntryType.Income).ToList();
        var expense = allEntries.Where(entry => entry.Type == FinanceEntryType.Expense).ToList();

        ReplaceCollection(IncomeEntries, income);
        ReplaceCollection(ExpenseEntries, expense);
        ReplaceCollection(RecentEntries, allEntries.Take(7));

        UpdateDashboardMetrics();
    }

    private async Task LoadDiaryAsync()
    {
        var entries = await _diaryEntryService.GetAllAsync(_currentUserId);
        ReplaceCollection(DiaryEntries, entries);
        OnPropertyChanged(nameof(FilteredDiaryEntries));

        UpdateDashboardMetrics();
    }

    private async Task LoadActivitiesAsync()
    {
        var items = await _activityItemService.GetAllAsync(_currentUserId);
        ReplaceCollection(Activities, items);
        OnPropertyChanged(nameof(FilteredActivities));

        UpdateDashboardMetrics();
    }

    private async Task LoadAlbumsAsync()
    {
        if (IsAlbumViewerOpen)
        {
            CloseAlbumViewer();
        }

        var items = await _albumService.GetAllAsync(_currentUserId);
        ReplaceAlbumCollection(items);
        OnPropertyChanged(nameof(FilteredAlbumImages));
        OnPropertyChanged(nameof(HasAlbumImages));
        OnPropertyChanged(nameof(ShowNoAlbumImages));
    }

    private void ReplaceAlbumCollection(IEnumerable<AlbumImage> items)
    {
        foreach (var image in AlbumImages)
        {
            image.Thumbnail?.Dispose();
        }

        AlbumImages.Clear();

        foreach (var item in items)
        {
            var fullPath = _albumService.GetImagePath(_currentUserId, item.FileName);
            AlbumImages.Add(new AlbumImageCard
            {
                Id = item.Id,
                Category = item.Category,
                Caption = item.Caption,
                CreatedAtUtc = item.CreatedAtUtc,
                FileName = item.FileName,
                FilePath = fullPath,
                Thumbnail = TryCreateBitmap(fullPath)
            });
        }
    }

    private void UpdateDashboardMetrics()
    {
        var periodStart = DateTime.Today.AddDays(-30);

        IncomeTotal = IncomeEntries
            .Where(entry => entry.EntryDate.Date >= periodStart)
            .Sum(entry => entry.Amount);

        ExpenseTotal = ExpenseEntries
            .Where(entry => entry.EntryDate.Date >= periodStart)
            .Sum(entry => entry.Amount);

        Balance = IncomeTotal - ExpenseTotal;
        DiaryEntriesCount = DiaryEntries.Count;
        OpenActivitiesCount = Activities.Count(item => item.Status != ActivityStatus.Done);
    }

    private async Task SignInUserAsync(AppUser user, bool isRememberedSession)
    {
        var rememberPersisted = true;

        _currentUserId = user.Id;
        CurrentUserEmail = user.Email;
        IsAuthenticated = true;
        IsLoginMode = true;
        SelectedSection = "Dashboard";

        ResetAuthInputs();
        ResetPasswordForm();
        ResetAlbumForm();

        if (RememberMe)
        {
            rememberPersisted = await _rememberMeService.SaveAsync(user.Id, user.Email);
        }
        else
        {
            _rememberMeService.Clear();
        }

        await ReloadAllAsync();
        StatusMessage = !rememberPersisted
            ? "Signed in, but unable to persist remember-me session."
            : (isRememberedSession ? "Signed in with remembered session." : "Signed in.");
    }

    private void ClearData()
    {
        if (IsAlbumViewerOpen)
        {
            CloseAlbumViewer();
        }

        IncomeEntries.Clear();
        ExpenseEntries.Clear();
        RecentEntries.Clear();
        DiaryEntries.Clear();
        Activities.Clear();

        foreach (var image in AlbumImages)
        {
            image.Thumbnail?.Dispose();
        }

        AlbumImages.Clear();

        IncomeTotal = 0;
        ExpenseTotal = 0;
        Balance = 0;
        DiaryEntriesCount = 0;
        OpenActivitiesCount = 0;

        OnPropertyChanged(nameof(FilteredAlbumImages));
        OnPropertyChanged(nameof(HasAlbumImages));
        OnPropertyChanged(nameof(ShowNoAlbumImages));
    }

    private void ResetAuthInputs()
    {
        LoginEmail = string.Empty;
        LoginPassword = string.Empty;
        RegisterEmail = string.Empty;
        RegisterPassword = string.Empty;
        RegisterConfirmPassword = string.Empty;
    }

    private void ResetPasswordForm()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
        PasswordChangeError = string.Empty;
        PasswordChangeInfo = string.Empty;
    }

    private void ResetAlbumForm()
    {
        if (IsAlbumViewerOpen)
        {
            CloseAlbumViewer();
        }

        SelectedAlbumCategory = "Personal";
        SelectedAlbumFilter = "All";
        AlbumCaption = string.Empty;
        AlbumError = string.Empty;
        AlbumInfo = string.Empty;
    }

    private void ResetIncomeForm()
    {
        EditingIncomeId = null;
        IncomeTitle = string.Empty;
        IncomeAmount = string.Empty;
        IncomeEntryDate = DateTimeOffset.Now.Date;
        IncomeNote = string.Empty;
        IncomeFormError = string.Empty;
    }

    private void ResetExpenseForm()
    {
        EditingExpenseId = null;
        ExpenseTitle = string.Empty;
        ExpenseAmount = string.Empty;
        ExpenseEntryDate = DateTimeOffset.Now.Date;
        ExpenseNote = string.Empty;
        ExpenseFormError = string.Empty;
    }

    private void ResetDiaryForm()
    {
        EditingDiaryId = null;
        DiaryTitle = string.Empty;
        DiaryContent = string.Empty;
        DiaryEntryDate = DateTimeOffset.Now.Date;
        DiaryTags = string.Empty;
        DiaryMood = string.Empty;
        DiaryFormError = string.Empty;
    }

    private void ResetActivityForm()
    {
        EditingActivityId = null;
        ActivityTitle = string.Empty;
        ActivityCategory = string.Empty;
        SelectedActivityStatus = ActivityStatus.Planned;
        ActivityStartDate = DateTimeOffset.Now.Date;
        ActivityDueDate = DateTimeOffset.Now.Date.AddDays(1);
        ActivityDescription = string.Empty;
        ActivityIsImportant = false;
        ActivityFormError = string.Empty;
    }

    private void ShowAlbumViewerImage(bool resetZoom)
    {
        if (_albumViewerIndex < 0 || _albumViewerIndex >= _albumViewerItems.Count)
        {
            CloseAlbumViewer();
            return;
        }

        var selected = _albumViewerItems[_albumViewerIndex];
        var bitmap = TryCreateBitmap(selected.FilePath);
        if (bitmap is null)
        {
            AlbumError = "Unable to open selected image.";
            CloseAlbumViewer();
            return;
        }

        AlbumViewerImage?.Dispose();
        AlbumViewerImage = bitmap;

        AlbumViewerItem = selected;
        AlbumViewerCaption = selected.Caption;
        AlbumViewerMeta = $"{selected.Category} • {selected.CreatedAtUtc:MMM dd, yyyy}";

        if (resetZoom)
        {
            AlbumViewerZoom = 1.0;
        }

        RefreshAlbumViewerDimensions();

        OnPropertyChanged(nameof(CanViewPreviousAlbumImage));
        OnPropertyChanged(nameof(CanViewNextAlbumImage));
    }

    private void SetAlbumViewerZoom(double zoom)
    {
        var clampedZoom = Math.Clamp(zoom, 0.25, 4.0);
        AlbumViewerZoom = clampedZoom;
    }

    private void RefreshAlbumViewerDimensions()
    {
        if (AlbumViewerImage is null)
        {
            AlbumViewerImageWidth = 0;
            AlbumViewerImageHeight = 0;
            return;
        }

        AlbumViewerImageWidth = Math.Max(1, AlbumViewerImage.PixelSize.Width * AlbumViewerZoom);
        AlbumViewerImageHeight = Math.Max(1, AlbumViewerImage.PixelSize.Height * AlbumViewerZoom);
    }

    private static string BuildDownloadFileName(string caption)
    {
        var baseName = string.IsNullOrWhiteSpace(caption)
            ? $"album_image_{DateTime.Now:yyyyMMdd_HHmmss}"
            : caption.Trim();

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleanName = new string(baseName
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray())
            .Trim();

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            cleanName = $"album_image_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        return cleanName;
    }

    private static string GetDownloadsDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfile))
        {
            return Environment.CurrentDirectory;
        }

        return Path.Combine(userProfile, "Downloads");
    }

    private static string GetUniqueFilePath(string directory, string fileName, string extension)
    {
        var normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension.ToLowerInvariant();
        var candidatePath = Path.Combine(directory, $"{fileName}{normalizedExtension}");
        if (!File.Exists(candidatePath))
        {
            return candidatePath;
        }

        var counter = 1;
        while (true)
        {
            candidatePath = Path.Combine(directory, $"{fileName}_{counter}{normalizedExtension}");
            if (!File.Exists(candidatePath))
            {
                return candidatePath;
            }

            counter++;
        }
    }

    private bool EnsureAuthenticated()
    {
        return IsAuthenticated && _currentUserId > 0;
    }

    private static Bitmap? TryCreateBitmap(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using var stream = File.OpenRead(path);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> collection, IEnumerable<T> source)
    {
        collection.Clear();
        foreach (var item in source)
        {
            collection.Add(item);
        }
    }

    private static bool TryParseAmount(string raw, out decimal amount)
    {
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out amount)
               || decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    private static bool TryParseMood(string raw, out int mood)
    {
        mood = 0;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out mood))
        {
            return false;
        }

        return mood is >= 0 and <= 5;
    }

    private static string NormalizeTags(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        return string.Join(
            ", ",
            raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static bool MatchesDiarySearch(DiaryEntry entry, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var value = search.Trim();
        return entry.Title.Contains(value, StringComparison.OrdinalIgnoreCase)
               || entry.Content.Contains(value, StringComparison.OrdinalIgnoreCase)
               || entry.TagsCsv.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime ToDate(DateTimeOffset? value)
    {
        return value?.Date ?? DateTime.Today;
    }

    public sealed class AlbumImageCard
    {
        public string Id { get; init; } = string.Empty;
        public string Category { get; init; } = "Personal";
        public string Caption { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public string FilePath { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public Bitmap? Thumbnail { get; init; }
    }
}
