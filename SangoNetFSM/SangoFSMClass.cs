//Developer: SangonomiyaSakunovi

namespace SangoNetFSM
{
    public abstract class SangoFSMInput { }

    public class SangoFSMInputEnum<T> : SangoFSMInput where T : struct
    {
        private T _operationEnum;

        public SangoFSMInputEnum(T operationEnum)
        {
            _operationEnum = operationEnum;
        }

        public override bool Equals(object? obj)
        {
            if (obj is SangoFSMInputEnum<T>)
            {
                SangoFSMInputEnum<T> input = obj as SangoFSMInputEnum<T>;
                return input._operationEnum.Equals(_operationEnum);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _operationEnum.GetHashCode();
        }
    }

    public class SangoFSMInputData : SangoFSMInput
    {
        private byte[] _data;

        public SangoFSMInputData(byte[] data)
        {
            _data = data;
        }

        public override bool Equals(object? obj)
        {
            if (obj is SangoFSMInputData)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class SangoFSMItem<T> where T : struct
    {
        public SangoFSMInput _input;
        public T _destinationState;
        public Func<T, SangoFSMInput, T, bool> _isCanTransStateCB;

        public SangoFSMItem(SangoFSMInput input, T destinationState, Func<T, SangoFSMInput, T, bool> isCanTransStateCB)
        {
            _input = input;
            _destinationState = destinationState;
            _isCanTransStateCB = isCanTransStateCB;
        }
    }
}
