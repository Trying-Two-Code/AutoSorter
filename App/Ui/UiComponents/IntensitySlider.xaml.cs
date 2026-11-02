using App.Ui.UiScripts;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace App.Ui.UiComponents
{
    /// <summary>
    /// Interaction logic for IntensitySlider.xaml
    /// </summary>
    public partial class IntensitySlider : System.Windows.Controls.UserControl
    {
        public IntensitySlider()
        {
            UserSettings.GetSettings();
            int? oldValue = (int?)UserSettings.GetSettings("promptUserAmm");
            InitializeComponent();
            if(oldValue != null)
            { 
                IntensitySliderInst.Value = (double)oldValue;
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
            {
                oldSliderValue = _newValue;
                IntensityLabel.Content = _newValue;
                UserSettings.Set("promptUserAmm", _newValue);
            }
        }
    }
}
