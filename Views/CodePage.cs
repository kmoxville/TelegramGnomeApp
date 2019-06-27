using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdk;
using Gtk;

namespace TelegramApp.views
{
    class CodePage : Box
    {
        private Entry _codeEntry;
        private Button _button;
        private MainWindow _window;

        public CodePage(MainWindow window) : base(Orientation.Vertical, 0)
        {
            _window = window;
            var label = new Label();
            label.Text = "Enter code";
            label.SetFontSize(20);
            label.Halign = Align.Start;
            this.PackStart(label, false, false, 5);

            _codeEntry = new DigitsEntry();
            //_codeEntry.WidthRequest = 10;
            //_codeEntry.MaxLength = 4;
            //_codeEntry.MarginEnd = 5;
            //_codeEntry.WidthChars = 4;
            this.PackStart(_codeEntry, true, false, 5);

            _button = new Button();
            _button.Label = "Ok";
            _button.Clicked += _button_ClickedAsync;
            this.PackStart(_button, true, false, 5);

            this.Valign = Align.Center;
            this.Halign = Align.Center;
            this.Expand = false;

            this.ShowAll();
        }

        public string Number { get; set; }
        public string Hash { get; set; }

        private async void _button_ClickedAsync(object sender, EventArgs e)
        {
            if (_codeEntry.Text.Length == 0)
                return;

            _button.Sensitive = false;
            if (!await Program.Client.SendCodeRequestAsync(_codeEntry.Text))
            {
                _window.InfoBarMessage.Text = "Failed to authentificate";
                _window.InfoBar.ShowAll();              
            }
            _button.Sensitive = true;
        }
    }

}
