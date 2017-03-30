// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using SpectrumAnalysis.Common.Events;
using SpectrumAnalysis.Module.Events;

namespace SpectrumAnalysis.Module.ViewModel.Menu
{
    public class MenuViewModel : IMenuViewModel
    {
        private readonly IEventAggregator eventAggregator;

        public MenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public ICommand New
            => new DelegateCommand(() => { eventAggregator.GetEvent<NewProjectEvent>().Publish("New Analysis"); });

        public ICommand Open
            => new DelegateCommand(() => { eventAggregator.GetEvent<OpenRawDataFileEvent>().Publish("Open Raw Data"); })
        ;

        public ICommand CleanData
            => new DelegateCommand(() => { eventAggregator.GetEvent<CleanDataEvent>().Publish("Clean Data"); })
        ;

        public ICommand CleanAnalysis
            => new DelegateCommand(() => { eventAggregator.GetEvent<CleanAnalysisEvent>().Publish("Clean Analysis"); })
        ;

        public ICommand Exit
            =>
                new DelegateCommand(
                    () => { eventAggregator.GetEvent<ExitApplicationEvent>().Publish("Exit Application"); });

        public ICommand Run
            => new DelegateCommand(() => { eventAggregator.GetEvent<RunAnalysisEvent>().Publish("Run Analysis"); });

        public ICommand About =>
            new DelegateCommand(
                () => { eventAggregator.GetEvent<ShowAboutBoxEvent>().Publish("Show About Dialog"); });
    }
}