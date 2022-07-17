using System;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace Client
{
    class serv
    {
        public static byte[] merge(byte[] id, byte[] pack)
        {
            byte[] merged = new byte[id.Length + pack.Length + 1];
            Array.Copy(id, 0, merged, 0, id.Length);
            merged[id.Length] = (byte)'|';
            Array.Copy(pack, 0, merged, id.Length + 1, pack.Length);
            return merged;
        }
        static void Main(string[] args)
        {
            IPAddress ip;
            int port;
            int portsend;
            string path;
            int waittime;
            if (args.Length > 0)
            {
                ip = IPAddress.Parse(args[0]);
                port = Convert.ToInt32(args[1]);
                portsend = Convert.ToInt32(args[2]);
                path = args[3];
                waittime = Convert.ToInt32(args[4]);
            }
            else
            {
                ip = IPAddress.Parse("127.0.0.1");
                port = 5555;
                portsend = 60000;
                path = "test.txt";
                waittime = 500;
            }
            int packnum = 5;
            TcpClient client = new TcpClient();
            client.Connect(ip, port);
            NetworkStream stream = client.GetStream();
            StreamReader file = new StreamReader(path);
            byte[] data = Encoding.UTF8.GetBytes(Path.GetFileName(path) + "|" + portsend);
            UdpClient sender = new UdpClient();
            stream.Write(data);
            stream.Read(data);
            data = Encoding.UTF8.GetBytes(file.ReadToEnd());
            file.Close();
            byte[] sub;
            byte[] res = new byte[256];
            int i = (int)Math.Ceiling((double)data.Length / packnum);
            bool check;
            stream.ReadTimeout = waittime;
            for (int c = 0; c < packnum; c++)
            {
                if (c == packnum - 1)
                {
                    sub = new byte[data.Length - i * c];
                    Array.Copy(data, i * c, sub, 0, data.Length - i * c);
                }
                else
                {
                    sub = new byte[i];
                    Array.Copy(data, i * c, sub, 0, i);
                }
                byte[] id = Encoding.UTF8.GetBytes(Convert.ToString(c + 1));
                check = false;
                while (check == false)
                {
                    stream.Write(Encoding.UTF8.GetBytes("0"));
                    sender.Send(merge(id, sub), sub.Length + id.Length+1, ip.ToString(), portsend);
                    try
                    {
                        stream.Read(res);
                        check = true;
                    }
                    catch
                    {
                        IOException e;
                    }
                }
            }
            check = false;
            while (check == false)
            {
                stream.Write(Encoding.UTF8.GetBytes("1"));
                try
                {
                    stream.Read(res);
                    if (Encoding.UTF8.GetString(res).Contains("s"))
                        check = true;
                }
                catch { IOException e; }

            }
            stream.Close();
        }
    }
}