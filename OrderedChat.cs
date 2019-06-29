using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramApp
{
    class OrderedChat : IComparable
    {
        public long Order { get; set; }
        public long Id { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            var otherChat = obj as OrderedChat;
            if (otherChat != null)
                return this.Order.CompareTo(otherChat.Order);
            else
                throw new ArgumentException("Object is not a OrderedChat");
        }
    }
}
