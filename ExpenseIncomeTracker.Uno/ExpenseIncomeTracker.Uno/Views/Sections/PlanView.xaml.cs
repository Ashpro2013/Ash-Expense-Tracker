namespace ExpenseIncomeTracker.Uno.Views.Sections;

public sealed partial class PlanView : UserControl
{
    public event RoutedEventHandler? AddPlanRequested;
    public event RoutedEventHandler? EditPlanRequested;
    public event RoutedEventHandler? TogglePlanRequested;
    public event RoutedEventHandler? DeletePlanRequested;

    public PlanView()
    {
        InitializeComponent();
    }

    private void OnAddPlanClicked(object sender, RoutedEventArgs e) => AddPlanRequested?.Invoke(sender, e);
    private void OnEditPlanClicked(object sender, RoutedEventArgs e) => EditPlanRequested?.Invoke(sender, e);
    private void OnTogglePlanClicked(object sender, RoutedEventArgs e) => TogglePlanRequested?.Invoke(sender, e);
    private void OnDeletePlanClicked(object sender, RoutedEventArgs e) => DeletePlanRequested?.Invoke(sender, e);
}
