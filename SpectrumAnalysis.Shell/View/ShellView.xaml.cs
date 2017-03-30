// Copyright (c) 2015-2017 Bartosz Rachwal.

using SpectrumAnalysis.Shell.ViewModel;

namespace SpectrumAnalysis.Shell.View
{
    public partial class ShellView
    {
        public ShellView(IShellViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}