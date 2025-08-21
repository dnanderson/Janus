using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Janus.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UutViewModelFactory _uutViewModelFactory;

        public MainViewModel(HomeViewModel homeViewModel, IServiceScopeFactory scopeFactory, UutViewModelFactory uutViewModelFactory)
        {
            _scopeFactory = scopeFactory;
            _uutViewModelFactory = uutViewModelFactory;

            RunningUuts = new ObservableCollection<UutViewModel>();
            homeViewModel.RunningUuts = RunningUuts;
            homeViewModel.OnBeginTest += OnBeginTest;

            Tabs = new ObservableCollection<object>
            {
                homeViewModel
            };
            _selectedTab = homeViewModel;
        }

        private void OnBeginTest(object? sender, BeginTestEventArgs e)
        {
            var uutScope = _scopeFactory.CreateScope();
            var uutViewModel = _uutViewModelFactory(e.SerialNumber, e.OperatorName, e.TestDescription, e.Drawer);

            uutViewModel.OnCloseTest += OnUutRequestClose;
            Tabs.Add(uutViewModel);
            RunningUuts.Add(uutViewModel);
            SelectedTab = uutViewModel;
        }

        private void OnUutRequestClose(object? sender, UutViewModel uutViewModel)
        {
            uutViewModel.OnCloseTest -= OnUutRequestClose;
            Tabs.Remove(uutViewModel);
            RunningUuts.Remove(uutViewModel);
            uutViewModel.Dispose();
        }

        public ObservableCollection<object> Tabs { get; }
        public ObservableCollection<UutViewModel> RunningUuts { get; }

        [ObservableProperty]
        private object _selectedTab;

        public void Dispose()
        {
            foreach (var tab in Tabs.OfType<IDisposable>())
            {
                tab.Dispose();
            }
        }
    }
}
