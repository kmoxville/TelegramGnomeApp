using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TdLib;
using Td = TdLib;

namespace TelegramApp
{
    class Client
    {
        public enum AuthState
        {
            WaitPhoneNumber,
            WaitCode,
            Ready,
            LoggingOut,
            Closing,
            CriticalError
        }

        private string _api_hash;
        private int _api_id;

        public AuthState CurrentState { get; private set; }
        private Td.Client _client = null;
        private Td.Hub _hub = null;
        private Td.Dialer _dialer;
        private NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private Thread _thread;
        private ISet<OrderedChat> _chatList = new SortedSet<OrderedChat>();
        private ConcurrentDictionary<int, TdApi.User> _users = new ConcurrentDictionary<int, TdApi.User>();
        private ConcurrentDictionary<int, TdApi.BasicGroup> _basicGroups = new ConcurrentDictionary<int, TdApi.BasicGroup>();
        private ConcurrentDictionary<int, TdApi.Supergroup> _spGroups = new ConcurrentDictionary<int, TdApi.Supergroup>();
        private ConcurrentDictionary<int, TdApi.SecretChat> _secretChats = new ConcurrentDictionary<int, TdApi.SecretChat>();

        public class AuthEventArgs : EventArgs
        {
            public AuthEventArgs() : base() { }

            public AuthState State { get; set; }
        }

        public class NewChatEventArgs : EventArgs
        {
            public NewChatEventArgs() : base() { }

            public Chat Chat { get; set; }
        }

        public delegate void AuthStateChangedHandler(object sender, AuthEventArgs args);
        public delegate void NewChatHandler(object sender, NewChatEventArgs args);

        public event AuthStateChangedHandler AuthStateChanged;
        public event NewChatHandler OnNewChat;

        public Client(int api_id, string api_hash)
        {
            _api_id = api_id;
            _api_hash = api_hash;
        }

        public bool IsUserAuthorized { get; private set; } = false;
        public ConcurrentDictionary<long, Chat> Chats { get; private set; } = new ConcurrentDictionary<long, Chat>();

        public void Start()
        {
            _client = new Td.Client();
            _hub = new Hub(_client);
            _hub.Received += Hub_Received;
            _thread = new Thread(() =>
            {
                _hub.Start();
            });
            _thread.Start();

            _dialer = new Dialer(_client, _hub);
        }

        private async void Hub_Received(object sender, TdApi.Object data)
        {
            Logger.Warn("Hub_Received: {@data}", data);
            switch (data)
            {
                case TdApi.Error o:
                    Logger.Warn("TdApi.Object is TdApi.Error: {@o}", o);
                    break;
                case TdApi.Ok _:
                    break;
                case TdLib.TdApi.Update.UpdateAuthorizationState o:
                    await AuthorizationStateUpdateHandlerAsync(o);
                    break;
                case TdApi.Update.UpdateUser o:
                    UpdateUser(o);                  
                    break;
                case TdApi.Update.UpdateUserStatus o:
                    UpdateUserStatus(o);
                    break;
                case TdApi.Update.UpdateBasicGroup o:
                    UpdateBasicGroup(o);
                    break;
                case TdApi.Update.UpdateSupergroup o:
                    UpdateSuperGroup(o);
                    break;
                case TdApi.Update.UpdateSecretChat o:
                    UpdateSecretChat(o);
                    break;
                case TdApi.Update.UpdateNewChat o:
                    await UpdateNewChatAsync(o);
                    break;
                case TdApi.Update.UpdateChatTitle o:
                    UpdateChatTitle(o);
                    break;
                case TdApi.Update.UpdateChatPhoto o:
                    await UpdateChatPhotoAsync(o);
                    break;
                case TdApi.Update.UpdateChatLastMessage o:
                    UpdateChatLastMessage(o);
                    break;
                case TdApi.Update.UpdateChatOrder o:
                    UpdateChatOrder(o);
                    break;
                case TdApi.Update.UpdateChatIsPinned o:
                    UpdateChatIsPinned(o);
                    break;
                case TdApi.Update.UpdateChatReadInbox o:
                    UpdateChatReadInbox(o);
                    break;
                case TdApi.Update.UpdateChatReadOutbox o:
                    UpdateChatReadOutbox(o);
                    break;
                case TdApi.Update.UpdateChatUnreadMentionCount o:
                    UpdateChatUnreadMentionCount(o);
                    break;
                case TdApi.Update.UpdateMessageMentionRead o:
                    UpdateMessageMentionRead(o);
                    break;
                case TdApi.Update.UpdateChatReplyMarkup o:
                    UpdateChatReplyMarkup(o);
                    break;
                case TdApi.Update.UpdateChatDraftMessage o:
                    UpdateChatDraftMessage(o);
                    break;
                case TdApi.Update.UpdateChatNotificationSettings o:
                    UpdateChatNotificationSettings(o);
                    break;
                case TdApi.Update.UpdateChatDefaultDisableNotification o:
                    UpdateChatDefaultDisableNotification(o);
                    break;
                case TdApi.Update.UpdateChatIsMarkedAsUnread o:
                    UpdateChatIsMarkedAsUnread(o);
                    break;
                case TdApi.Update.UpdateChatIsSponsored o:
                    UpdateChatIsSponsored(o);
                    break;
                case TdApi.Update.UpdateUserFullInfo o:
                    UpdateUserFullInfo(o);
                    break;
                default:
                    Logger.Warn("Unhandled update: {@data}", data);
                    break;
            }
        }

