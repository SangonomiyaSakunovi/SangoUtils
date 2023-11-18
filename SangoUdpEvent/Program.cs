// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System.Text;

UDPSendMessage("127.0.0.1",52022,"这是一段来自Sango的UDP测试数据");
UDPSendMessage("127.0.0.1",52022,"这个String类型的数据收到了吗？");
UDPSendMessage("127.0.0.1",52022,"0123456789");
UDPSendMessage("127.0.0.1",52022,"Can you give me response?");
UDPSendMessage("127.0.0.1",52022,"SangoTestTokenKey");

void UDPSendMessage(string remoteIP, int remotePort, string message)
{
    byte[] sendbytes = Encoding.UTF8.GetBytes(message);
    IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
    UdpClient udpSend = new UdpClient();
    udpSend.Send(sendbytes, sendbytes.Length, remoteIPEndPoint);
    udpSend.Close();
}
