using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoRed
{
        class TStack<T>
        {
            private T[] arr;
            private int size;
            private int maxsize;

            public TStack(int _size = 5)
            {
                arr = new T[_size];
                maxsize = _size;
                size = 0;
            }


            public bool isfull()
            {
                return size == maxsize;
            }

            public bool isempty()
            {
                return size == 0;
            }

            public T top()
            {
                return arr[size - 1];
            }

            public T pop()
            {
                size--;
                return arr[size];
            }

            public void push(T el)
            {
                arr[size] = el;
                size++;
            }

            void clear()
            {
                size = 0;
            }

            int getsize()
            {
                return size;
            }

            int getmaxsize()
            {
                return maxsize;
            }
        }
    
}
