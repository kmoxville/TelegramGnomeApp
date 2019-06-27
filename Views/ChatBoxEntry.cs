using System;
using System.Collections.Generic;
using System.Text;
using Gdk;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace TelegramApp.views
{
    class ChatBoxEntry : ListBoxRow
    {
        Label _channel = new Label();
        Label _lastMessage = new Label();
        Image _icon = new Image();
        Label _unread = new Label();
        Label _date = new Label();
        Label _type = new Label();

        public ChatBoxEntry()
        {
            var grid = new Grid();


            var hbox = new HBox(false, 10);
            //var pixbuf = new Pixbuf(null, "TelegramApp.Data.TelegramIcon.png");
            //Icon = new Image(pixbuf.ScaleSimple(64, 64, InterpType.Bilinear));


            hbox.PackStart(_icon, false, false, 0);
            var vbox = new VBox(false, 0);
            vbox.PackStart(_channel, false, false, 5);
            vbox.PackStart(_lastMessage, false, false, 5);
            vbox.Halign = Align.Start;
            vbox.Valign = Align.Center;
            hbox.PackStart(vbox, true, true, 0);

            vbox = new VBox(false, 0);
            vbox.PackStart(_date, false, false, 5);
            vbox.PackStart(_unread, false, false, 5);
            vbox.Halign = Align.Center;
            vbox.Valign = Align.Center;
            hbox.PackStart(vbox, false, false, 0);

            this.Add(hbox);
        }

        public string Channel
        {
            get
            {
                return _channel.Text;
            }
            set
            {
                _channel.Text = value;
            }
        }

        public string LastMessage
        {
            get
            {
                return _lastMessage.Text;
            }
            set
            {
                _lastMessage.Text = value;
            }
        }

        public string Icon
        {
            get
            {
                return "";
            }
            set
            {
                var pixbuf = new Pixbuf(null, value);
                _icon.Pixbuf = pixbuf.ScaleSimple(64, 64, InterpType.Bilinear);
            }
        }

        public int UnreadCount
        {
            get
            {
                return int.Parse(_unread.Text);
            }
            set
            {
                if (value > 0)
                    _unread.Text = value.ToString();
                else
                    _unread.Text = "";
            }
        }

        public DateTime Date
        {
            get
            {
                return DateTime.Parse(_date.Text);
            }
            set
            {
                _date.Text = value.ToShortDateString();
            }
        }
    }
}
