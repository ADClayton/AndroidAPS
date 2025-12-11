using AndroidAPSMaui.Graphs;
using AndroidAPSMaui.Services;
using AndroidAPSMaui.ViewModels;
using System.Collections.Specialized;

namespace AndroidAPSMaui.Pages;

public partial class MainPage : ContentPage
{
    private readonly GraphViewModel _viewModel;
    private readonly BgGraphDrawable _drawable = new();

    public MainPage()
    {
        InitializeComponent();
        _viewModel = ServiceResolver.Resolve<GraphViewModel>() ?? throw new InvalidOperationException("GraphViewModel not available");
        BindingContext = _viewModel;
        Graph.Drawable = _drawable;
        _viewModel.Readings.CollectionChanged += OnDataChanged;
        _viewModel.PumpEvents.CollectionChanged += OnDataChanged;
        UpdateGraph();
    }

    private void OnDataChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateGraph();

    private void UpdateGraph()
    {
        _drawable.Update(_viewModel.Readings.ToList(), _viewModel.PumpEvents.ToList());
        Graph.Invalidate();
    }
}
