using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMessageManager
{
    class MessageProvider : IList<Message>
    {
        public Message this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(Message item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Message item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Message[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Message> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(Message item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Message item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Message item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
