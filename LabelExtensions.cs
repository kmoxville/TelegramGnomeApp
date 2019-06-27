using System;
using System.Collections.Generic;
using System.Text;

using Gtk;

namespace TelegramApp
{
    public static class LabelExtensions
    {
        public static void SetFontSize(this Label label, int fontSize)
        {
            var provider = new CssProvider();
            provider.LoadFromData($"label {{ font-size: {fontSize}px; }}");
            label.StyleContext.AddProvider(provider, Gtk.StyleProviderPriority.User);
        }
    }
}
