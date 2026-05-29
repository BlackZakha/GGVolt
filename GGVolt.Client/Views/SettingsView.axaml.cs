using System;
using Avalonia.Controls;
namespace GGVolt.Client.Views;
public partial class SettingsView : UserControl { public SettingsView() => InitializeComponent();

    private void SpeedValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (SpeedUpDownSetting.Value == null)
            SpeedUpDownSetting.Value = 0;
        
        if (SpeedUpDownSetting.Value == 0)
            SpeedUpDownSetting.FormatString = "Без лимита";
        else
            SpeedUpDownSetting.FormatString = "";
    }
}