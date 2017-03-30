// Copyright (c) 2015-2017 Bartosz Rachwal.

using SpectrumAnalysis.Module.ViewModel.StatusBar;

namespace SpectrumAnalysis.Module.View.StatusBar
{
    public partial class StatusBarView
    {
        public StatusBarView(IStatusBarViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}