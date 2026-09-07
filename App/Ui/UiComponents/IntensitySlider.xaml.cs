using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using App.Ui.UiScripts;

namespace App.Ui.UiComponents
{
    /// <summary>
    /// Interaction logic for IntensitySlider.xaml
    /// </summary>
    public partial class IntensitySlider : System.Windows.Controls.UserControl
    {
        public IntensitySlider()
        {
            InitializeComponent();
            UserSettings.GetSettings();
            int oldValue = UserSettings.promptUserAmm;
            if(oldValue != null)
            {
                IntensitySliderInst.Value = oldValue;
            }
            else
            {
                IntensitySliderInst.Value = 50;
            }
        }

        static int oldSliderValue = 0;
        private void IntensitySliderInst_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider _sender = (Slider)sender;
            int _newValue = (int)Convert.ToInt32(e.NewValue);

            if (oldSliderValue != _newValue)  
                oldSliderValue = _newValue;
                IntensityLabel.Content = _newValue;
                UserSettings.JsonFileWriter.Write<int>("Ui/UserSettings/userSettings.json", "promptUserAmm", _newValue);
        }
    }
}
