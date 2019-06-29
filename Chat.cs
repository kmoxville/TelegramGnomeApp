using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using TdLib;
using Td = TdLib;

namespace TelegramApp
{
    class Chat : TdApi.Chat, INotifyPropertyChanged
    {     
        public Chat() : base()
        {

        }

        public Chat(TdApi.Chat chat) : base()
        {
            base.Id = chat.Id;
            base.CanBeReported = chat.CanBeReported;
            base.ClientData = chat.ClientData;
            base.DataType = chat.DataType;
            base.DefaultDisableNotification = chat.DefaultDisableNotification;
            base.DraftMessage = chat.DraftMessage;
            base.Extra = chat.Extra;
            base.IsMarkedAsUnread = chat.IsMarkedAsUnread;
            base.IsPinned = chat.IsPinned;
            base.IsSponsored = chat.IsSponsored;
            base.LastMessage = chat.LastMessage;
            base.LastReadInboxMessageId = chat.LastReadInboxMessageId;
            base.LastReadOutboxMessageId = chat.LastReadOutboxMessageId;
            base.NotificationSettings = chat.NotificationSettings;
            base.Order = chat.Order;
            base.Photo = chat.Photo;
            base.ReplyMarkupMessageId = chat.ReplyMarkupMessageId;
            base.Title = chat.Title;
            base.Type = chat.Type;
            base.UnreadCount = chat.UnreadCount;
            base.UnreadMentionCount = chat.UnreadMentionCount;
        }

        public new int UnreadMentionCount
        {
            get { return base.UnreadMentionCount; }
            set
            {
                base.UnreadMentionCount = value;
                OnPropertyChanged();
            }
        }

        public new int UnreadCount
        {
            get { return base.UnreadCount; }
            set
            {
                base.UnreadCount = value;
                OnPropertyChanged();
            }
        }

        public new TdApi.ChatType Type
        {
            get { return base.Type; }
            set
            {
                base.Type = value;
                OnPropertyChanged();
            }
        }

        public new string Title
        {
            get { return base.Title; }
            set
            {
                base.Title = value;
                OnPropertyChanged();
            }
        }

        public new long ReplyMarkupMessageId
        {
            get { return base.ReplyMarkupMessageId; }
            set
            {
                base.ReplyMarkupMessageId = value;
                OnPropertyChanged();
            }
        }

        public new TdApi.ChatPhoto Photo
        {
            get { return base.Photo; }
            set
            {
                base.Photo = value;
                OnPropertyChanged();
            }
        }

        public new long Order
        {
            get { return base.Order; }
            set
            {
                base.Order = value;
                OnPropertyChanged();
            }
        }

        public new TdApi.ChatNotificationSettings NotificationSettings
        {
            get { return base.NotificationSettings; }
            set
            {
                base.NotificationSettings = value;
                OnPropertyChanged();
            }
        }

        public new long LastReadOutboxMessageId
        {
            get { return base.LastReadOutboxMessageId; }
            set
            {
                base.LastReadOutboxMessageId = value;
                OnPropertyChanged();
            }
        }

        public new long LastReadInboxMessageId
        {
            get { return base.LastReadInboxMessageId; }
            set
            {
                base.LastReadInboxMessageId = value;
                OnPropertyChanged();
            }
        }

        public new TdApi.Message LastMessage
        {
            get { return base.LastMessage; }
            set
            {
                base.LastMessage = value;
                OnPropertyChanged();
            }
        }

        public new bool IsSponsored
        {
            get { return base.IsSponsored; }
            set
            {
                base.IsSponsored = value;
                OnPropertyChanged();
            }
        }

        public new bool IsPinned
        {
            get { return base.IsPinned; }
            set
            {
                base.IsPinned = value;
                OnPropertyChanged();
            }
        }

        public new bool IsMarkedAsUnread
        {
            get { return base.IsMarkedAsUnread; }
            set
            {
                base.IsMarkedAsUnread = value;
                OnPropertyChanged();
            }
        }

        public new string Extra
        {
            get { return base.Extra; }
            set
            {
                base.Extra = value;
                OnPropertyChanged();
            }
        }

        public new TdApi.DraftMessage DraftMessage
        {
            get { return base.DraftMessage; }
            set
            {
                base.DraftMessage = value;
                OnPropertyChanged();
            }
        }

        public new bool DefaultDisableNotification
        {
            get { return base.DefaultDisableNotification; }
            set
            {
                base.DefaultDisableNotification = value;
                OnPropertyChanged();
            }
        }

        public new string DataType
        {
            get { return base.DataType; }
            set
            {
                base.DataType = value;
                OnPropertyChanged();
            }
        }

        public new long Id
        {
            get { return base.Id; }
            set
            {
                base.Id = value;
                OnPropertyChanged();
            }
        }

        public new bool CanBeReported
        {
            get { return base.CanBeReported; }
            set
            {
                base.CanBeReported = value;
                OnPropertyChanged();
            }
        }

        public new string ClientData
        {
            get { return base.ClientData; }
            set
            {
                base.ClientData = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void OnChangedHandler(Chat chat, ChangedEventArgs args);
        public event OnChangedHandler OnChanged;
    }

    public class ChangedEventArgs : EventArgs
    {

    }
}
