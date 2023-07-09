using System.Collections.Generic;

//Developer: SangonomiyaSakunovi

namespace SangoPriorityQueue
{
    public class MinHeap
    {
        private List<int> _heapList;

        public MinHeap(int capacity = 4)
        {
            _heapList = new List<int>(capacity);
        }

        public void AddNode(int value)
        {
            _heapList.Add(value);
            //Heaplize from bottom
            int childIndex = _heapList.Count - 1;
            int parentIndex = (childIndex - 1) / 2;
            while (childIndex > 0 && _heapList[childIndex] < _heapList[parentIndex])
            {
                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
                parentIndex = (childIndex - 1) / 2;
            }
        }

        public int RemoveNode()
        {
            if(_heapList.Count == 0)
            {
                return int.MinValue;
            }
            int value = _heapList[0];
            int endIndex = _heapList.Count - 1;
            _heapList[0] = _heapList[endIndex];
            _heapList.RemoveAt(endIndex);
            endIndex--;
            //Heaplize from top
            int topIndex = 0;
            while (true)
            {
                int minIndex = topIndex;
                int leftChildIndex = topIndex * 2 + 1;
                if (leftChildIndex <= endIndex &&  _heapList[leftChildIndex] < _heapList[minIndex])
                {
                    minIndex = leftChildIndex;
                }
                int rightChildIndex = leftChildIndex + 1;
                if (rightChildIndex <= endIndex && _heapList[rightChildIndex] < _heapList[minIndex])
                {
                    minIndex = rightChildIndex;
                }
                if (minIndex == topIndex) { break; }
                Swap(minIndex, topIndex);
                topIndex = minIndex;
            }
            return value;
        }

        private void Swap(int a, int b)
        {
            int temp = _heapList[a];
            _heapList[a] = _heapList[b];
            _heapList[b] = temp;
        }
    }
}