        private void UpdateUserFullInfo(TdApi.Update.UpdateUserFullInfo o)
        {
            //FixMe
        }

        private void UpdateChatIsSponsored(TdApi.Update.UpdateChatIsSponsored o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.IsSponsored = o.IsSponsored;
                    //chat.Order = o.Order;
                    SetChatOrder(chat, o.Order);
                }
            }
            else
            {
                Logger.Warn("UpdateChatIsSponsored fail: {@chat}", chat);
            }
        }

        private void UpdateChatIsMarkedAsUnread(TdApi.Update.UpdateChatIsMarkedAsUnread o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.IsMarkedAsUnread = o.IsMarkedAsUnread;
                }
            }
            else
            {
                Logger.Warn("UpdateChatIsMarkedAsUnread fail: {@chat}", chat);
            }
        }

        private void SetChatOrder(TdApi.Chat chat, long order)
        {
            lock (_chatList)
            {
                if (chat.Order != 0)
                {
                    _chatList.Remove(new OrderedChat() { Id = chat.Id, Order = chat.Order});
                    Chats.TryRemove(chat.Id, out _);
                    chat.Order = order;
                    _chatList.Add(new OrderedChat() { Id = chat.Id, Order = chat.Order });
                    Chats.TryAdd(chat.Id, new Chat(chat));
                }
            }
        }

        private void UpdateChatDefaultDisableNotification(TdApi.Update.UpdateChatDefaultDisableNotification o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.DefaultDisableNotification = o.DefaultDisableNotification;
                }
            }
            else
            {
                Logger.Warn("DefaultDisableNotification fail: {@chat}", chat);
            }
        }

        private void UpdateChatNotificationSettings(TdApi.Update.UpdateChatNotificationSettings o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.NotificationSettings = o.NotificationSettings;
                }
            }
            else
            {
                Logger.Warn("UpdateChatNotificationSettings fail: {@chat}", chat);
            }
        }

        private void UpdateChatDraftMessage(TdApi.Update.UpdateChatDraftMessage o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.DraftMessage = o.DraftMessage;
                    //chat.Order = o.Order;
                    SetChatOrder(chat, o.Order);
                }
            }
            else
            {
                Logger.Warn("UpdateChatDraftMessage fail: {@chat}", chat);
            }
        }

        private void UpdateChatReplyMarkup(TdApi.Update.UpdateChatReplyMarkup o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.ReplyMarkupMessageId = o.ReplyMarkupMessageId;
                }
            }
            else
            {
                Logger.Warn("UpdateChatReadInbox fail: {@chat}", chat);
            }
        }

        private void UpdateMessageMentionRead(TdApi.Update.UpdateMessageMentionRead o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.UnreadMentionCount = o.UnreadMentionCount;
                }
            }
            else
            {
                Logger.Warn("UpdateMessageMentionRead fail: {@chat}", chat);
            }
        }

        private void UpdateChatUnreadMentionCount(TdApi.Update.UpdateChatUnreadMentionCount o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.UnreadMentionCount = o.UnreadMentionCount;
                }
            }
            else
            {
                Logger.Warn("UpdateChatUnreadMentionCount fail: {@chat}", chat);
            }
        }

        private void UpdateChatReadOutbox(TdApi.Update.UpdateChatReadOutbox o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.LastReadOutboxMessageId = o.LastReadOutboxMessageId;
                }
            }
            else
            {
                Logger.Warn("UpdateChatReadOutbox fail: {@chat}", chat);
            }
        }

        private void UpdateChatReadInbox(TdApi.Update.UpdateChatReadInbox o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.LastReadInboxMessageId = o.LastReadInboxMessageId;
                    chat.UnreadCount = o.UnreadCount;
                }
            }
            else
            {
                Logger.Warn("UpdateChatReadInbox fail: {@chat}", chat);
            }
        }

        private void UpdateChatIsPinned(TdApi.Update.UpdateChatIsPinned o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.IsPinned = o.IsPinned;
                    //chat.Order = o.Order;
                    SetChatOrder(chat, o.Order);
                }
            }
            else
            {
                Logger.Warn("UpdateChatIsPinned fail: {@chat}", chat);
            }
        }

        private void UpdateChatOrder(TdApi.Update.UpdateChatOrder o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.Order = o.Order;
                    SetChatOrder(chat, o.Order);
                }
            }
            else
            {
                Logger.Warn("UpdateChatOrder fail: {@chat}", chat);
            }
        }

        private void UpdateChatLastMessage(TdApi.Update.UpdateChatLastMessage o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.LastMessage = o.LastMessage;
                    SetChatOrder(chat, o.Order);
                }
            }
            else
            {
                Logger.Warn("UpdateChatLastMessage fail: {@chat}", chat);
            }
        }

        private async Task UpdateChatPhotoAsync(TdApi.Update.UpdateChatPhoto o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.Photo = o.Photo;             
                }
                var file = await _dialer.ExecuteAsync(new TdApi.DownloadFile() { FileId = o.Photo.Small.Id, Priority = 16 });
            }
            else
            {
                Logger.Warn("UpdateChatPhoto fail: {@chat}", chat);
            }
        }

        private void UpdateChatTitle(TdApi.Update.UpdateChatTitle o)
        {
            Chat chat;
            if (Chats.TryGetValue(o.ChatId, out chat))
            {
                lock (chat)
                {
                    chat.Title = o.Title;
                }
            }
            else
            {
                Logger.Warn("UpdateChatTitle fail: {@chat}", chat);
            }
        }

        private async Task UpdateNewChatAsync(TdApi.Update.UpdateNewChat o)
        {
            var newChat = new Chat(o.Chat);
            lock (newChat)
            {
                if (Chats.TryAdd(newChat.Id, newChat))
                {
                    var order = newChat.Order;
                    newChat.Order = 0;
                    SetChatOrder(newChat, order);                
                    OnNewChat?.Invoke(this, new NewChatEventArgs()
                    {
                        Chat = newChat
                    });
                }
                else
                {
                    Logger.Warn("UpdateBasicGroup fail: {@newChat}", newChat);
                }
            }
            if (o.Chat?.Photo?.Small != null)
            {
                var file = await _dialer.ExecuteAsync(new TdApi.DownloadFile() { FileId = o.Chat.Photo.Small.Id, Priority = 16 });
            }
        }

        private void UpdateSecretChat(TdApi.Update.UpdateSecretChat o)
        {
            var schat = o.SecretChat;
            if (!_secretChats.TryAdd(schat.Id, schat))
            {
                Logger.Warn("UpdateSecretChat fail: {@schat}", schat);
            }
        }

        private void UpdateSuperGroup(TdApi.Update.UpdateSupergroup o)
        {
            var group = o.Supergroup;
            if (!_spGroups.TryAdd(group.Id, group))
            {
                Logger.Warn("UpdateSuperGroup fail: {@group}", group);
            }
        }

        private void UpdateBasicGroup(TdApi.Update.UpdateBasicGroup o)
        {
            var group = o.BasicGroup;
            if (!_basicGroups.TryAdd(group.Id, group))
            {
                Logger.Warn("UpdateBasicGroup fail: {@group}", group);
            }
        }

        private void UpdateUserStatus(TdApi.Update.UpdateUserStatus o)
        {
            TdApi.User user;
            _users.TryGetValue(o.UserId, out user);
            lock (user)
            {
                user.Status = o.Status;
            }
        }

        private void UpdateUser(TdApi.Update.UpdateUser updateUser)
        {
            if (!_users.TryAdd(updateUser.User.Id, updateUser.User))
            {
                Logger.Warn("UpdateUser fail: {@updateUser.User}", updateUser.User);
            }
        }

        private async Task AuthorizationStateUpdateHandlerAsync(TdLib.TdApi.Update.UpdateAuthorizationState authUpdate)
        {
            switch (authUpdate.AuthorizationState)
            {
                case TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters _:
                    string path = DataStorage.UserDataFolder;
                    await _dialer.ExecuteAsync(new TdApi.SetTdlibParameters
                    {
                        Parameters = new TdApi.TdlibParameters
                        {
                            UseTestDc = false,
                            DatabaseDirectory = path, // directory here
                            FilesDirectory = path, // directory here
                            UseFileDatabase = true,
                            UseChatInfoDatabase = true,
                            UseMessageDatabase = true,
                            UseSecretChats = true,
                            ApiId = _api_id, // your API ID
                            ApiHash = _api_hash, // your API HASH
                            SystemLanguageCode = "en",
                            DeviceModel = "Desktop",
                            SystemVersion = "0.1",
                            ApplicationVersion = "0.1",
                            EnableStorageOptimizer = true,
                            IgnoreFileNames = false
                        }
                    });
                    break;

                case TdApi.AuthorizationState.AuthorizationStateWaitEncryptionKey _:
                    await _dialer.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());
                    break;

                case TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber _:
                    AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.WaitPhoneNumber });
                    break;

                case TdApi.AuthorizationState.AuthorizationStateWaitCode _:
                    AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.WaitCode });
                    break;

                case TdApi.AuthorizationState.AuthorizationStateWaitPassword _:
                    await _dialer.ExecuteAsync(new TdApi.CheckAuthenticationPassword
                    {
                        Password = "P@$$w0rd" // your password
                    });
                    break;

                case TdApi.AuthorizationState.AuthorizationStateReady _:
                    AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.Ready });
                    IsUserAuthorized = true;
                    await GetChats(512);
                    break;
                case TdApi.AuthorizationState.AuthorizationStateLoggingOut _:
                    AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.LoggingOut });
                    break;
                case TdApi.AuthorizationState.AuthorizationStateClosing _:
                    AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.Closing });
                    break;
            }
        }

        public void Dispose()
        {
            Logger.Info("Disposing client...");
            _client.Dispose();
        }

        public async Task<bool> SendCodeRequestAsync(string code)
        {
            Logger.Info("Send code {code}", code);

            try
            {
                await _dialer.ExecuteAsync(new TdApi.CheckAuthenticationCode
                {
                    Code = code // your auth code
                });
                return true;
            }
            catch (TdLib.ErrorException ex)
            {
                Logger.Warn(ex, "Tdlib exception: send code");
            }

            return false;
        }

        public async Task GetChats(int limit)
        {
            long offsetOrder = Int64.MaxValue;
            long offsetChatId = 0;
            if (_chatList.Count > 0)
            {
                OrderedChat last = _chatList.Last();
                offsetOrder = last.Order;
                offsetChatId = last.Id;
            }
            var chats = await _dialer.ExecuteAsync(new TdApi.GetChats()
            {
                OffsetOrder = offsetOrder,
                OffsetChatId = offsetChatId,
                Limit = limit - _chatList.Count
            });
            /*foreach (var id in chats.ChatIds)
            {
                lock (_chatList)
                {
                    _chatList.Add(new OrderedChat() { Id = id, });
                }
                Chats.TryAdd(id, new Chat() { Id = id });
            }*/
            //OnNewChat?.Invoke(this, new NewChatEventArgs());
            
        }

        public async Task<bool> MakeAuthAsync(string number)
        {
            Logger.Info("Send phone number ----");

            try
            {
                await _dialer.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
                {
                    PhoneNumber = number
                });
                return true;
            }
            catch (TdLib.ErrorException ex)
            {
                Logger.Warn(ex, "Tdlib exception: phone number");
            }

            return false;
        }

    }
}
