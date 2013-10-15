using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gpserv
{
    public class deque<T>
    {
        private int size;
        public T[] v;
        int toProduce, toConsume;
        public int length;

        public deque(int _size)
        {
            this.size = _size;
            v = new T[size];
            length = toProduce = toConsume = 0;
        }

        public void clear()
        {
            length = toProduce = toConsume = 0;
        }

        public bool empty()
        {
            return toProduce == toConsume;
        }

        public bool full()
        {
            return toProduce == (toConsume - 1 + size)%size;
        }

        public T front()
        {
            if (empty())
            {
                throw new DequeException("Deque is empty");
            }
            return v[toConsume];
        }

        public T back()
        {
            if (empty())
            {
                throw new DequeException("Deque is empty");
            }
            return v[toProduce-1];
        }

        public void pop_front()
        {
            if (empty()) throw new DequeException("Deque is empty");
            toConsume = (toConsume+1)%size;
            length--;
        }

        public void pop_back()
        {
            if (empty()) throw new DequeException("Deque is empty");
            toProduce = (toProduce - 1 + size) % size;
            length--;
        }

        public void push_front(T item)
        {
            if (full()) throw new DequeException("Deque is full");
            toConsume = (toConsume - 1 + size) % size;
            v[toConsume] = item;
            length++;
        }

        public void push_back(T item)
        {
            if (full()) throw new DequeException("Deque is full");
            v[toProduce] = item;
            toProduce = (toProduce + 1) % size;
            length++;
        }

    }

    [Serializable]
    public class DequeException : Exception
    {
        public DequeException() { }
        public DequeException(string message) : base(message) { }
        public DequeException(string message, Exception inner) : base(message, inner) { }
        protected DequeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

}