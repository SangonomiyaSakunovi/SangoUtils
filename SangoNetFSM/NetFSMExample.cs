//Developer: SangonomiyaSakunovi

namespace SangoNetFSM
{
    public enum NetStateCode
    {
        None,
        Connecting,
        Validating,
        Connected,
        DisConnected
    }

    public enum NetOpCode
    {
        None,
        Connect,
        Validate,
        DisConnect
    }

    public class NetFSMExample
    {
        private string _fsmName;

        private SangoFSM<NetStateCode> _testNetStater;
        private SangoFSMInput _inputConnectOp = new SangoFSMInputEnum<NetOpCode>(NetOpCode.Connect);
        private SangoFSMInput _inputValidOp = new SangoFSMInputEnum<NetOpCode>(NetOpCode.Validate);
        private SangoFSMInput _inputDataOp = new SangoFSMInputData(null);
        private SangoFSMInput _inputDisConnectOp = new SangoFSMInputEnum<NetOpCode>(NetOpCode.DisConnect);

        public void InitFSM(string name)
        {
            _fsmName = name;
            _testNetStater = new SangoFSM<NetStateCode>(NetStateCode.None);

            _testNetStater.AddLocalTransition(NetStateCode.Connecting, _inputValidOp, NetStateCode.Validating, TrySendValidateInfo);
            _testNetStater.AddLocalTransition(NetStateCode.Validating, _inputDataOp, NetStateCode.Connected, TryValidateResult);
            _testNetStater.AddLocalTransition(NetStateCode.Connected, _inputDataOp, NetStateCode.Connected, TryHandleNetMessage);

            _testNetStater.AddGlobalTransition(_inputConnectOp, NetStateCode.Connecting, TryConnectServer);
            _testNetStater.AddGlobalTransition(_inputDisConnectOp, NetStateCode.DisConnected, TryDisConnected);

            _testNetStater._transitionCB = (NetStateCode before, NetStateCode after) =>
            {
                this.LogDone(_fsmName + ": " + before + " has switched to " + after);
            };

            while (true)
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "a":
                        SendSwitchCMD(_inputConnectOp);
                        break;
                    case "b":
                        SendSwitchCMD(_inputValidOp);
                        break;
                    case "c":
                        SendSwitchCMD(_inputDataOp);
                        break;
                    case "d":
                        SendSwitchCMD(_inputDataOp);
                        break;
                    case "e":
                        SendSwitchCMD(_inputDisConnectOp);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SendSwitchCMD(SangoFSMInput input)
        {
            _testNetStater.ProcessInput(input);
        }

        private bool TryConnectServer(NetStateCode currentState, SangoFSMInput input, NetStateCode destinationState)
        {
            return true;
        }

        private bool TrySendValidateInfo(NetStateCode currentState, SangoFSMInput input, NetStateCode destinationState)
        {
            return true;
        }

        private bool TryValidateResult(NetStateCode currentState, SangoFSMInput input, NetStateCode destinationState)
        {
            return true;
        }

        private bool TryHandleNetMessage(NetStateCode currentState, SangoFSMInput input, NetStateCode destinationState)
        {
            return true;
        }

        private bool TryDisConnected(NetStateCode currentState, SangoFSMInput input, NetStateCode destinationState)
        {
            return true;
        }

        public void UnInitFSM()
        {
            _testNetStater.CleanInputCache();
        }
    }
}
