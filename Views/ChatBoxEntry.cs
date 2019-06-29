using System;
using System.Collections.Generic;
using System.Text;
using Gdk;
using Gtk;
using NLog;
using UI = Gtk.Builder.ObjectAttribute;

namespace TelegramApp.Views
{
    class ChatBoxEntry : ListBoxRow
    {
        [UI]
        Label _title = new Label();

        [UI]
        Label _lastMessage = new Label();

        [UI]
        Image _icon = new Image();

        [UI]
        Label _unreadCount = new Label();

        [UI]
        Label _date = new Label();
        Chat _chat;

        private Logger Logger = LogManager.GetCurrentClassLogger();

        public ChatBoxEntry(Chat chat) : this(chat, new Builder("TelegramApp.Views.ui.ChatBoxEntry.glade"))
        {

        }

        public ChatBoxEntry(Chat chat, Builder builder) : base(builder.GetObject("_chatBoxEntryRoot").Handle)
        {
            builder.Autoconnect(this);
            _chat = chat;
            Chat_PropertyChanged(_chat, new System.ComponentModel.PropertyChangedEventArgs("kek"));
            ShowAll();
            _chat.PropertyChanged += Chat_PropertyChanged;            
        }

        private void Chat_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Gtk.Application.Invoke((oo, ee) =>
            {
                var chat = sender as Chat;
                Title = chat.Title;
                var lastMessage = chat.LastMessage?.Content;
                switch (lastMessage)
                {
                    case TdLib.TdApi.MessageContent.MessageText o:
                        LastMessage = o.Text.Text;
                        break;
                    case TdLib.TdApi.MessageContent.MessageAnimation _:
                        LastMessage = "gif";
                        break;
                    default:
                        Logger.Warn("Unhandled message content");
                        break;
                }

                if (chat.Photo == null)
                {
                    Icon = new Pixbuf(null, "TelegramApp.Data.DeletedChat.png");
                }
                else
                {
                    Icon = new Pixbuf(chat.Photo.Small.Local.Path);
                }
                
                UnreadCount = chat.UnreadCount;
                if (chat.LastMessage != null)
                    Date = UnixTimeStampToDateTime(chat.LastMessage.Date);
            });    
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /*private void Chat_OnChanged(Chat chat, ChangedEventArgs args)
        {
            Channel = chat.Title;
            LastMessage = chat.LastMessage.Content.ToString();
            Icon = chat.Photo.Small.Local.Path;
            UnreadCount = chat.UnreadCount;
            Date = new DateTime(chat.LastMessage.Date);
        }*/

        public string Title
        {
            get
            {
                return _title.Text;
            }
            set
            {
                _title.Text = value;
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

        public Pixbuf Icon
        {
            get
            {
                return null;
            }
            set
            {
                //if (string.IsNullOrEmpty(value))
                //    return;
                //var pixbuf = new Pixbuf(value);
                _icon.Pixbuf = value.ScaleSimple(64, 64, InterpType.Bilinear);
            }
        }

        public int UnreadCount
        {
            get
            {
                return int.Parse(_unreadCount.Text);
            }
            set
            {
                if (value > 0)
                    _unreadCount.Text = value.ToString();
                else
                    _unreadCount.Text = "";
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
                _date.Text = value.ToString("dd/MM/yyyy");
            }
        }
    }
}
