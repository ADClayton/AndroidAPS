using OmnipodDashController.ViewModels;

namespace OmnipodDashController.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
