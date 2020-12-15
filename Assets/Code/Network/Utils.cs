using System.IO;
using System.Net.Sockets;

public class Utils
{
    public static int TCP_PORT = 11000;
    public static int UDP_PORT = 11001;

    public static byte[] ReadData(NetworkStream stream)
    {
        byte[] buffer = new byte[128];
        using (MemoryStream ms = new MemoryStream())
        {
            do
            {
                stream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer,0,buffer.Length);
            } while (stream.DataAvailable);

            return ms.ToArray();
        }
    }
}