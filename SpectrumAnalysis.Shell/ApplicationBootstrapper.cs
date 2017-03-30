// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Globalization;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Unity;
using SpectrumAnalysis.Data.Cursor;
using SpectrumAnalysis.Data.Intensity;
using SpectrumAnalysis.Data.Raw;
using SpectrumAnalysis.Data.Voltage;
using SpectrumAnalysis.Dialogs;
using SpectrumAnalysis.Module;
using SpectrumAnalysis.Shell.View;
using SpectrumAnalysis.Shell.ViewModel;

namespace SpectrumAnalysis.Shell
{
    public class ApplicationBootstrapper : UnityBootstrapper
    {
        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            var moduleCatalog = (ModuleCatalog) ModuleCatalog;
            moduleCatalog.AddModule(typeof(ApplicationModule));
        }

        protected override void ConfigureContainer()
        {
            Container.RegisterType<IRawDataReader, RawDataReader>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISpectraRepository, SpectraRepository>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IVoltageRepository, VoltageRepository>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDataCursor, DataCursor>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogLoader, DialogLoader>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IShellViewModel, ShellViewModel>();

            base.ConfigureContainer();
        }

        protected override DependencyObject CreateShell()
        {
            var view = Container.TryResolve<ShellView>();
            return view;
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Container.Resolve<IDialogLoader>();

            System.Windows.Application.Current.MainWindow = (Window) Shell;
            System.Windows.Application.Current.MainWindow.Show();
        }
    }
}