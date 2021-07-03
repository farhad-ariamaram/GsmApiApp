using System;
using System.Net;

namespace GsmApiApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GSM Modem Api");
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("This app does not support on your system, try again with Admin user or other OS...");
                return;
            }
            Console.WriteLine("Listening on : http://localhost:2470/");
            while (true)
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:2470/");
                listener.Start();
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"new {request.HttpMethod} request");

                if(response.StatusCode == 200)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("status 200/OK");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"status {response.StatusCode}");
                }

                output.Close();
                listener.Stop();
                
            }
        }
    }
}
