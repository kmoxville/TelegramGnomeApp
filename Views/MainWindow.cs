using System;
using System.Reflection;
using System.Resources;
using Gtk;
using NLog;
using TelegramApp.Views;
using UI = Gtk.Builder.ObjectAttribute;

namespace TelegramApp
{
    class MainWindow : Window
    {
        [UI]
        private HeaderBar _titlebar = null;

        [UI]
        private Stack _stackWidget = null;

        [UI]
        private HeaderBar _chatsBar = null;

        [UI]
        private Box _chatBox= null;

        [UI]
        private Paned _titlePane = null;

        [UI]
        public InfoBar InfoBar = new InfoBar();

        public Label InfoBarMessage;
        private ChatPage _chatPage;
        private CodePage _codePage;
        private PhonePage _phonePage;
        private Logger Logger = LogManager.GetCurrentClassLogger();

        public MainWindow() : this(new Builder("TelegramApp.Views.ui.MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetObject("_mainWindow").Handle)
        {
            builder.Autoconnect(this);
            _titlebar.Destroyed += _titleBar_Destroyed;

            Program.Client.AuthStateChanged += Client_StateChanged1;
            
            InfoBarMessage = new Label("");
            InfoBarMessage.Halign = Align.Center;
            InfoBar.ContentArea.Expand = false;
            InfoBar.PackStart(InfoBarMessage, true, true, 0);
            var button = new Button(Gtk.Stock.Close);
            button.Clicked += InfoBarCloseButtonClicked;
            InfoBar.PackStart(button, false, false, 0);
            
            this.InfoBar.Hide();
            this.InfoBar.Respond += (o, args) =>
            {
                InfoBar.Hide();
            };

            _codePage = new CodePage(this);
            _stackWidget.AddNamed(_codePage, "CodePage");
            _chatPage = new ChatPage();
            _stackWidget.AddNamed(_chatPage, "ChatPage");
            _chatsBar.SizeAllocated += _titlebar_SizeAllocated;
            _phonePage = new PhonePage(this);
            _stackWidget.AddNamed(_phonePage, "PhonePage");
            _chatsBar.SizeAllocated += _titlebar_SizeAllocated;

            /*for (int i = 0; i < 5; i++)
            {           
                _listBox.Add(new ChatBoxEntry() {
                    Channel = "Channel name",
                    LastMessage = "Last message",
                    UnreadCount = i,
                    Date = DateTime.Now,
                    Icon = "TelegramApp.Data.TelegramIcon.png"
                });
            }
            _listBox.ShowAll();*/

            NavigateToAsync(PageType.Chat);
            Program.Client.Start();
        }

        private void Client_StateChanged1(object sender, Client.AuthEventArgs args)
        {
            if (args.State == Client.AuthState.WaitPhoneNumber)
            {
                NavigateToAsync(PageType.Phone);
            }
            if (args.State == Client.AuthState.WaitCode)
            {
                NavigateToAsync(PageType.Code);
            }
            if (args.State == Client.AuthState.Ready)
            {
                NavigateToAsync(PageType.Chat);
            }
        }

        public enum PageType //Fixme
        {
            Phone,
            Code,
            Chat
        }

        public void NavigateToAsync(PageType type) //Fixme
        {
            Logger.Info("Navigated to {@type}", type);

            _chatsBar.Hide();

            if (type == PageType.Code)
            {
                _stackWidget.VisibleChild = _codePage;
            }
            if (type == PageType.Phone)
                _stackWidget.VisibleChild = _phonePage;
            if (type == PageType.Chat)
            {
                _stackWidget.VisibleChild = _chatPage;
                _chatsBar.Show();
            }

        }

        private void InfoBarCloseButtonClicked(object sender, EventArgs e)
        {
            InfoBar.Hide();
        }

        private void _titlebar_SizeAllocated(object o, SizeAllocatedArgs args)
        {
            _chatPage.PaneWidth = args.Allocation.Width;
        }

        private void _titleBar_Destroyed(object sender, EventArgs e)
        {
            //Program.Client.Dispose();
            Gtk.Application.Quit();
            Environment.Exit(0);
        }
    }
}
