using Gtk;
using System;
using System.Collections.Generic;
using System.Text;
using UI = Gtk.Builder.ObjectAttribute;

namespace TelegramApp
{
    class ChatPage : Box
    {
        [UI]
        private ListBox _listBox;

        [UI]
        private Box _leftPane;

        public ChatPage() : this(new Builder("TelegramApp.Views.ui.ChatPage.glade")) { }

        public ChatPage(Builder builder) : base(builder.GetObject("_rootBox").Handle)
        {
            builder.Autoconnect(this);
            ShowAll();
        }

        public int PaneWidth
        {
            get
            {
                return _listBox.WidthRequest;
            }
            set
            {
                _listBox.WidthRequest = value;
            }
        }
    }
}
