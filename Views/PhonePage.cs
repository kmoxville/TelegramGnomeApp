using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace TelegramApp.views
{
    class PhonePage : Box
    {
        [UI]
        private Box _hbox;

        [UI]
        private Button _okButton;

        [UI]
        private Entry _codeEntry;

        [UI]
        private Entry _phoneEntry;

        [UI]
        private ComboBox _comboBox;

        private MainWindow _window;

        public PhonePage(MainWindow window) : this(new Builder("TelegramApp.Views.ui.PhonePage.glade"))
        {
            _window = window;
        }

        public PhonePage(Builder builder) : base(builder.GetObject("_rootPhonePageBox").Handle)
        {
            builder.Autoconnect(this);

            var listStore = new ListStore(typeof(string), typeof(string));
            foreach (var country in Country.Countries)
            {
                listStore.AppendValues(country.Name, "+" + country.PhoneCode);
            }
            _comboBox.Model = listStore;
            _comboBox.Changed += CountryComboBoxChanged;

            var countryNameRenderer = new CellRendererText();
            _comboBox.PackStart(countryNameRenderer, true);
            _comboBox.AddAttribute(countryNameRenderer, "text", 0);

            var countryCodeRenderer = new CellRendererText();
            _comboBox.PackStart(countryCodeRenderer, false);
            _comboBox.AddAttribute(countryCodeRenderer, "text", 1);

            _okButton.Clicked += OkButtonClickedAsync;

            _codeEntry = new DigitsEntry();
            _codeEntry.WidthRequest = 10;
            _codeEntry.MaxLength = 4;
            _codeEntry.MarginEnd = 5;
            _codeEntry.WidthChars = 4;
            _codeEntry.Changed += CodeEntryChanged;
            _hbox.PackStart(_codeEntry, false, false, 0);

            _phoneEntry = new DigitsEntry();
            _phoneEntry.PlaceholderText = "--- --- -- --";
            _phoneEntry.MaxLength = 10;
            _phoneEntry.MaxWidthChars = 10;
            _hbox.PackStart(_phoneEntry, true, true, 0);
           
            ShowAll();
        }

        private void CodeEntryChanged(object sender, EventArgs e)
        {
            if (_codeEntry.Text.Length == 0)
                return;

            var model = _comboBox.Model;
            TreeIter iter;
            model.GetIterFirst(out iter);
            List<TreeIter> iterList = new List<TreeIter>();
            do
            {
                GLib.Value thisRow = new GLib.Value();
                model.GetValue(iter, 1, ref thisRow);
                if ((thisRow.Val as string).Equals("+" + _codeEntry.Text))
                {
                    iterList.Add(iter);
                }
            } while (model.IterNext(ref iter));
            if (iterList.Count > 0)
                _comboBox.SetActiveIter(iterList.Last());
        }

        private async void OkButtonClickedAsync(object sender, EventArgs e)
        {
            if (_phoneEntry.Text.Length == 0)
                return;

            _okButton.Sensitive = false;

            string number = _codeEntry.Text + _phoneEntry.Text;
            if (!await Program.Client.MakeAuthAsync(number))
            {
                _window.InfoBarMessage.Text = "Failed to connect to server, try proxy";
                _window.InfoBar.ShowAll();

            }
            _okButton.Sensitive = true;
        }

        private void CountryComboBoxChanged(object sender, EventArgs e)
        {
            TreeIter tree;
            _comboBox.GetActiveIter(out tree);
            string code = (String)_comboBox.Model.GetValue(tree, 1);
            _codeEntry.Text = code.Substring(1);
        }
    }
}
