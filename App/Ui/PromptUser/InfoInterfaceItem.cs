
using System.Windows;
using System.Windows.Controls;

namespace App.Ui.PromptUser
{
    public class InfoInterfaceItem
    {
        public InfoInterfaceItem(string _Name = "") 
        {
            Name = _Name; 
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = "InfoItem_" + value;
                ID = value;
            }
        }
        private string _name { get; set; }
        public string ID
        {
            get => ID;
            private set;
        }

        public string? LabelText { get; set; }
        public EventHandler<InterfaceItemClickArgs>? ClickHandler { get; set; }
        public string? Description { get; set; }
        public UIElement? ChildContent { get; set; }
    }

    public class InterfaceItemClickArgs : EventArgs
    {
        public UIElement? DataElement { get; set; }
        public DateTime? MuteUntil { get; set; } = default(DateTime?);
    }

    public class InfoInterfaceUserControls 
    { 
        public static System.Windows.Controls.ComboBox MuteComboBox 
        { 
            get => new System.Windows.Controls.ComboBox()
            {
                Name = "MuteComboBox",
                Background = System.Windows.Media.Brushes.Transparent,
                ItemsSource = new List<ComboBoxItem>()
                    {
                        new ComboBoxItem(){Content="30 minutes",Tag=(TimeSpan.FromMinutes(30)), },
                        new ComboBoxItem(){Content="1 hour",    Tag=(TimeSpan.FromMinutes(60))},
                        new ComboBoxItem(){Content="2 hours",   Tag=(TimeSpan.FromMinutes(120))},
                        new ComboBoxItem(){Content="8 hours",   Tag=(TimeSpan.FromHours(8))},
                        new ComboBoxItem(){Content="3 days",    Tag=(TimeSpan.FromDays(3))},
                        new ComboBoxItem(){Content="1 week",    Tag=(TimeSpan.FromDays(7))},
                        new ComboBoxItem(){Content="1 month",   Tag=(TimeSpan.FromDays(30))},
                        new ComboBoxItem(){Content="6 months",  Tag=(TimeSpan.FromDays(182))},
                    }
            };
        }

    }
}
