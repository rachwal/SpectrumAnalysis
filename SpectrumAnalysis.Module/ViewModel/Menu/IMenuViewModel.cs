// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Windows.Input;

namespace SpectrumAnalysis.Module.ViewModel.Menu
{
    public interface IMenuViewModel
    {
        ICommand New { get; }
        ICommand Open { get; }
        ICommand CleanData { get; }
        ICommand CleanAnalysis { get; }
        ICommand Exit { get; }
        ICommand Run { get; }
        ICommand About { get; }
    }
}