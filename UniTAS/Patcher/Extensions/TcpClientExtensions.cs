using System.Net.Sockets;

namespace UniTAS.Patcher.Extensions;

public static class TcpClientExtensions
{
    public static bool IsConnected(this TcpClient client)
    {
        try
        {
            if (client is not { Client.Connected: true }) return false;
            
            /* pear to the documentation on Poll:
             * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
             * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
             * -or- true if data is available for reading;
             * -or- true if the connection has been closed, reset, or terminated;
             * otherwise, returns false
             */
            // Detect if client disconnected
            if (!client.Client.Poll(0, SelectMode.SelectRead)) return true;

            var buff = new byte[1];
            return client.Client.Receive(buff, SocketFlags.Peek) != 0;
        }
        catch
        {
            return false;
        }
    }
}