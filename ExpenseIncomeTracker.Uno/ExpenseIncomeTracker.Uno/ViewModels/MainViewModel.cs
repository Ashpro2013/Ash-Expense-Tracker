using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseIncomeTracker.Uno.Interfaces;
using ExpenseIncomeTracker.Uno.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ExpenseIncomeTracker.Uno.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string AlbumFolderName = "album";

    private readonly IAppStateService _stateService;
    private AppState _state = new();
    private string _currentUser = string.Empty;

    private string? _editingIncomeId;
    private string? _editingExpenseId;
    private string? _editingActivityId;
    private string? _editingDiaryId;
    private string? _editingPlanId;
    private string? _editingAddressBookId;
    private string? _editingPasswordDirectoryId;

    public MainViewModel(IAppStateService stateService)
    {
        _stateService = stateService;
    }

    public ObservableCollection<FinanceEntry> IncomeEntries { get; } = new();
    public ObservableCollection<FinanceEntry> ExpenseEntries { get; } = new();
    public ObservableCollection<ActivityItem> ActivityEntries { get; } = new();
    public ObservableCollection<DiaryEntry> DiaryEntries { get; } = new();
    public ObservableCollection<DayPlanItem> PlanEntries { get; } = new();
    public ObservableCollection<AlbumImageItem> AlbumEntries { get; } = new();
    public ObservableCollection<AddressBookEntry> AddressBookEntries { get; } = new();
    public ObservableCollection<PasswordDirectoryEntry> PasswordDirectoryEntries { get; } = new();
    public IReadOnlyList<string> ActivityStatuses { get; } = new[] { "Planned", "InProgress", "Done" };

    [ObservableProperty] private bool isAuthenticated;
    [ObservableProperty] private bool registerMode;
    [ObservableProperty] private string authEmail = string.Empty;
    [ObservableProperty] private string authPassword = string.Empty;
    [ObservableProperty] private string authConfirmPassword = string.Empty;
    [ObservableProperty] private string authErrorText = string.Empty;
    [ObservableProperty] private Visibility authErrorVisibility = Visibility.Collapsed;

    [ObservableProperty] private string welcomeText = string.Empty;
    [ObservableProperty] private string selectedSection = "dashboard";
    [ObservableProperty] private bool isSidebarOpen = true;

    public GridLength SidebarColumnWidth => IsSidebarOpen ? new GridLength(250) : new GridLength(0);

    partial void OnIsSidebarOpenChanged(bool value)
    {
        OnPropertyChanged(nameof(SidebarColumnWidth));
    }

    [ObservableProperty] private string incomeTotalText = string.Empty;
    [ObservableProperty] private string expenseTotalText = string.Empty;
    [ObservableProperty] private string balanceText = string.Empty;
    [ObservableProperty] private string activityCountText = string.Empty;
    [ObservableProperty] private string diaryCountText = string.Empty;
    [ObservableProperty] private string planCountText = string.Empty;

    [ObservableProperty] private string incomeTitle = string.Empty;
    [ObservableProperty] private string incomeAmount = string.Empty;
    [ObservableProperty] private string incomeNote = string.Empty;
    [ObservableProperty] private string incomeSaveButtonText = "Add Income";

    [ObservableProperty] private string expenseTitle = string.Empty;
    [ObservableProperty] private string expenseAmount = string.Empty;
    [ObservableProperty] private string expenseNote = string.Empty;
    [ObservableProperty] private string expenseSaveButtonText = "Add Expense";

    [ObservableProperty] private string activityTitle = string.Empty;
    [ObservableProperty] private string activityDueDate = string.Empty;
    [ObservableProperty] private string activityStatus = "Planned";
    [ObservableProperty] private string activityNote = string.Empty;
    [ObservableProperty] private bool activityImportant;
    [ObservableProperty] private string activitySaveButtonText = "Add Activity";

    [ObservableProperty] private string diaryTitle = string.Empty;
    [ObservableProperty] private string diaryContent = string.Empty;
    [ObservableProperty] private string diaryTags = string.Empty;
    [ObservableProperty] private string diaryMood = string.Empty;
    [ObservableProperty] private string diarySaveButtonText = "Add Diary Entry";

    [ObservableProperty] private DateTimeOffset planDate = DateTimeOffset.Now.Date;
    [ObservableProperty] private string planTitle = string.Empty;
    [ObservableProperty] private TimeSpan planStartTime = new(9, 0, 0);
    [ObservableProperty] private TimeSpan planEndTime = new(10, 0, 0);
    [ObservableProperty] private string planNotes = string.Empty;
    [ObservableProperty] private string planSaveButtonText = "Add Plan Item";

    [ObservableProperty] private string albumStatusText = string.Empty;
    [ObservableProperty] private Visibility albumStatusVisibility = Visibility.Collapsed;
    [ObservableProperty] private Brush albumStatusBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 101, 52));
    [ObservableProperty] private Visibility albumFlipVisibility = Visibility.Collapsed;
    [ObservableProperty] private Visibility albumSlideEmptyVisibility = Visibility.Visible;
    [ObservableProperty] private bool albumCanSlide;
    [ObservableProperty] private int selectedAlbumIndex = -1;
    [ObservableProperty] private string albumSlideMetaText = string.Empty;

    [ObservableProperty] private string addressName = string.Empty;
    [ObservableProperty] private string addressAddress = string.Empty;
    [ObservableProperty] private string addressPhoneNo = string.Empty;
    [ObservableProperty] private string addressEmail = string.Empty;
    [ObservableProperty] private string addressRemarks = string.Empty;
    [ObservableProperty] private string addressSaveButtonText = "Add Address Entry";

    [ObservableProperty] private string passwordTitle = string.Empty;
    [ObservableProperty] private string passwordUsername = string.Empty;
    [ObservableProperty] private string passwordValue = string.Empty;
    [ObservableProperty] private string passwordNotes = string.Empty;
    [ObservableProperty] private string passwordSaveButtonText = "Add Password Entry";

    public Visibility AuthRootVisibility => IsAuthenticated ? Visibility.Collapsed : Visibility.Visible;
    public Visibility AppRootVisibility => IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;

    public string AuthModeText => RegisterMode ? "Create Account" : "Sign In";
    public string AuthSubmitButtonText => RegisterMode ? "Register" : "Sign In";
    public string AuthSwitchButtonText => RegisterMode ? "I already have an account" : "Create account";
    public Visibility AuthConfirmPasswordVisibility => RegisterMode ? Visibility.Visible : Visibility.Collapsed;

    public Visibility DashboardSectionVisibility => GetSectionVisibility("dashboard");
    public Visibility IncomeSectionVisibility => GetSectionVisibility("income");
    public Visibility ExpenseSectionVisibility => GetSectionVisibility("expense");
    public Visibility ActivitySectionVisibility => GetSectionVisibility("activities");
    public Visibility DiarySectionVisibility => GetSectionVisibility("diary");
    public Visibility PlanSectionVisibility => GetSectionVisibility("plan");
    public Visibility AlbumSectionVisibility => GetSectionVisibility("album");
    public Visibility AddressBookSectionVisibility => GetSectionVisibility("addressbook");
    public Visibility PasswordDirectorySectionVisibility => GetSectionVisibility("passwords");

    partial void OnIsAuthenticatedChanged(bool value)
    {
        OnPropertyChanged(nameof(AuthRootVisibility));
        OnPropertyChanged(nameof(AppRootVisibility));
    }

    partial void OnRegisterModeChanged(bool value)
    {
        OnPropertyChanged(nameof(AuthModeText));
        OnPropertyChanged(nameof(AuthSubmitButtonText));
        OnPropertyChanged(nameof(AuthSwitchButtonText));
        OnPropertyChanged(nameof(AuthConfirmPasswordVisibility));
    }

    partial void OnSelectedSectionChanged(string value)
    {
        RaiseSectionVisibilityChanged();
    }

    partial void OnSelectedAlbumIndexChanged(int value)
    {
        UpdateAlbumSlideMeta();
    }

    private Visibility GetSectionVisibility(string section)
    {
        return string.Equals(SelectedSection, section, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void RaiseSectionVisibilityChanged()
    {
        OnPropertyChanged(nameof(DashboardSectionVisibility));
        OnPropertyChanged(nameof(IncomeSectionVisibility));
        OnPropertyChanged(nameof(ExpenseSectionVisibility));
        OnPropertyChanged(nameof(ActivitySectionVisibility));
        OnPropertyChanged(nameof(DiarySectionVisibility));
        OnPropertyChanged(nameof(PlanSectionVisibility));
        OnPropertyChanged(nameof(AlbumSectionVisibility));
        OnPropertyChanged(nameof(AddressBookSectionVisibility));
        OnPropertyChanged(nameof(PasswordDirectorySectionVisibility));
    }

    [RelayCommand]
    private async Task Initialize()
    {
        _state = await _stateService.LoadAsync();
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

    [RelayCommand]
    private async Task AuthSubmit()
    {
        var email = (AuthEmail ?? string.Empty).Trim().ToLowerInvariant();
        var password = AuthPassword ?? string.Empty;
        var confirm = AuthConfirmPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowAuthError("Email and password are required.");
            return;
        }

        if (RegisterMode)
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
            await _stateService.SaveAsync(_state);
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
        await _stateService.SaveAsync(_state);
        ShowApp();
    }

    [RelayCommand]
    private void AuthSwitch()
    {
        RegisterMode = !RegisterMode;
    }

    [RelayCommand]
    private async Task SignOut()
    {
        _state.CurrentUserEmail = null;
        _currentUser = string.Empty;
        await _stateService.SaveAsync(_state);
        ShowAuth();
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
    }

    private void ShowAuth()
    {
        IsAuthenticated = false;
        RegisterMode = false;
        AuthPassword = string.Empty;
        AuthConfirmPassword = string.Empty;
        ShowAuthError(string.Empty);
    }

    private void ShowApp()
    {
        IsAuthenticated = true;
        WelcomeText = $"Welcome, {_currentUser}";
        SelectedSection = "dashboard";
        RefreshView();
    }

    private void ShowAuthError(string message)
    {
        AuthErrorText = message;
        AuthErrorVisibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
    }

    [RelayCommand]
    private void SelectSection(string? section)
    {
        if (string.IsNullOrWhiteSpace(section))
        {
            return;
        }

        SelectedSection = section;
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

    private IEnumerable<AddressBookEntry> UserAddressBookEntries()
        => _state.AddressBookEntries.Where(item => item.UserEmail == _currentUser);

    private IEnumerable<PasswordDirectoryEntry> UserPasswordDirectoryEntries()
        => _state.PasswordDirectoryEntries.Where(item => item.UserEmail == _currentUser);

    private async Task SaveAndRefreshAsync()
    {
        await _stateService.SaveAsync(_state);
        RefreshView();
    }

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

        AddressBookEntries.Clear();
        foreach (var item in UserAddressBookEntries().OrderBy(item => item.Name))
        {
            AddressBookEntries.Add(item);
        }

        PasswordDirectoryEntries.Clear();
        foreach (var item in UserPasswordDirectoryEntries().OrderBy(item => item.Title))
        {
            PasswordDirectoryEntries.Add(item);
        }

        var income = UserFinance(FinanceType.Income).Sum(item => item.Amount);
        var expense = UserFinance(FinanceType.Expense).Sum(item => item.Amount);
        var balance = income - expense;
        var openActivities = UserActivities().Count(item => item.Status != Models.ActivityStatus.Done);

        IncomeTotalText = $"Total Income: {income:C}";
        ExpenseTotalText = $"Total Expense: {expense:C}";
        BalanceText = $"Balance: {balance:C}";
        ActivityCountText = $"Open Activities: {openActivities}";
        DiaryCountText = $"Diary Entries: {UserDiaries().Count()}";
        PlanCountText = $"Plan Items: {UserPlans().Count()}";

        // Explicitly notify property changes for dashboard properties
        OnPropertyChanged(nameof(IncomeTotalText));
        OnPropertyChanged(nameof(ExpenseTotalText));
        OnPropertyChanged(nameof(BalanceText));
        OnPropertyChanged(nameof(ActivityCountText));
        OnPropertyChanged(nameof(DiaryCountText));
        OnPropertyChanged(nameof(PlanCountText));
    }

    [RelayCommand]
    private async Task AddIncome()
    {
        if (!decimal.TryParse(IncomeAmount, out var amount) || amount <= 0 || string.IsNullOrWhiteSpace(IncomeTitle))
        {
            return;
        }

        if (_editingIncomeId is null)
        {
            _state.FinanceEntries.Add(new FinanceEntry
            {
                UserEmail = _currentUser,
                Type = FinanceType.Income,
                Title = IncomeTitle.Trim(),
                Amount = amount,
                Note = string.IsNullOrWhiteSpace(IncomeNote) ? null : IncomeNote.Trim(),
                EntryDate = DateTime.Today
            });
        }
        else
        {
            var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == _editingIncomeId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = IncomeTitle.Trim();
                existing.Amount = amount;
                existing.Note = string.IsNullOrWhiteSpace(IncomeNote) ? null : IncomeNote.Trim();
            }
        }

        _editingIncomeId = null;
        IncomeSaveButtonText = "Add Income";
        IncomeTitle = string.Empty;
        IncomeAmount = string.Empty;
        IncomeNote = string.Empty;
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditIncome(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Income);
        if (existing is null)
        {
            return;
        }

        _editingIncomeId = existing.Id;
        IncomeTitle = existing.Title;
        IncomeAmount = existing.Amount.ToString("0.##");
        IncomeNote = existing.Note ?? string.Empty;
        IncomeSaveButtonText = "Update Income";
    }

    [RelayCommand]
    private async Task DeleteIncome(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.FinanceEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Income);
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private async Task AddExpense()
    {
        if (!decimal.TryParse(ExpenseAmount, out var amount) || amount <= 0 || string.IsNullOrWhiteSpace(ExpenseTitle))
        {
            return;
        }

        if (_editingExpenseId is null)
        {
            _state.FinanceEntries.Add(new FinanceEntry
            {
                UserEmail = _currentUser,
                Type = FinanceType.Expense,
                Title = ExpenseTitle.Trim(),
                Amount = amount,
                Note = string.IsNullOrWhiteSpace(ExpenseNote) ? null : ExpenseNote.Trim(),
                EntryDate = DateTime.Today
            });
        }
        else
        {
            var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == _editingExpenseId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = ExpenseTitle.Trim();
                existing.Amount = amount;
                existing.Note = string.IsNullOrWhiteSpace(ExpenseNote) ? null : ExpenseNote.Trim();
            }
        }

        _editingExpenseId = null;
        ExpenseSaveButtonText = "Add Expense";
        ExpenseTitle = string.Empty;
        ExpenseAmount = string.Empty;
        ExpenseNote = string.Empty;
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditExpense(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.FinanceEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Expense);
        if (existing is null)
        {
            return;
        }

        _editingExpenseId = existing.Id;
        ExpenseTitle = existing.Title;
        ExpenseAmount = existing.Amount.ToString("0.##");
        ExpenseNote = existing.Note ?? string.Empty;
        ExpenseSaveButtonText = "Update Expense";
    }

    [RelayCommand]
    private async Task DeleteExpense(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.FinanceEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser && item.Type == FinanceType.Expense);
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private async Task AddActivity()
    {
        if (string.IsNullOrWhiteSpace(ActivityTitle))
        {
            return;
        }

        var dueDate = DateTime.Today;
        _ = DateTime.TryParse(ActivityDueDate, out dueDate);
        _ = Enum.TryParse<ActivityStatus>(ActivityStatus, true, out var status);

        if (_editingActivityId is null)
        {
            _state.ActivityItems.Add(new ActivityItem
            {
                UserEmail = _currentUser,
                Title = ActivityTitle.Trim(),
                Description = ActivityNote,
                DueDate = dueDate,
                Status = status,
                IsImportant = ActivityImportant
            });
        }
        else
        {
            var existing = _state.ActivityItems.FirstOrDefault(item => item.Id == _editingActivityId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = ActivityTitle.Trim();
                existing.Description = ActivityNote;
                existing.DueDate = dueDate;
                existing.Status = status;
                existing.IsImportant = ActivityImportant;
            }
        }

        _editingActivityId = null;
        ActivitySaveButtonText = "Add Activity";
        ActivityTitle = string.Empty;
        ActivityDueDate = string.Empty;
        ActivityNote = string.Empty;
        ActivityImportant = false;
        ActivityStatus = "Planned";
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditActivity(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.ActivityItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingActivityId = existing.Id;
        ActivityTitle = existing.Title;
        ActivityDueDate = existing.DueDate.ToString("yyyy-MM-dd");
        ActivityNote = existing.Description ?? string.Empty;
        ActivityImportant = existing.IsImportant;
        ActivityStatus = existing.Status.ToString();
        ActivitySaveButtonText = "Update Activity";
    }

    [RelayCommand]
    private async Task ToggleActivityDone(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.ActivityItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        existing.Status = existing.Status == Models.ActivityStatus.Done
            ? Models.ActivityStatus.InProgress
            : Models.ActivityStatus.Done;
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private async Task DeleteActivity(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.ActivityItems.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private async Task AddDiary()
    {
        if (string.IsNullOrWhiteSpace(DiaryTitle) || string.IsNullOrWhiteSpace(DiaryContent))
        {
            return;
        }

        var mood = 0;
        _ = int.TryParse(DiaryMood, out mood);
        mood = Math.Clamp(mood, 0, 5);

        if (_editingDiaryId is null)
        {
            _state.DiaryEntries.Add(new DiaryEntry
            {
                UserEmail = _currentUser,
                Title = DiaryTitle.Trim(),
                Content = DiaryContent.Trim(),
                EntryDate = DateTime.Today,
                Tags = DiaryTags ?? string.Empty,
                Mood = mood
            });
        }
        else
        {
            var existing = _state.DiaryEntries.FirstOrDefault(item => item.Id == _editingDiaryId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = DiaryTitle.Trim();
                existing.Content = DiaryContent.Trim();
                existing.Tags = DiaryTags ?? string.Empty;
                existing.Mood = mood;
            }
        }

        _editingDiaryId = null;
        DiarySaveButtonText = "Add Diary Entry";
        DiaryTitle = string.Empty;
        DiaryContent = string.Empty;
        DiaryTags = string.Empty;
        DiaryMood = string.Empty;
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditDiary(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.DiaryEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingDiaryId = existing.Id;
        DiaryTitle = existing.Title;
        DiaryContent = existing.Content;
        DiaryTags = existing.Tags;
        DiaryMood = existing.Mood.ToString();
        DiarySaveButtonText = "Update Diary Entry";
    }

    [RelayCommand]
    private async Task DeleteDiary(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.DiaryEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private async Task AddPlan()
    {
        if (string.IsNullOrWhiteSpace(PlanTitle))
        {
            return;
        }

        var planDateValue = PlanDate.Date;

        if (_editingPlanId is null)
        {
            _state.DayPlanItems.Add(new DayPlanItem
            {
                UserEmail = _currentUser,
                PlanDate = planDateValue,
                Title = PlanTitle.Trim(),
                StartTime = FormatPlanTime(PlanStartTime),
                EndTime = FormatPlanTime(PlanEndTime),
                Notes = PlanNotes
            });
        }
        else
        {
            var existing = _state.DayPlanItems.FirstOrDefault(item => item.Id == _editingPlanId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.PlanDate = planDateValue;
                existing.Title = PlanTitle.Trim();
                existing.StartTime = FormatPlanTime(PlanStartTime);
                existing.EndTime = FormatPlanTime(PlanEndTime);
                existing.Notes = PlanNotes;
            }
        }

        _editingPlanId = null;
        PlanSaveButtonText = "Add Plan Item";
        ResetPlanInputs();
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditPlan(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.DayPlanItems.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingPlanId = existing.Id;
        PlanDate = new DateTimeOffset(existing.PlanDate.Date);
        PlanTitle = existing.Title;
        PlanStartTime = ParsePlanTime(existing.StartTime, new TimeSpan(9, 0, 0));
        PlanEndTime = ParsePlanTime(existing.EndTime, new TimeSpan(10, 0, 0));
        PlanNotes = existing.Notes ?? string.Empty;
        PlanSaveButtonText = "Update Plan Item";
    }

    [RelayCommand]
    private async Task TogglePlan(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
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

    [RelayCommand]
    private async Task DeletePlan(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.DayPlanItems.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
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
        PlanDate = DateTimeOffset.Now.Date;
        PlanTitle = string.Empty;
        PlanStartTime = new TimeSpan(9, 0, 0);
        PlanEndTime = new TimeSpan(10, 0, 0);
        PlanNotes = string.Empty;
    }

    [RelayCommand]
    private async Task PickFromGallery()
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

    [RelayCommand]
    private async Task TakePhoto()
    {
        var capturedFile = await CaptureFromCameraAsync();
        if (capturedFile is null)
        {
            ShowAlbumStatus("Camera capture was canceled or unavailable.");
            return;
        }

        await AddImageToAlbumAsync(capturedFile, "Camera");
    }

    [RelayCommand]
    private async Task DeleteAlbumImage(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
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

    [RelayCommand]
    private void ShowPreviousAlbumImage()
    {
        if (AlbumEntries.Count == 0)
        {
            return;
        }

        if (SelectedAlbumIndex <= 0)
        {
            SelectedAlbumIndex = AlbumEntries.Count - 1;
        }
        else
        {
            SelectedAlbumIndex -= 1;
        }
    }

    [RelayCommand]
    private void ShowNextAlbumImage()
    {
        if (AlbumEntries.Count == 0)
        {
            return;
        }

        if (SelectedAlbumIndex < 0 || SelectedAlbumIndex >= AlbumEntries.Count - 1)
        {
            SelectedAlbumIndex = 0;
        }
        else
        {
            SelectedAlbumIndex += 1;
        }
    }

    private void UpdateAlbumSlideState()
    {
        var hasImages = AlbumEntries.Count > 0;
        AlbumCanSlide = hasImages;
        AlbumFlipVisibility = hasImages ? Visibility.Visible : Visibility.Collapsed;
        AlbumSlideEmptyVisibility = hasImages ? Visibility.Collapsed : Visibility.Visible;

        if (!hasImages)
        {
            SelectedAlbumIndex = -1;
            AlbumSlideMetaText = "Add images to start sliding through your album.";
            return;
        }

        if (SelectedAlbumIndex < 0 || SelectedAlbumIndex >= AlbumEntries.Count)
        {
            SelectedAlbumIndex = 0;
        }
        else
        {
            UpdateAlbumSlideMeta();
        }
    }

    private void UpdateAlbumSlideMeta()
    {
        if (SelectedAlbumIndex >= 0 && SelectedAlbumIndex < AlbumEntries.Count)
        {
            var item = AlbumEntries[SelectedAlbumIndex];
            AlbumSlideMetaText = $"{item.OriginalName} | {item.SourceType} | {item.AddedOn:g}";
        }
        else
        {
            AlbumSlideMetaText = string.Empty;
        }
    }

    private void ShowAlbumStatus(string message, bool isError = false)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            AlbumStatusText = string.Empty;
            AlbumStatusVisibility = Visibility.Collapsed;
            return;
        }

        AlbumStatusText = message;
        AlbumStatusBrush = isError
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 185, 28, 28))
            : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 101, 52));
        AlbumStatusVisibility = Visibility.Visible;
    }

    [RelayCommand]
    private async Task AddAddress()
    {
        if (string.IsNullOrWhiteSpace(AddressName))
        {
            return;
        }

        if (_editingAddressBookId is null)
        {
            _state.AddressBookEntries.Add(new AddressBookEntry
            {
                UserEmail = _currentUser,
                Name = AddressName.Trim(),
                Address = (AddressAddress ?? string.Empty).Trim(),
                PhoneNo = (AddressPhoneNo ?? string.Empty).Trim(),
                Email = (AddressEmail ?? string.Empty).Trim(),
                Remarks = (AddressRemarks ?? string.Empty).Trim()
            });
        }
        else
        {
            var existing = _state.AddressBookEntries.FirstOrDefault(item => item.Id == _editingAddressBookId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Name = AddressName.Trim();
                existing.Address = (AddressAddress ?? string.Empty).Trim();
                existing.PhoneNo = (AddressPhoneNo ?? string.Empty).Trim();
                existing.Email = (AddressEmail ?? string.Empty).Trim();
                existing.Remarks = (AddressRemarks ?? string.Empty).Trim();
            }
        }

        _editingAddressBookId = null;
        AddressSaveButtonText = "Add Address Entry";
        ResetAddressInputs();
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditAddress(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.AddressBookEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingAddressBookId = existing.Id;
        AddressName = existing.Name;
        AddressAddress = existing.Address;
        AddressPhoneNo = existing.PhoneNo;
        AddressEmail = existing.Email;
        AddressRemarks = existing.Remarks;
        AddressSaveButtonText = "Update Address Entry";
    }

    [RelayCommand]
    private async Task DeleteAddress(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.AddressBookEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private async Task AddPassword()
    {
        if (string.IsNullOrWhiteSpace(PasswordTitle))
        {
            return;
        }

        if (_editingPasswordDirectoryId is null)
        {
            _state.PasswordDirectoryEntries.Add(new PasswordDirectoryEntry
            {
                UserEmail = _currentUser,
                Title = PasswordTitle.Trim(),
                Username = (PasswordUsername ?? string.Empty).Trim(),
                Password = PasswordValue ?? string.Empty,
                Notes = (PasswordNotes ?? string.Empty).Trim()
            });
        }
        else
        {
            var existing = _state.PasswordDirectoryEntries.FirstOrDefault(item => item.Id == _editingPasswordDirectoryId && item.UserEmail == _currentUser);
            if (existing is not null)
            {
                existing.Title = PasswordTitle.Trim();
                existing.Username = (PasswordUsername ?? string.Empty).Trim();
                existing.Password = PasswordValue ?? string.Empty;
                existing.Notes = (PasswordNotes ?? string.Empty).Trim();
            }
        }

        _editingPasswordDirectoryId = null;
        PasswordSaveButtonText = "Add Password Entry";
        ResetPasswordInputs();
        await SaveAndRefreshAsync();
    }

    [RelayCommand]
    private void EditPassword(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        var existing = _state.PasswordDirectoryEntries.FirstOrDefault(item => item.Id == id && item.UserEmail == _currentUser);
        if (existing is null)
        {
            return;
        }

        _editingPasswordDirectoryId = existing.Id;
        PasswordTitle = existing.Title;
        PasswordUsername = existing.Username;
        PasswordValue = existing.Password;
        PasswordNotes = existing.Notes;
        PasswordSaveButtonText = "Update Password Entry";
    }

    [RelayCommand]
    private async Task DeletePassword(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        _state.PasswordDirectoryEntries.RemoveAll(item => item.Id == id && item.UserEmail == _currentUser);
        await SaveAndRefreshAsync();
    }

    private void ResetAddressInputs()
    {
        AddressName = string.Empty;
        AddressAddress = string.Empty;
        AddressPhoneNo = string.Empty;
        AddressEmail = string.Empty;
        AddressRemarks = string.Empty;
    }

    private void ResetPasswordInputs()
    {
        PasswordTitle = string.Empty;
        PasswordUsername = string.Empty;
        PasswordValue = string.Empty;
        PasswordNotes = string.Empty;
    }
}
