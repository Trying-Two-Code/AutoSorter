using System;
using System.Collections.Generic;
using System.Text;

namespace App.Ui.PromptUser
{
    public class InfoInterfaceItem
    {
        public InfoInterfaceItem(string _Name = "") { Name = _Name; }

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
        public EventHandler? ClickHandler { get; set; }
        public string? Description { get; set; }
    }
}
