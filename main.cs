using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Mono.Options;

namespace lab02
{
    class Program
    {

        static void Main(string[] args)
        {
            bool show_help = false;
            int port = 8015;
            string dir = Directory.GetCurrentDirectory();
            var p = new OptionSet()
            {
                {
                    "d|dir=",
                    "Source directory with files.\n",
                    v =>
                    {
                        if (v != null)
                        {
                            dir = v;
                        }
                    }
                },
                {
                    "p|port=", "server port",
                    v =>
                    {
                        if (v != null && v.All(Char.IsDigit))
                        {
                            port = Int32.Parse(v);
                        }

                    }
                },
                {
                    "h|help", "show this message and exit",
                    v => show_help = v != null
                },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
                return;
            }

            TcpListener listener = new TcpListener(port);
            listener.Start();
            while (true)
            {

                Console.WriteLine("Waiting for a connetion");
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Accepted new client connection...");
                Thread tcpListenerThread = new Thread(() =>
                {
                    bool f = false;
                    StreamReader sr = new StreamReader(client.GetStream());
                    StreamWriter sw = new StreamWriter(client.GetStream());
                    try
                    {
                        string request = sr.ReadLine();
                        Console.WriteLine(request);
                        string[] tokens = request.Split(' ');
                        string page = tokens[1];
                        if (page == "/")
                        {
                            page = "/index.html";
                        }

                        string checkfile = dir + "\\" + page.Remove(0, 1);
 
                        try
                        {
                            if (!File.Exists(checkfile))
                                throw new FileNotFoundException();
                            else
                                f = true;
                        }
                        catch (FileNotFoundException e)
                        {
                            sw.WriteLine("HTTP/1.0 404 OK\n");
                            sw.WriteLine("<H1>ERROR 404 FILE NOT FOUND</H1>");
                            sw.Flush();

                        }

                        if (f)
                        {
                            StreamReader file = new StreamReader(dir + page);

                            sw.WriteLine("HTTP/1.0 200 OK\n");
                            string data = file.ReadLine();
                            while (data != null)
                            {
                                sw.WriteLine(data);
                                sw.Flush();
                                data = file.ReadLine();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        sw.WriteLine("HTTP/1.0 500 OK\n");
                        sw.WriteLine("<H1>ERROR 500</H1>");
                        sw.Flush();
                    }

                    client.Close();
                });
                tcpListenerThread.Start();
            }
        }
    }
}