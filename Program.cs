using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{

    class Program
    {
        private static int GetPort()
        {//same as nestat -a
            Console.WriteLine("Введите PORT");
            int port = Convert.ToInt32(Console.ReadLine());
            bool isAvailable = true;

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    Console.WriteLine("Invalid Port");
                    break;
                }
            }

            if (isAvailable == true)
                return port;
            else
            {
                GetPort();
                return 0;
            }

        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Port to listen && HOST");
                SocketServer(GetPort(), Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static void SocketServer(int port, string host)
        {
            var Host = Dns.GetHostEntry(host);
            var ipv4 = Host.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipv4, port);
            var Listener = new Socket(ipv4.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(ipEndPoint);
                Listener.Listen(10);
                while (true)
                {
                    Console.WriteLine("Server port {0}", ipEndPoint);
                    Socket handler = Listener.Accept(); //not async
                    string data = null;
                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("Received: {0}", data);
                    string reply = "Hello, message lgth " + data.Length.ToString();
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    handler.Send(msg);

                    if (data.IndexOf("<TheEnd>") > -1)
                    {
                        Console.WriteLine("Сервер завершил соединение с клиентом.");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
