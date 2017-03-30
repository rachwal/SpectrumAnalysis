// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Globalization;
using System.Threading;
using System.Windows;

namespace SpectrumAnalysis.Shell
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var bootstrapper = new ApplicationBootstrapper();
            bootstrapper.Run();
        }
    }
}