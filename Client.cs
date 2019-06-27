using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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

        public class AuthEventArgs : EventArgs
        {
            public AuthEventArgs() : base() { }

            public AuthState State { get; set; }
        }

        public delegate void AuthStateChangedHandler(object sender, AuthEventArgs args);
        public event AuthStateChangedHandler AuthStateChanged;

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
            if (data is TdApi.Error)
                Logger.Warn("TdApi.Object is TdApi.Error: {@data}", data);
            else
                Logger.Info("TdApi.Object data: {@data}", data);

            if (data is TdApi.Ok)
            {

            }
            else if (data is TdLib.TdApi.Update.UpdateAuthorizationState)
            {
                var authUpdate = data as TdLib.TdApi.Update.UpdateAuthorizationState;
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
                        break;
                    case TdApi.AuthorizationState.AuthorizationStateLoggingOut _:
                        AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.LoggingOut });
                        break;
                    case TdApi.AuthorizationState.AuthorizationStateClosing _:
                        AuthStateChanged?.Invoke(this, new AuthEventArgs() { State = AuthState.Closing });
                        break;
                }
            }
        }

        public void Dispose()
        {
            Logger.Info("Disposing client...");
            _client.Dispose();
        }

        public Client(int api_id, string api_hash)
        {
            _api_id = api_id;
            _api_hash = api_hash;
        }

        public bool IsUserAuthorized { get; private set; }

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
