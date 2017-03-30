// Copyright (c) 2015-2017 Bartosz Rachwal.

using SpectrumAnalysis.Module.ViewModel.Menu;

namespace SpectrumAnalysis.Module.View.Menu
{
    public partial class MenuView
    {
        public MenuView(IMenuViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}