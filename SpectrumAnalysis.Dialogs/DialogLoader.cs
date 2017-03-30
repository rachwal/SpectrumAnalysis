// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Windows;
using Microsoft.Win32;
using Prism.Events;
using SpectrumAnalysis.Data.Cursor;
using SpectrumAnalysis.Data.Raw;
using SpectrumAnalysis.Module.Events;

namespace SpectrumAnalysis.Dialogs
{
    public class DialogLoader : IDialogLoader
    {
        private readonly IDataCursor dataCursor;
        private readonly IRawDataReader dataReader;

        public DialogLoader(IRawDataReader reader, IDataCursor cursor, IEventAggregator eventAggregator)
        {
            dataReader = reader;
            dataCursor = cursor;

            eventAggregator.GetEvent<OpenRawDataFileEvent>().Subscribe(OpenRawData);
            eventAggregator.GetEvent<ShowAboutBoxEvent>().Subscribe(ShowAboutBox);
        }

        private void ShowAboutBox(string obj)
        {
            MessageBox.Show(
                "Created by Bartosz Rachwal.\nCopyright (c) 2015 Bartosz Rachwal.\nbartosz.rachwal@gmail.com",
                "About Spectrum Analysis");
        }

        private void OpenRawData(string info)
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            dataReader.Read(dialog.FileName);
            dataCursor.Reset();
        }
    }
}