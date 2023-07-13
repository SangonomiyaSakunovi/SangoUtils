using System;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

//Developer: SangonomiyaSakunovi

namespace SangoKCPNet
{
    public abstract class IClientPeer<T> where T : KCPMessage, new()
    {
        protected uint _peerId;
        private Action<byte[], IPEndPoint> _udpSender;
        private IPEndPoint _remotePoint;
        protected ConnectionStateCode _connectionState = ConnectionStateCode.None;

        public Action<uint> OnClientPeerCloseCallBack;

        public KCPHandle _handle;
        public Kcp _kcp;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public void InitClientPeer(uint sessionId, Action<byte[], IPEndPoint> udpSender, IPEndPoint remotePoint)
        {
            KCPLog.Info("Init Client Peer.");
            _peerId = sessionId;
            _udpSender = udpSender;
            _remotePoint = remotePoint;
            _connectionState = ConnectionStateCode.Connected;

            _handle = new KCPHandle();
            _kcp = new Kcp(sessionId, _handle);
            _kcp.NoDelay(1, 10, 2, 1);
            _kcp.WndSize(64, 64);
            _kcp.SetMtu(512);

            _handle.Out = (Memory<byte> buffer) =>
            {
                byte[] bytes = buffer.ToArray();
                _udpSender(bytes, remotePoint);
            };

            _handle.Recv = (byte[] buffer) =>
            {
                buffer = KCPTool.DeCompress(buffer);
                T message = KCPTool.DeSerialize<T>(buffer);
                if (message != null)
                {
                    OnReceivedMessage(message);
                }
            };

            OnConnected();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            Task.Run(Update, _cancellationToken);
        }

        protected abstract void OnConnected();

        protected abstract void OnUpdate(DateTimeOffset now);

        protected abstract void OnDisconnected();

        protected abstract void OnReceivedMessage(byte[] byteMessages);
        protected abstract void OnReceivedMessage(T byteMessages);

        public void RecieveData(byte[] buffer)
        {
            _kcp.Input(buffer.AsSpan());
        }

        private async void Update()
        {
            try
            {
                while (true)
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    OnUpdate(now);                   
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        KCPLog.Done("ClientPeer Update is Cancelled.");
                        break;
                    }
                    else
                    {
                        _kcp.Update(now);
                        int length;
                        while ((length = _kcp.PeekSize()) > 0)
                        {
                            var buffer = new byte[length];
                            if (_kcp.Recv(buffer) >= 0)
                            {
                                _handle.Receive(buffer);
                            }
                        }
                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception ex)
            {
                KCPLog.Warning("ClientPeer Update Exception:{0}", ex.ToString());
            }
        }

        public void SendMessage(T message)
        {
            if (IsConnected())
            {
                byte[] bytes = KCPTool.Serialize(message);
                if (bytes != null)
                {
                    SendMessage(bytes);
                }
            }
            else
            {
                KCPLog.Warning("ClientPeer is Disconnected, peerId is:{0}", _peerId);
            }
        }

        public void SendMessage(byte[] bytes)
        {
            if (IsConnected())
            {
                bytes = KCPTool.Compress(bytes);
                _kcp.Send(bytes.AsSpan());
            }
            else
            {
                KCPLog.Warning("ClientPeer is Disconnected, peerId is:{0}", _peerId);
            }
        }

        public void CloseClientPeer()
        {
            _cancellationTokenSource.Cancel();
            OnDisconnected();

            OnClientPeerCloseCallBack?.Invoke(_peerId);
            OnClientPeerCloseCallBack = null;

            _connectionState = ConnectionStateCode.Disconnected;
            _remotePoint = null;
            _udpSender = null;
            _peerId = 0;
            
            _handle = null;
            _kcp = null;
            _cancellationTokenSource = null;
        }

        public bool IsConnected()
        {
            return _connectionState == ConnectionStateCode.Connected;
        }
    }

    [Serializable]
    public abstract class KCPMessage
    {

    }
}
