using System.Windows;
using System.Windows.Media;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EarTrumpet.Interop.Helpers;

namespace EarTrumpet.Benchmarks;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BenchmarkRunner.Run<MainWindow>();
        EnableAcrylic();
    }

    [Benchmark]
    public void EnableAcrylic() => AccentPolicyLibrary.EnableAcrylic(this, Colors.Red, Interop.User32.AccentFlags.DrawAllBorders);
}