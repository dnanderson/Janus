using Autofac;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac.Core;

namespace Janus.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly ILifetimeScope _scope;

        public MainViewModel(HomeViewModel homeViewModel, ILifetimeScope scope)
        {
            _scope = scope;

            RunningUuts = new ObservableCollection<UutViewModel>();
            homeViewModel.RunningUuts = RunningUuts;
            homeViewModel.OnBeginTest += OnBeginTest;

            Tabs = new ObservableCollection<object>
            {
                homeViewModel
            };
            SelectedTab = homeViewModel;
        }

        private void OnBeginTest(object? sender, BeginTestEventArgs e)
        {
            var uutViewModel = _scope.Resolve<UutViewModel>(
                new NamedParameter("serialNumber", e.SerialNumber),
                new NamedParameter("operatorName", e.OperatorName),
                new NamedParameter("testDescription", e.TestDescription),
                new NamedParameter("drawer", e.Drawer));

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

        private object _selectedTab;
        public object SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            foreach (var tab in Tabs.OfType<IDisposable>())
            {
                tab.Dispose();
            }
            _scope.Dispose();
        }
    }
}
