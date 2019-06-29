using System.Linq;
using Gtk;

namespace TelegramApp.Views
{
    class DigitsEntry : Entry
    {
        protected override void OnTextInserted(string newText, ref int position)
        {
            if (newText.All(char.IsDigit))
            {
                base.OnTextInserted(newText, ref position);
            }
        }
    }
}
