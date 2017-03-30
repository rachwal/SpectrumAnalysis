// Copyright (c) 2015-2017 Bartosz Rachwal.

using Prism.Events;
using SpectrumAnalysis.Common.Events;

namespace SpectrumAnalysis.Shell.ViewModel
{
    public class ShellViewModel : IShellViewModel
    {
        public ShellViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ExitApplicationEvent>().Subscribe(ExitApplication);
        }

        private void ExitApplication(string obj)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}