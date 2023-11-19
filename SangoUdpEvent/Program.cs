// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System.Text;

UDPSendMessage("255.255.255.255",52022,"ABCDEFG|1234567|这是测试|abcdefg");

void UDPSendMessage(string remoteIP, int remotePort, string message)
{
    byte[] sendbytes = Encoding.UTF8.GetBytes(message);
    IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
    UdpClient udpSend = new UdpClient();
    udpSend.Send(sendbytes, sendbytes.Length, remoteIPEndPoint);
    udpSend.Close();
}
