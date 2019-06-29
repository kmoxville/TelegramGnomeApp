using Gtk;
using System;
using System.Collections.Generic;
using System.Text;
using TelegramApp.Views;
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
            Program.Client.OnNewChat += OnNewChatHandler;
            ShowAll();
        }

        private void OnNewChatHandler(object sender, Client.NewChatEventArgs args)
        {
            Gtk.Application.Invoke((o, e) =>
            {
                    _listBox.Add(new ChatBoxEntry(args.Chat)
                    {
                        //LastMessage = chat.LastMessage.Content.ToString(),
                        //Icon = chat.Photo.Small.Local.Path,
                        //UnreadCount = chat.UnreadCount,
                        //Date = new DateTime(chat.LastMessage.Date)
                    });

            });
            
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
