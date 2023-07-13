using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

//Developer: SangonomiyaSakunovi

namespace SangoKCPNet
{
    public class KCPPeer<T, K> where T : IClientPeer<K>, new()
                              where K : KCPMessage, new()
    {
        private UdpClient _udpClient;
        private IPEndPoint _remotePoint;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public KCPPeer()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        #region Client
        public T ClientPeer;

        public void StartAsClient(string ipAddress, int port)
        {
            KCPLog.Start("Start as Client.");
            _udpClient = new UdpClient(0);
            _remotePoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            Task.Run(ClientRecieve, _cancellationToken);
        }

        //If the  message is [0,0,0,0], we define that`s a new client, the Server will give it a PeerId
        public void ConnectServer()
        {
            SendUdpMessage(new byte[4], _remotePoint);
        }

        private async void ClientRecieve()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        KCPLog.Done("Client Recieve Task is Cancelled.");
                        break;
                    }
                    result = await _udpClient.ReceiveAsync();

                    if (Equals(_remotePoint, result.RemoteEndPoint))
                    {
                        uint peerId = BitConverter.ToUInt32(result.Buffer, 0);
                        if (peerId == 0)    //First connect
                        {
                            if (ClientPeer != null && ClientPeer.IsConnected())
                            {
                                //Already connected, that`s surplus info
                                KCPLog.Info("Client is Init Done, abandon the surplus info.");
                            }
                            else
                            {
                                peerId = BitConverter.ToUInt32(result.Buffer, 4);
                                KCPLog.Done("Udp Request peerId:{0}", peerId);

                                ClientPeer = new T();
                                ClientPeer.InitClientPeer(peerId, SendUdpMessage, _remotePoint);
                                ClientPeer.OnClientPeerCloseCallBack = OnClientPeerClose;
                            }
                        }
                        else
                        {
                            if (ClientPeer != null && ClientPeer.IsConnected())
                            {
                                ClientPeer.RecieveData(result.Buffer);
                            }
                            else
                            {
                                KCPLog.Warning("Client is Initing, abandon this message.");
                            }
                        }
                    }
                    else
                    {
                        KCPLog.Warning("Client Udp Recieve Illegal target Data, Ip:{0}", _remotePoint.Address.ToString());
                    }
                }
                catch (Exception ex)
                {
                    KCPLog.Warning("Client Udp Recieve Data Exception:{0}", ex.ToString());
                }
            }
        }

        private void OnClientPeerClose(uint peerId)
        {
            _cancellationTokenSource.Cancel();
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }
            KCPLog.Done("Client Peer Close, peerId: {0}", peerId);
        }

        public void CloseClient()
        {
            if (ClientPeer != null)
            {
                ClientPeer.CloseClientPeer();
            }
        }
        #endregion

        private void SendUdpMessage(byte[] bytes, IPEndPoint remotePoint)
        {
            if (_udpClient != null)
            {
                _udpClient.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
    }
}
