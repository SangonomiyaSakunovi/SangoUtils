using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

//Developer: SangonomiyaSakunovi
//If the message buffer(0) is 0, we define that`s a new client, the Server will give it a PeerId
//If the message head is [0,0,0,0], we define the message is PeerId

namespace SangoKCPNet
{
    public class KCPPeer<T> where T : IClientPeer, new()
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

        #region Server
        private Dictionary<uint, T> _peerDict;

        public void StartAsServer(string ipAddress, int port)
        {
            KCPLog.Start("Start as Server.");
            _peerDict = new Dictionary<uint, T>();
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //In windowsPlatForm, when you force to close a udp, will be a Warning: 10054
                _udpClient.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            _remotePoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            Task.Run(ServerReceive, _cancellationToken);
        }

        private async void ServerReceive()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        KCPLog.Done("Server Recieve Task is Cancelled.");
                        break;
                    }
                    result = await _udpClient.ReceiveAsync();

                    uint peerId = BitConverter.ToUInt32(result.Buffer, 0);
                    if (peerId == 0)
                    {
                        peerId = GeneratePeerId();
                        byte[] peerId_bytes = BitConverter.GetBytes(peerId);
                        byte[] peerId_message = new byte[8];
                        Array.Copy(peerId_bytes, 0, peerId_message, 4, 4);

                        SendUdpMessage(peerId_message, result.RemoteEndPoint);
                    }
                    else
                    {
                        if (!_peerDict.TryGetValue(peerId, out T currentPeer))
                        {
                            KCPLog.Special("New Client Peer Connect, peerId: {0}", peerId);
                            currentPeer = new T();
                            currentPeer.InitClientPeer(peerId, SendUdpMessage, result.RemoteEndPoint);
                            currentPeer.OnClientPeerCloseCallBack = OnClientPeerCloseInServer;
                            lock (_peerDict)
                            {
                                _peerDict.Add(peerId, currentPeer);
                            }
                        }
                        else
                        {
                            currentPeer = _peerDict[peerId];
                        }
                        currentPeer.RecieveData(result.Buffer);
                    }
                }
                catch (Exception ex)
                {
                    KCPLog.Warning("Server Udp Recieve Data Exception:{0}", ex.ToString());
                }
            }
        }

        private void OnClientPeerCloseInServer(uint peerId)
        {
            if (_peerDict.ContainsKey(peerId))
            {
                lock (_peerDict)
                {
                    _peerDict.Remove(peerId);
                    KCPLog.Special("Client Peer Close, peerId: {0}", peerId);
                }
            }
            else
            {
                KCPLog.Error("PeerId: {0}, cannot find in PeerDict.", peerId);
            }
        }

        public void CloseServer()
        {
            foreach (var item in _peerDict)
            {
                item.Value.CloseClientPeer();
            }
            _peerDict = null;
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
                _cancellationTokenSource.Cancel();
            }
        }
        #endregion

        #region Client
        public T ClientPeer;

        public void StartAsClient(string ipAddress, int port)
        {
            KCPLog.Start("Start as Client.");
            _udpClient = new UdpClient(0);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //In windowsPlatForm, when you force to close a udp, will be a Warning: 10054
                _udpClient.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            _remotePoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            Task.Run(ClientReceive, _cancellationToken);
        }


        public Task<bool> ConnectServer(int heartBeatInterval, int maxInterval = 5000)
        {
            SendUdpMessage(new byte[4], _remotePoint);
            int sumHeartBeatTime = 0;
            Task<bool> heartBeatTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(heartBeatInterval);
                    sumHeartBeatTime += heartBeatInterval;
                    if (ClientPeer != null && ClientPeer.IsConnected())
                    {
                        return true;
                    }
                    else
                    {
                        if (sumHeartBeatTime > maxInterval)
                        {
                            return false;
                        }
                    }
                }
            });
            return heartBeatTask;
        }

        private async void ClientReceive()
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
                        if (peerId == 0)
                        {
                            if (ClientPeer != null && ClientPeer.IsConnected())
                            {
                                KCPLog.Info("Client is Init Done, abandon the surplus info.");
                            }
                            else
                            {
                                peerId = BitConverter.ToUInt32(result.Buffer, 4);
                                KCPLog.Special("Connect to Server, Udp Request peerId:{0}", peerId);

                                ClientPeer = new T();
                                ClientPeer.InitClientPeer(peerId, SendUdpMessage, _remotePoint);
                                ClientPeer.OnClientPeerCloseCallBack = OnClientPeerCloseInClient;
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

        private void OnClientPeerCloseInClient(uint peerId)
        {
            _cancellationTokenSource.Cancel();
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }
            KCPLog.Special("Client Peer Close, peerId: {0}", peerId);
        }

        public void CloseClient()
        {
            if (ClientPeer != null)
            {
                ClientPeer.CloseClientPeer();
            }
        }
        #endregion

        public void BroadCastMessage(byte[] bytes)
        {
            foreach (var item in _peerDict)
            {
                item.Value.SendMessage(bytes);
            }
        }

        private void SendUdpMessage(byte[] bytes, IPEndPoint remotePoint)
        {
            if (_udpClient != null)
            {
                _udpClient.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }

        private uint _freePeerId = 0;

        private uint GeneratePeerId()
        {
            lock (_peerDict)
            {
                while (true)
                {
                    ++_freePeerId;
                    if (_freePeerId == uint.MaxValue)
                    {
                        _freePeerId = 1;
                    }
                    if (!_peerDict.ContainsKey(_freePeerId))
                    {
                        break;
                    }
                }
            }
            return _freePeerId;
        }
    }
}
