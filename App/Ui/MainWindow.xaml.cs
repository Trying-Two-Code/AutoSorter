using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes; 

namespace AutoSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            createSound(@"Ui\Assets\Audio\SFX-Success1.mp3");
        }

        private void createSound(string soundFilePath)
        {
            //Debug::: if not making sound :::
            //1. right click audio file in solution explorer;
            //2. properties > Build action: Content;
            //3. properties > Copy to output directory: always
            Uri uri = new Uri(soundFilePath, UriKind.RelativeOrAbsolute);
            MediaPlayer player = new MediaPlayer();

            player.Open(uri);
            player.Play();
        }
    }
}