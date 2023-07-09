using System;
using System.Collections.Generic;

//Developer: SangonomiyaSakunovi

namespace SangoPriorityQueue
{
    public class SangoPriorityQueue<T> where T : IComparable<T>
    {
        private List<T> _queueList;
        public int Count { get => _queueList.Count; }

        public SangoPriorityQueue(int capacity = 4)
        {
            _queueList = new List<T>(capacity);
        }

        public void Enqueue(T item)
        {
            _queueList.Add(item);
            HeapifyUp(_queueList.Count - 1);
        }

        public T Dequeue()
        {
            if (_queueList.Count == 0)
            {
                return default;
            }
            T item = _queueList[0];
            int endIndex = _queueList.Count - 1;
            _queueList[0] = _queueList[endIndex];
            _queueList.RemoveAt(endIndex);
            endIndex--;
            HeapifyDown(0, endIndex);
            return item;
        }

        public T Peek()
        {
            return _queueList.Count > 0 ? _queueList[0] : default;
        }

        public int IndexOf(T item)
        {
            return _queueList.IndexOf(item);
        }

        public T RemoveAt(int index)
        {
            if (index > _queueList.Count)
            {
                return default;
            }
            T item = _queueList[index];
            int endIndex = _queueList.Count - 1;
            _queueList[index] = _queueList[endIndex];
            _queueList.RemoveAt(endIndex);
            endIndex--;

            if (index < endIndex)
            {
                int parentIndex = (index - 1) / 2;
                if (parentIndex > 0 && _queueList[index].CompareTo(_queueList[parentIndex]) < 0)
                {
                    HeapifyUp(index);
                }
                else
                {
                    HeapifyDown(index, endIndex);
                }
            }
            return item;
        }

        public T RemoveItem(T item)
        {
            int index= _queueList.IndexOf(item);
            return index != -1 ? RemoveAt(index) : default;
        }

        public void Clear()
        {
            _queueList.Clear();
        }

        public bool Contains(T item)
        {
            return _queueList.Contains(item);
        }

        public bool IsEmpty()
        {
            return _queueList.Count == 0;
        }

        public List<T> ToList()
        {
            return _queueList;
        }

        public T[] ToArray()
        {
            return _queueList.ToArray();
        }

        private void Swap(int a, int b)
        {
            T temp = _queueList[a];
            _queueList[a] = _queueList[b];
            _queueList[b] = temp;
        }

        private void HeapifyDown(int topIndex, int endIndex)
        {
            while (true)
            {
                int minIndex = topIndex;
                int leftChildIndex = topIndex * 2 + 1;
                if (leftChildIndex <= endIndex && _queueList[leftChildIndex].CompareTo(_queueList[minIndex]) < 0)
                {
                    minIndex = leftChildIndex;
                }
                int rightChildIndex = leftChildIndex + 1;
                if (rightChildIndex <= endIndex && _queueList[rightChildIndex].CompareTo(_queueList[minIndex]) < 0)
                {
                    minIndex = rightChildIndex;
                }
                if (minIndex == topIndex) { break; }
                Swap(minIndex, topIndex);
                topIndex = minIndex;
            }
        }

        private void HeapifyUp(int childIndex)
        {
            int parentIndex = (childIndex - 1) / 2;
            while (childIndex > 0 && _queueList[childIndex].CompareTo(_queueList[parentIndex]) < 0)
            {
                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
                parentIndex = (childIndex - 1) / 2;
            }
        }
    }
}
