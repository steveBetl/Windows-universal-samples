﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Devices;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace CameraManualControls
{
    public sealed partial class MainPage : Page
    {

        private void UpdateWbControlCapabilities()
        {
            var whiteBalanceControl = _mediaCapture.VideoDeviceController.WhiteBalance;

            if (whiteBalanceControl.Capabilities.Supported)
            {
                WbButton.Tag = Visibility.Visible;

                if (WbComboBox.ItemsSource == null)
                {
                    WbComboBox.ItemsSource = Enum.GetValues(typeof(ColorTemperaturePreset)).Cast<ColorTemperaturePreset>();
                }

                
                if (whiteBalanceControl.Capabilities.Max - whiteBalanceControl.Capabilities.Min > whiteBalanceControl.Capabilities.Step)
                {
                    // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                    WbSlider.ValueChanged -= WbSlider_ValueChanged;

                    whiteBalanceControl.TrySetAuto(false);
                    if (whiteBalanceControl.TryGetValue(out double value))
                    {
                        WbSlider.Minimum = whiteBalanceControl.Capabilities.Min;
                        WbSlider.Maximum = whiteBalanceControl.Capabilities.Max;
                        WbSlider.StepFrequency = whiteBalanceControl.Capabilities.Step;
                        WbSlider.Value = value;
                    }
                    WbSlider.ValueChanged += WbSlider_ValueChanged;
                }
                else
                {
                    WbSlider.Visibility = Visibility.Collapsed;
                    WbTextBox.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                WbButton.Visibility = Visibility.Collapsed;
                WbButton.Tag = Visibility.Collapsed;
            }
        }

        private async void WbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ColorTemperaturePreset)WbComboBox.SelectedItem;

            WbSlider.IsEnabled = (selected == ColorTemperaturePreset.Manual);

            await _mediaCapture.VideoDeviceController.WhiteBalanceControl.SetPresetAsync(selected);

            // Note: Automatic white balance can be locked to the current value by setting the preset to manual and not passing in a value to the control
        }

        private async void WbSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_settingUpUi) return;

            var value = (sender as Slider).Value;

            _mediaCapture.VideoDeviceController.WhiteBalance.TrySetValue(value);
        }

    }
}
