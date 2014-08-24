/*
Media Browser Player
Copyright (C) 2014  Blue Bit Solutions

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using MediaBrowserPlayer.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace MediaBrowserPlayer
{
    public sealed partial class SettingsFlyoutStreaming : SettingsFlyout
    {
        AppSettings settings = new AppSettings();

        public SettingsFlyoutStreaming()
        {
            this.InitializeComponent();

            List<ComboBoxData> videoBitrateItems = new List<ComboBoxData>();
            videoBitrateItems.Add(new ComboBoxData() { DataName = "20 M", DataValue = 20000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "15 M", DataValue = 15000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "10 M", DataValue = 10000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "5.0 M", DataValue = 5000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "3.0 M", DataValue = 3000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "2.5 M", DataValue = 2500000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "2.0 M", DataValue = 2000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "1.5 M", DataValue = 1500000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "1.0 M", DataValue = 1000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "0.5 M", DataValue = 1000000 });
            videoBitrate.ItemsSource = videoBitrateItems;

            int videoBitrateSetting = settings.GetAppSettingInt("video_bitrate");
            if (videoBitrateSetting == -1)
            {
                videoBitrateSetting = 10000000;
            }
            videoBitrate.SelectedItem = videoBitrateItems[GetSelectedItem(videoBitrateItems, videoBitrateSetting)];

            List<ComboBoxData> videoMaxWidthItems = new List<ComboBoxData>();
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1920", DataValue = 1920 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1280", DataValue = 1280 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "720", DataValue = 720 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "576", DataValue = 576 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "480", DataValue = 480 });
            videoMaxWidth.ItemsSource = videoMaxWidthItems;

            int videoMaxWidthSetting = settings.GetAppSettingInt("video_max_width");
            if (videoMaxWidthSetting == -1)
            {
                videoMaxWidthSetting = 1920;
            }
            videoMaxWidth.SelectedItem = videoMaxWidthItems[GetSelectedItem(videoMaxWidthItems, videoMaxWidthSetting)];

            List<ComboBoxData> audioBitrateItems = new List<ComboBoxData>();
            audioBitrateItems.Add(new ComboBoxData() { DataName = "720 K", DataValue = 720000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "448 K", DataValue = 448000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "320 K", DataValue = 320000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "256 K", DataValue = 256000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "196 K", DataValue = 196000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "128 K", DataValue = 128000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "96 K", DataValue = 96000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "64 K", DataValue = 64000 });
            audioBitrate.ItemsSource = audioBitrateItems;

            int audioBitrateSetting = settings.GetAppSettingInt("audio_bitrate");
            if (audioBitrateSetting == -1)
            {
                audioBitrateSetting = 128000;
            }
            audioBitrate.SelectedItem = audioBitrateItems[GetSelectedItem(audioBitrateItems, audioBitrateSetting)];

        }

        public int GetSelectedItem(List<ComboBoxData> items, int data)
        {
            for(int x = 0; x < items.Count; x++)
            {
                if(items[x].DataValue == data)
                {
                    return x;
                }
            }
            return 0;
        }

        private void videoBitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("video_bitrate", selected.DataValue);
        }

        private void videoMaxWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("video_max_width", selected.DataValue);
        }

        private void audioBitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("audio_bitrate", selected.DataValue);
        }
    }
}
