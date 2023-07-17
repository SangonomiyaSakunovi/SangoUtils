//Developer: SangonomiyaSakunovi

namespace SangoNetFSM
{
    public class SangoFSM<T> where T : struct
    {
        public Action<T, T> _transitionCB;

        private T _currentState;
        private Dictionary<T, List<SangoFSMItem<T>>> _transition1ToNDict;
        private List<SangoFSMItem<T>> _transitionNTo1List;

        private bool _isInputProcessing = false;
        private Queue<SangoFSMInput> _cacheInputQueue;

        public SangoFSM(T startState)
        {
            _currentState = startState;
            _transition1ToNDict = new Dictionary<T, List<SangoFSMItem<T>>>();
            _transitionNTo1List = new List<SangoFSMItem<T>>();
            _cacheInputQueue = new Queue<SangoFSMInput>();
        }

        public void AddLocalTransition(T currentState, SangoFSMInput input, T destinationState, Func<T, SangoFSMInput, T, bool> isCanTransStateCB)
        {
            SangoFSMItem<T> item = new SangoFSMItem<T>(input, destinationState, isCanTransStateCB);
            if (_transition1ToNDict.TryGetValue(currentState, out List<SangoFSMItem<T>> itemList))
            {
                itemList.Add(item);
            }
            else
            {
                itemList = new List<SangoFSMItem<T>>() { item };
                _transition1ToNDict.Add(currentState, itemList);
            }
        }

        public void AddGlobalTransition(SangoFSMInput input, T destinationState, Func<T, SangoFSMInput, T, bool> isCanTransStateCB)
        {
            SangoFSMItem<T> item = new SangoFSMItem<T>(input, destinationState, isCanTransStateCB);
            _transitionNTo1List.Add(item);
        }

        public void ProcessInput(SangoFSMInput input)
        {
            if (_isInputProcessing)
            {
                _cacheInputQueue.Enqueue(input);
                return;
            }            
            _isInputProcessing = true;
            bool result = false;
            if (_transition1ToNDict.TryGetValue(_currentState, out List<SangoFSMItem<T>> itemList))
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    result = TransWork(itemList[i], input);
                    if (result)
                    {
                        break;
                    }
                }
            }
            if (!result)
            {
                for(int i = 0; i < _transitionNTo1List.Count; i++)
                {
                    result = TransWork(_transitionNTo1List[i], input);
                    if (result)
                    {
                        break;
                    }
                }
            }
            _isInputProcessing = false;
            if (_cacheInputQueue.Count > 0)
            {
                SangoFSMInput cacheInput = _cacheInputQueue.Dequeue();
                ProcessInput(cacheInput);               
            }
        }

        private bool TransWork(SangoFSMItem<T> item, SangoFSMInput input)
        {
            bool result = false;
            if (item._input.Equals(input))
            {
                if (item._isCanTransStateCB != null)
                {
                    result = item._isCanTransStateCB(_currentState, input, item._destinationState);
                }
                else
                {
                    result = true;
                }
                if (result)
                {
                    T preState = _currentState;
                    _currentState = item._destinationState;
                    _transitionCB?.Invoke(preState, _currentState);
                }
            }
            return result;
        }

        public void CleanInputCache()
        {
            _cacheInputQueue.Clear();
        }
    }
}
