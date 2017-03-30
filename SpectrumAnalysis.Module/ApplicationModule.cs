// Copyright (c) 2015-2017 Bartosz Rachwal.

using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using SpectrumAnalysis.Algorithms.Model;
using SpectrumAnalysis.Algorithms.Peaks;
using SpectrumAnalysis.Algorithms.Smoothing;
using SpectrumAnalysis.Analysis;
using SpectrumAnalysis.Common.Regions;
using SpectrumAnalysis.Module.View.Main;
using SpectrumAnalysis.Module.View.Menu;
using SpectrumAnalysis.Module.View.StatusBar;
using SpectrumAnalysis.Module.ViewModel.Main;
using SpectrumAnalysis.Module.ViewModel.Menu;
using SpectrumAnalysis.Module.ViewModel.StatusBar;

namespace SpectrumAnalysis.Module
{
    public class ApplicationModule : IModule
    {
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;

        public ApplicationModule(IUnityContainer container, IRegionManager regionManager)
        {
            this.container = container;
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            RegisterTypes();
            RegisterRegions();
        }

        private void RegisterTypes()
        {
            container.RegisterType<ILorentzian, Lorentzian>();
            container.RegisterType<ISmoother, GaussianSmoother>();
            container.RegisterType<IPeaksAnalyzer, PeaksAnalyzer>();
            container.RegisterType<IPeaksFinder, PeaksFinder>();
            container.RegisterType<IPeaksRepository, PeaksRepository>(new ContainerControlledLifetimeManager());

            container.RegisterType<IMenuViewModel, MenuViewModel>();
            container.RegisterType<MenuView>();

            container.RegisterType<IMainViewModel, MainViewModel>();
            container.RegisterType<MainView>();

            container.RegisterType<IStatusBarViewModel, StatusBarViewModel>();
            container.RegisterType<StatusBarView>();
        }

        private void RegisterRegions()
        {
            regionManager.RegisterViewWithRegion(RegionNames.MenuRegion, () => container.Resolve<MenuView>());
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => container.Resolve<MainView>());
            regionManager.RegisterViewWithRegion(RegionNames.StatusBarRegion, () => container.Resolve<StatusBarView>());
        }
    }
}