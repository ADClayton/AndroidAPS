using Microsoft.Maui.Controls;
using OmnipodDashController.Views;

namespace OmnipodDashController;

public partial class App : Application
{
    public App(MainPage page)
    {
        InitializeComponent();

        MainPage = new NavigationPage(page)
        {
            BarTextColor = Colors.White,
            BarBackgroundColor = Colors.DarkSlateBlue
        };
    }
}
