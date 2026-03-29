using System.Collections.ObjectModel;
using System.IO;
using ExpenseIncomeTracker.Uno.Models;
using ExpenseIncomeTracker.Uno.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ExpenseIncomeTracker.Uno;

public sealed partial class MainPage : Page
{
    private const string AlbumFolderName = "album";
    private readonly LocalStoreService _store = new();
    private AppState _state = new();
    private string _currentUser = string.Empty;
    private bool _registerMode;

    private string? _editingIncomeId;
    private string? _editingExpenseId;
    private string? _editingActivityId;
    private string? _editingDiaryId;
    private string? _editingPlanId;

    public ObservableCollection<FinanceEntry> IncomeEntries { get; } = new();
    public ObservableCollection<FinanceEntry> ExpenseEntries { get; } = new();
    public ObservableCollection<ActivityItem> ActivityEntries { get; } = new();
    public ObservableCollection<DiaryEntry> DiaryEntries { get; } = new();
    public ObservableCollection<DayPlanItem> PlanEntries { get; } = new();
    public ObservableCollection<AlbumImageItem> AlbumEntries { get; } = new();

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _state = await _store.LoadAsync();
        if (!string.IsNullOrWhiteSpace(_state.CurrentUserEmail))
        {
            _currentUser = _state.CurrentUserEmail;
            ShowApp();
        }
        else
        {
            ShowAuth();
        }
    }

    private void ShowAuth()
    {
        AuthRoot.Visibility = Visibility.Visible;
        AppRoot.Visibility = Visibility.Collapsed;
        SetAuthMode(false);
    }

    private void ShowApp()
    {
        AuthRoot.Visibility = Visibility.Collapsed;
        AppRoot.Visibility = Visibility.Visible;
        WelcomeText.Text = $"Welcome, {_currentUser}";
        RefreshView();
        RootNav.SelectedItem = RootNav.MenuItems[0];
        ApplyNavMenuColors("dashboard");
        SwitchSection("dashboard");
    }

    private void SetAuthMode(bool register)
    {
        _registerMode = register;
        AuthModeText.Text = register ? "Create Account" : "Sign In";
        AuthSubmitButton.Content = register ? "Register" : "Sign In";
        AuthSwitchButton.Content = register ? "I already have an account" : "Create account";
        AuthConfirmPasswordBox.Visibility = register ? Visibility.Visible : Visibility.Collapsed;
        AuthErrorText.Visibility = Visibility.Collapsed;
        AuthErrorText.Text = string.Empty;
    }

    private async void AuthSubmitClicked(object sender, RoutedEventArgs e)
    {
        var email = (AuthEmailBox.Text ?? string.Empty).Trim().ToLowerInvariant();
        var password = AuthPasswordBox.Password ?? string.Empty;
        var confirm = AuthConfirmPasswordBox.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowAuthError("Email and password are required.");
            return;
        }

        if (_registerMode)
        {
            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                ShowAuthError("Passwords do not match.");
                return;
            }

            if (_state.Users.Any(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                ShowAuthError("Account already exists.");
                return;
            }

            _state.Users.Add(new UserAccount { Email = email, Password = password });
            _state.CurrentUserEmail = email;
            _currentUser = email;
            await _store.SaveAsync(_state);
            ShowApp();
            return;
        }

        var account = _state.Users.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)
            && user.Password == password);

        if (account is null)
        {
            ShowAuthError("Invalid credentials.");
            return;
        }

        _state.CurrentUserEmail = account.Email;
        _currentUser = account.Email;
        await _store.SaveAsync(_state);
        ShowApp();
    }

    private void AuthSwitchClicked(object sender, RoutedEventArgs e)
    {
        SetAuthMode(!_registerMode);
    }

    private async void SignOutClicked(object sender, RoutedEventArgs e)
    {
        _state.CurrentUserEmail = null;
        _currentUser = string.Empty;
        await _store.SaveAsync(_state);
        ShowAuth();
    }

    private void ShowAuthError(string message)
    {
        AuthErrorText.Text = message;
        AuthErrorText.Visibility = Visibility.Visible;
    }

    private IEnumerable<FinanceEntry> UserFinance(FinanceType type)
        => _state.FinanceEntries.Where(item => item.UserEmail == _currentUser && item.Type == type);

    private IEnumerable<ActivityItem> UserActivities()
        => _state.ActivityItems.Where(item => item.UserEmail == _currentUser);

    private IEnumerable<DiaryEntry> UserDiaries()
        => _state.DiaryEntries.Where(item => item.UserEmail == _currentUser);

    private IEnumerable<DayPlanItem> UserPlans()
        => _state.DayPlanItems.Where(item => item.UserEmail == _currentUser);

    private IEnumerable<AlbumImageItem> UserAlbumImages()
        => _state.AlbumImages.Where(item => item.UserEmail == _currentUser);

    private void RefreshView()
    {
        IncomeEntries.Clear();
        foreach (var entry in UserFinance(FinanceType.Income).OrderByDescending(item => item.EntryDate))
        {
            IncomeEntries.Add(entry);
        }

        ExpenseEntries.Clear();
        foreach (var entry in UserFinance(FinanceType.Expense).OrderByDescending(item => item.EntryDate))
        {
            ExpenseEntries.Add(entry);
        }

        ActivityEntries.Clear();
        foreach (var item in UserActivities().OrderBy(item => item.DueDate))
        {
            ActivityEntries.Add(item);
        }

        DiaryEntries.Clear();
        foreach (var item in UserDiaries().OrderByDescending(item => item.EntryDate))
        {
            DiaryEntries.Add(item);
        }

        PlanEntries.Clear();
        foreach (var item in UserPlans().OrderBy(item => item.PlanDate).ThenBy(item => item.StartTime))
        {
            PlanEntries.Add(item);
        }

        AlbumEntries.Clear();
        foreach (var item in UserAlbumImages().OrderByDescending(item => item.AddedOn))
        {
            AlbumEntries.Add(item);
        }
        UpdateAlbumSlideState();

        var income = UserFinance(FinanceType.Income).Sum(item => item.Amount);
        var expense = UserFinance(FinanceType.Expense).Sum(item => item.Amount);
        var balance = income - expense;
        var openActivities = UserActivities().Count(item => item.Status != ActivityStatus.Done);

        IncomeTotalText.Text = $"Total Income: {income:C}";
        ExpenseTotalText.Text = $"Total Expense: {expense:C}";
        BalanceText.Text = $"Balance: {balance:C}";
        ActivityCountText.Text = $"Open Activities: {openActivities}";
        DiaryCountText.Text = $"Diary Entries: {UserDiaries().Count()}";
        PlanCountText.Text = $"Plan Items: {UserPlans().Count()}";
    }

    private async Task SaveAndRefreshAsync()
    {
        await _store.SaveAsync(_state);
        RefreshView();
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section)
        {
            ApplyNavMenuColors(section);
            SwitchSection(section);
        }
    }

    private void ApplyNavMenuColors(string selectedSection)
    {
        foreach (var menuItem in RootNav.MenuItems.OfType<NavigationViewItem>())
        {
            var isSelected = string.Equals(menuItem.Tag?.ToString(), selectedSection, StringComparison.OrdinalIgnoreCase);
            menuItem.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 23, 42));
            menuItem.Background = isSelected
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 204, 251, 241))
                : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }
    }

    private void SwitchSection(string section)
    {
        DashboardSection.Visibility = Visibility.Collapsed;
        IncomeSection.Visibility = Visibility.Collapsed;
        ExpenseSection.Visibility = Visibility.Collapsed;
        ActivitySection.Visibility = Visibility.Collapsed;
        DiarySection.Visibility = Visibility.Collapsed;
        PlanSection.Visibility = Visibility.Collapsed;
        AlbumSection.Visibility = Visibility.Collapsed;

        switch (section)
        {
            case "income":
                IncomeSection.Visibility = Visibility.Visible;
                break;
            case "expense":
                ExpenseSection.Visibility = Visibility.Visible;
                break;
            case "activities":
                ActivitySection.Visibility = Visibility.Visible;
                break;
            case "diary":
                DiarySection.Visibility = Visibility.Visible;
                break;
            case "plan":
                PlanSection.Visibility = Visibility.Visible;
                break;
            case "album":
                AlbumSection.Visibility = Visibility.Visible;
                break;
            default:
                DashboardSection.Visibility = Visibility.Visible;
                break;
        }
    }

    private async void AddIncomeClicked(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(IncomeAmountBox.Text, out var amount) || amount <= 0 || string.IsNullOrWhiteSpace(IncomeTitleBox.Text))
        {
            return;
        }

        if (_editingIncomeId is null)
        {
            _state.FinanceEntries.Add(new FinanceEntry
            {
                UserEmail = _currentUser,
                Type = FinanceType.Income,
                Title = IncomeTitleBox.Text.Trim(),
                Amount = amount,
                Note = string.IsNullOrWhiteSpace(IncomeNoteBox.Text) ? null : IncomeNoteBox.Text.Trim(),
                EntryDate = DateTime.Today
            });
        }
        else
        {
            var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == _editingIncomeId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = IncomeTitleBox.Text.Trim();
                existing.Amount = amount;
                existing.Note = string.IsNullOrWhiteSpace(IncomeNoteBox.Text) ? null : IncomeNoteBox.Text.Trim();
            }
        }

        _editingIncomeId = null;
        IncomeSaveButton.Content = "Add Income";
        IncomeTitleBox.Text = string.Empty;
        IncomeAmountBox.Text = string.Empty;
        IncomeNoteBox.Text = string.Empty;
        await SaveAndRefreshAsync();
    }

    private void EditIncomeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Income);
        if (existing is null)
        {
            return;
        }

        _editingIncomeId = existing.Id;
        IncomeTitleBox.Text = existing.Title;
        IncomeAmountBox.Text = existing.Amount.ToString("0.##");
        IncomeNoteBox.Text = existing.Note ?? string.Empty;
        IncomeSaveButton.Content = "Update Income";
    }

    private async void DeleteIncomeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        _state.FinanceEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Income);
        await SaveAndRefreshAsync();
    }

    private async void AddExpenseClicked(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(ExpenseAmountBox.Text, out var amount) || amount <= 0 || string.IsNullOrWhiteSpace(ExpenseTitleBox.Text))
        {
            return;
        }

        if (_editingExpenseId is null)
        {
            _state.FinanceEntries.Add(new FinanceEntry
            {
                UserEmail = _currentUser,
                Type = FinanceType.Expense,
                Title = ExpenseTitleBox.Text.Trim(),
                Amount = amount,
                Note = string.IsNullOrWhiteSpace(ExpenseNoteBox.Text) ? null : ExpenseNoteBox.Text.Trim(),
                EntryDate = DateTime.Today
            });
        }
        else
        {
            var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == _editingExpenseId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = ExpenseTitleBox.Text.Trim();
                existing.Amount = amount;
                existing.Note = string.IsNullOrWhiteSpace(ExpenseNoteBox.Text) ? null : ExpenseNoteBox.Text.Trim();
            }
        }

        _editingExpenseId = null;
        ExpenseSaveButton.Content = "Add Expense";
        ExpenseTitleBox.Text = string.Empty;
        ExpenseAmountBox.Text = string.Empty;
        ExpenseNoteBox.Text = string.Empty;
        await SaveAndRefreshAsync();
    }

    private void EditExpenseClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Expense);
        if (existing is null)
        {
            return;
        }

        _editingExpenseId = existing.Id;
        ExpenseTitleBox.Text = existing.Title;
        ExpenseAmountBox.Text = existing.Amount.ToString("0.##");
        ExpenseNoteBox.Text = existing.Note ?? string.Empty;
        ExpenseSaveButton.Content = "Update Expense";
    }

    private async void DeleteExpenseClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        _state.FinanceEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Expense);
        await SaveAndRefreshAsync();
    }

    private async void AddActivityClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ActivityTitleBox.Text))
        {
            return;
        }

        var dueDate = DateTime.Today;
        _ = DateTime.TryParse(ActivityDueDateBox.Text, out dueDate);
        Enum.TryParse<ActivityStatus>(ActivityStatusBox.SelectedItem?.ToString(), out var status);

        if (_editingActivityId is null)
        {
            _state.ActivityItems.Add(new ActivityItem
            {
                UserEmail = _currentUser,
                Title = ActivityTitleBox.Text.Trim(),
                Description = ActivityNoteBox.Text,
                DueDate = dueDate,
                Status = status,
                IsImportant = ActivityImportantBox.IsChecked == true
            });
        }
        else
        {
            var existing = _state.ActivityItems.FirstOrDefault(item => item.Id == _editingActivityId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = ActivityTitleBox.Text.Trim();
                existing.Description = ActivityNoteBox.Text;
                existing.DueDate = dueDate;
                existing.Status = status;
                existing.IsImportant = ActivityImportantBox.IsChecked == true;
            }
        }

        _editingActivityId = null;
        ActivitySaveButton.Content = "Add Activity";
        ActivityTitleBox.Text = string.Empty;
        ActivityDueDateBox.Text = string.Empty;
        ActivityNoteBox.Text = string.Empty;
        ActivityImportantBox.IsChecked = false;
        ActivityStatusBox.SelectedIndex = 0;
        await SaveAndRefreshAsync();
    }

    private void EditActivityClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.ActivityItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingActivityId = existing.Id;
        ActivityTitleBox.Text = existing.Title;
        ActivityDueDateBox.Text = existing.DueDate.ToString("yyyy-MM-dd");
        ActivityNoteBox.Text = existing.Description ?? string.Empty;
        ActivityImportantBox.IsChecked = existing.IsImportant;
        ActivityStatusBox.SelectedItem = existing.Status.ToString();
        ActivitySaveButton.Content = "Update Activity";
    }

    private async void ToggleActivityDoneClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.ActivityItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        existing.Status = existing.Status == ActivityStatus.Done ? ActivityStatus.InProgress : ActivityStatus.Done;
        await SaveAndRefreshAsync();
    }

    private async void DeleteActivityClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        _state.ActivityItems.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    private async void AddDiaryClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DiaryTitleBox.Text) || string.IsNullOrWhiteSpace(DiaryContentBox.Text))
        {
            return;
        }

        var mood = 0;
        _ = int.TryParse(DiaryMoodBox.Text, out mood);
        mood = Math.Clamp(mood, 0, 5);

        if (_editingDiaryId is null)
        {
            _state.DiaryEntries.Add(new DiaryEntry
            {
                UserEmail = _currentUser,
                Title = DiaryTitleBox.Text.Trim(),
                Content = DiaryContentBox.Text.Trim(),
                EntryDate = DateTime.Today,
                Tags = DiaryTagsBox.Text ?? string.Empty,
                Mood = mood
            });
        }
        else
        {
            var existing = _state.DiaryEntries.FirstOrDefault(item => item.Id == _editingDiaryId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = DiaryTitleBox.Text.Trim();
                existing.Content = DiaryContentBox.Text.Trim();
                existing.Tags = DiaryTagsBox.Text ?? string.Empty;
                existing.Mood = mood;
            }
        }

        _editingDiaryId = null;
        DiarySaveButton.Content = "Add Diary Entry";
        DiaryTitleBox.Text = string.Empty;
        DiaryContentBox.Text = string.Empty;
        DiaryTagsBox.Text = string.Empty;
        DiaryMoodBox.Text = string.Empty;
        await SaveAndRefreshAsync();
    }

    private void EditDiaryClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.DiaryEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingDiaryId = existing.Id;
        DiaryTitleBox.Text = existing.Title;
        DiaryContentBox.Text = existing.Content;
        DiaryTagsBox.Text = existing.Tags;
        DiaryMoodBox.Text = existing.Mood.ToString();
        DiarySaveButton.Content = "Update Diary Entry";
    }

    private async void DeleteDiaryClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        _state.DiaryEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    private async void AddPlanClicked(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PlanTitleBox.Text))
        {
            return;
        }

        var planDate = PlanDatePicker.Date.Date;
        var startTime = PlanStartPicker.Time;
        var endTime = PlanEndPicker.Time;

        if (_editingPlanId is null)
        {
            _state.DayPlanItems.Add(new DayPlanItem
            {
                UserEmail = _currentUser,
                PlanDate = planDate,
                Title = PlanTitleBox.Text.Trim(),
                StartTime = FormatPlanTime(startTime),
                EndTime = FormatPlanTime(endTime),
                Notes = PlanNotesBox.Text
            });
        }
        else
        {
            var existing = _state.DayPlanItems.FirstOrDefault(item => item.Id == _editingPlanId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.PlanDate = planDate;
                existing.Title = PlanTitleBox.Text.Trim();
                existing.StartTime = FormatPlanTime(startTime);
                existing.EndTime = FormatPlanTime(endTime);
                existing.Notes = PlanNotesBox.Text;
            }
        }

        _editingPlanId = null;
        PlanSaveButton.Content = "Add Plan Item";
        ResetPlanInputs();
        await SaveAndRefreshAsync();
    }

    private void EditPlanClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.DayPlanItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingPlanId = existing.Id;
        PlanDatePicker.Date = new DateTimeOffset(existing.PlanDate.Date);
        PlanTitleBox.Text = existing.Title;
        PlanStartPicker.Time = ParsePlanTime(existing.StartTime, new TimeSpan(9, 0, 0));
        PlanEndPicker.Time = ParsePlanTime(existing.EndTime, new TimeSpan(10, 0, 0));
        PlanNotesBox.Text = existing.Notes ?? string.Empty;
        PlanSaveButton.Content = "Update Plan Item";
    }

    private static string FormatPlanTime(TimeSpan value)
    {
        return value.ToString(@"hh\:mm");
    }

    private static TimeSpan ParsePlanTime(string? value, TimeSpan fallback)
    {
        if (!string.IsNullOrWhiteSpace(value) && TimeSpan.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private void ResetPlanInputs()
    {
        PlanDatePicker.Date = DateTimeOffset.Now.Date;
        PlanTitleBox.Text = string.Empty;
        PlanStartPicker.Time = new TimeSpan(9, 0, 0);
        PlanEndPicker.Time = new TimeSpan(10, 0, 0);
        PlanNotesBox.Text = string.Empty;
    }

    private async void TogglePlanClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.DayPlanItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        existing.IsCompleted = !existing.IsCompleted;
        await SaveAndRefreshAsync();
    }

    private async void DeletePlanClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        _state.DayPlanItems.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    private async void PickFromGalleryClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".bmp");

            var selectedFile = await picker.PickSingleFileAsync();
            if (selectedFile is null)
            {
                ShowAlbumStatus("Image selection was canceled.");
                return;
            }

            await AddImageToAlbumAsync(selectedFile, "Gallery");
        }
        catch
        {
            ShowAlbumStatus("Unable to open gallery picker.", isError: true);
        }
    }

    private async void TakePhotoClicked(object sender, RoutedEventArgs e)
    {
        var capturedFile = await CaptureFromCameraAsync();
        if (capturedFile is null)
        {
            ShowAlbumStatus("Camera capture was canceled or unavailable.");
            return;
        }

        await AddImageToAlbumAsync(capturedFile, "Camera");
    }

    private async void DeleteAlbumImageClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id })
        {
            return;
        }

        var existing = _state.AlbumImages.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        try
        {
            var albumFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AlbumFolderName, CreationCollisionOption.OpenIfExists);
            var file = await albumFolder.TryGetItemAsync(existing.FileName) as StorageFile;
            if (file is not null)
            {
                await file.DeleteAsync();
            }
        }
        catch
        {
            // Keep metadata cleanup even if physical file deletion fails.
        }

        _state.AlbumImages.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
        ShowAlbumStatus("Image removed from album.");
    }

    private async Task AddImageToAlbumAsync(StorageFile sourceFile, string sourceType)
    {
        try
        {
            var copied = await CopyToAlbumAsync(sourceFile);
            _state.AlbumImages.Add(new AlbumImageItem
            {
                UserEmail = _currentUser,
                FileName = copied.Name,
                OriginalName = sourceFile.Name,
                SourceType = sourceType,
                AddedOn = DateTime.Now
            });

            await SaveAndRefreshAsync();
            ShowAlbumStatus($"Added from {sourceType}: {sourceFile.Name}");
        }
        catch
        {
            ShowAlbumStatus("Could not save the selected image.", isError: true);
        }
    }

    private async Task<StorageFile> CopyToAlbumAsync(StorageFile sourceFile)
    {
        var albumFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AlbumFolderName, CreationCollisionOption.OpenIfExists);
        var extension = Path.GetExtension(sourceFile.Name);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var targetName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        return await sourceFile.CopyAsync(albumFolder, targetName, NameCollisionOption.GenerateUniqueName);
    }

    private async Task<StorageFile?> CaptureFromCameraAsync()
    {
#if __ANDROID__
        try
        {
            var captureUi = new Windows.Media.Capture.CameraCaptureUI();
            return await captureUi.CaptureFileAsync(Windows.Media.Capture.CameraCaptureUIMode.Photo);
        }
        catch
        {
            return null;
        }
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    private void ShowPreviousAlbumImageClicked(object sender, RoutedEventArgs e)
    {
        if (AlbumEntries.Count == 0)
        {
            return;
        }

        var currentIndex = AlbumFlipView.SelectedIndex;
        if (currentIndex <= 0)
        {
            AlbumFlipView.SelectedIndex = AlbumEntries.Count - 1;
        }
        else
        {
            AlbumFlipView.SelectedIndex = currentIndex - 1;
        }

        UpdateAlbumSlideMeta();
    }

    private void ShowNextAlbumImageClicked(object sender, RoutedEventArgs e)
    {
        if (AlbumEntries.Count == 0)
        {
            return;
        }

        var currentIndex = AlbumFlipView.SelectedIndex;
        if (currentIndex < 0 || currentIndex >= AlbumEntries.Count - 1)
        {
            AlbumFlipView.SelectedIndex = 0;
        }
        else
        {
            AlbumFlipView.SelectedIndex = currentIndex + 1;
        }

        UpdateAlbumSlideMeta();
    }

    private void AlbumFlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateAlbumSlideMeta();
    }

    private void UpdateAlbumSlideState()
    {
        var hasImages = AlbumEntries.Count > 0;
        AlbumPrevButton.IsEnabled = hasImages;
        AlbumNextButton.IsEnabled = hasImages;
        AlbumFlipView.Visibility = hasImages ? Visibility.Visible : Visibility.Collapsed;
        AlbumSlideEmptyText.Visibility = hasImages ? Visibility.Collapsed : Visibility.Visible;

        if (!hasImages)
        {
            AlbumSlideMetaText.Text = "Add images to start sliding through your album.";
            return;
        }

        if (AlbumFlipView.SelectedIndex < 0 || AlbumFlipView.SelectedIndex >= AlbumEntries.Count)
        {
            AlbumFlipView.SelectedIndex = 0;
        }

        UpdateAlbumSlideMeta();
    }

    private void UpdateAlbumSlideMeta()
    {
        if (AlbumFlipView.SelectedItem is AlbumImageItem item)
        {
            AlbumSlideMetaText.Text = $"{item.OriginalName} | {item.SourceType} | {item.AddedOn:g}";
        }
        else
        {
            AlbumSlideMetaText.Text = string.Empty;
        }
    }

    private void ShowAlbumStatus(string message, bool isError = false)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            AlbumStatusText.Text = string.Empty;
            AlbumStatusText.Visibility = Visibility.Collapsed;
            return;
        }

        AlbumStatusText.Text = message;
        AlbumStatusText.Foreground = isError
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 185, 28, 28))
            : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 101, 52));
        AlbumStatusText.Visibility = Visibility.Visible;
    }
}
