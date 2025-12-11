using AndroidAPSMaui.Services;
using AndroidAPSMaui.ViewModels;

namespace AndroidAPSMaui.Pages;

public partial class PumpConnectionPage : ContentPage
{
    public PumpConnectionPage()
    {
        InitializeComponent();
        BindingContext = ServiceResolver.Resolve<PumpConnectionViewModel>() ?? throw new InvalidOperationException("PumpConnectionViewModel not available");
    }
}
