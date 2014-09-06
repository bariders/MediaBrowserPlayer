/*
Smart Player for Media Browser
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

using SmartPlayer.Classes;
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

namespace SmartPlayer
{
    public sealed partial class SettingsFlyoutStreaming : SettingsFlyout
    {
        AppSettings settings = new AppSettings();

        public SettingsFlyoutStreaming()
        {
            this.InitializeComponent();

            // video bitrate
            List<ComboBoxData> videoBitrateItems = new List<ComboBoxData>();
            videoBitrateItems.Add(new ComboBoxData() { DataName = "100 M", DataValueInt = 100000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "50 M", DataValueInt = 50000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "20 M", DataValueInt = 20000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "15 M", DataValueInt = 15000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "10 M", DataValueInt = 10000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "7.5 M", DataValueInt = 7500000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "5.0 M", DataValueInt = 5000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "4.0 M", DataValueInt = 4000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "3.0 M", DataValueInt = 3000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "2.5 M", DataValueInt = 2500000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "2.0 M", DataValueInt = 2000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "1.5 M", DataValueInt = 1500000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "1.0 M", DataValueInt = 1000000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "750 K", DataValueInt = 750000 });
            videoBitrateItems.Add(new ComboBoxData() { DataName = "500 K", DataValueInt = 500000 });
            videoBitrate.ItemsSource = videoBitrateItems;

            int videoBitrateSetting = settings.GetAppSettingInt("video_bitrate");
            if (videoBitrateSetting == -1)
            {
                videoBitrateSetting = 10000000;
            }
            videoBitrate.SelectedItem = videoBitrateItems[GetSelectedIntItem(videoBitrateItems, videoBitrateSetting)];

            // max video width
            List<ComboBoxData> videoMaxWidthItems = new List<ComboBoxData>();
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1920", DataValueInt = 1920 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1600", DataValueInt = 1600 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1440", DataValueInt = 1440 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1280", DataValueInt = 1280 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "1024", DataValueInt = 1024 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "800", DataValueInt = 800 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "720", DataValueInt = 720 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "640", DataValueInt = 640 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "560", DataValueInt = 560 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "480", DataValueInt = 480 });
            videoMaxWidthItems.Add(new ComboBoxData() { DataName = "376", DataValueInt = 376 });
            videoMaxWidth.ItemsSource = videoMaxWidthItems;

            int videoMaxWidthSetting = settings.GetAppSettingInt("video_max_width");
            if (videoMaxWidthSetting == -1)
            {
                videoMaxWidthSetting = 1920;
            }
            videoMaxWidth.SelectedItem = videoMaxWidthItems[GetSelectedIntItem(videoMaxWidthItems, videoMaxWidthSetting)];

            // audio bitrate
            List<ComboBoxData> audioBitrateItems = new List<ComboBoxData>();
            audioBitrateItems.Add(new ComboBoxData() { DataName = "720 K", DataValueInt = 720000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "448 K", DataValueInt = 448000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "320 K", DataValueInt = 320000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "256 K", DataValueInt = 256000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "196 K", DataValueInt = 196000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "128 K", DataValueInt = 128000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "96 K", DataValueInt = 96000 });
            audioBitrateItems.Add(new ComboBoxData() { DataName = "64 K", DataValueInt = 64000 });
            audioBitrate.ItemsSource = audioBitrateItems;

            int audioBitrateSetting = settings.GetAppSettingInt("audio_bitrate");
            if (audioBitrateSetting == -1)
            {
                audioBitrateSetting = 128000;
            }
            audioBitrate.SelectedItem = audioBitrateItems[GetSelectedIntItem(audioBitrateItems, audioBitrateSetting)];

            // audio channels
            List<ComboBoxData> audioChannelList = new List<ComboBoxData>();
            audioChannelList.Add(new ComboBoxData() { DataName = "1", DataValueInt = 1 });
            audioChannelList.Add(new ComboBoxData() { DataName = "2", DataValueInt = 2 });
            audioChannelList.Add(new ComboBoxData() { DataName = "3", DataValueInt = 3 });
            audioChannelList.Add(new ComboBoxData() { DataName = "4", DataValueInt = 4 });
            audioChannelList.Add(new ComboBoxData() { DataName = "5", DataValueInt = 5 });
            audioChannelList.Add(new ComboBoxData() { DataName = "6", DataValueInt = 6 });
            audioChannels.ItemsSource = audioChannelList;
            int audioChannelsSetting = settings.GetAppSettingInt("audio_channels");
            if (audioChannelsSetting == -1)
            {
                audioChannelsSetting = 6;
            }
            audioChannels.SelectedItem = audioChannelList[GetSelectedIntItem(audioChannelList, audioChannelsSetting)];

            // audio codec
            List<ComboBoxData> audioCodecList = new List<ComboBoxData>();
            audioCodecList.Add(new ComboBoxData() { DataName = "AC3", DataValueString = "ac3" });
            audioCodecList.Add(new ComboBoxData() { DataName = "AAC", DataValueString = "aac" });
            audioCodecList.Add(new ComboBoxData() { DataName = "AAC, AC3", DataValueString = "aac,ac3" });
            audioCodecs.ItemsSource = audioCodecList;

            string selectedAudioCodec = settings.GetAppSettingString("audio_codec");
            if (string.IsNullOrWhiteSpace(selectedAudioCodec))
            {
                selectedAudioCodec = "aac,ac3";
            }
            audioCodecs.SelectedItem = audioCodecList[GetSelectedStringItem(audioCodecList, selectedAudioCodec)];

            // enable stream copy
            List<ComboBoxData> enableStreamCopyList = new List<ComboBoxData>();
            enableStreamCopyList.Add(new ComboBoxData() { DataName = "YES", DataValueString = "true" });
            enableStreamCopyList.Add(new ComboBoxData() { DataName = "NO", DataValueString = "false" });
            enableStreamCopy.ItemsSource = enableStreamCopyList;

            string selectedEnableStreamCopy = settings.GetAppSettingString("stream_copy");
            if (string.IsNullOrWhiteSpace(selectedEnableStreamCopy))
            {
                selectedEnableStreamCopy = "true";
            }
            enableStreamCopy.SelectedItem = enableStreamCopyList[GetSelectedStringItem(enableStreamCopyList, selectedEnableStreamCopy)];
        }

        public int GetSelectedIntItem(List<ComboBoxData> items, int data)
        {
            for(int x = 0; x < items.Count; x++)
            {
                if(items[x].DataValueInt == data)
                {
                    return x;
                }
            }
            return 0;
        }

        public int GetSelectedStringItem(List<ComboBoxData> items, string data)
        {
            for (int x = 0; x < items.Count; x++)
            {
                if (items[x].DataValueString == data)
                {
                    return x;
                }
            }
            return 0;
        }

        private void audioChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("audio_channels", selected.DataValueInt);
        }

        private void audioCodec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingString("audio_codec", selected.DataValueString);
        }

        private void enableStreamCopy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingString("stream_copy", selected.DataValueString);
        }

        private void videoBitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("video_bitrate", selected.DataValueInt);
        }

        private void videoMaxWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("video_max_width", selected.DataValueInt);
        }

        private void audioBitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxData selected = (sender as ComboBox).SelectedItem as ComboBoxData;
            settings.SaveAppSettingInt("audio_bitrate", selected.DataValueInt);
        }
    }
}
