using System;
using System.Net;
using System.Threading.Tasks;

namespace GsmApiApp
{
    class Program
    {
        static async Task Main(string[] args)
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
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                string Phone, Body, Index, responseMsg = "";

                switch (request.Url.AbsolutePath)
                {
                    case "/api/send":
                        try
                        {
                            Phone = request.QueryString.GetValues(0)[0];
                            Body = request.QueryString.GetValues(1)[0];
                            if (await Send(Phone, Body))
                            {
                                responseMsg = $"send sms {Body} to {Phone} successfully";
                            }
                            else
                            {
                                responseMsg = $"failed to send sms {Body} to {Phone}";
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(responseMsg);
                        }
                        catch (Exception)
                        {
                            responseMsg = "Unknow command";
                        }
                        break;
                    case "/api/readAll":
                        try
                        {
                            if (await ReadAll())
                            {
                                responseMsg = "read all sms successfully";
                            }
                            else
                            {
                                responseMsg = "failed to read all sms";
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(responseMsg);
                        }
                        catch (Exception)
                        {
                            responseMsg = "Unknow command";
                        }
                        break;
                    case "/api/readAllPhone":
                        try
                        {
                            Phone = request.QueryString.GetValues(0)[0];
                            if (await ReadAllPhone(Phone))
                            {
                                responseMsg = $"read all sms of {Phone} successfully";
                            }
                            else
                            {
                                responseMsg = $"failed to read all sms of {Phone}";
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(responseMsg);
                        }
                        catch (Exception)
                        {
                            responseMsg = "Unknow command";
                        }
                        break;
                    case "/api/read":
                        try
                        {
                            Index = request.QueryString.GetValues(0)[0];
                            if (await Read(Index))
                            {
                                responseMsg = $"read sms with index {Index} successfully";
                            }
                            else
                            {
                                responseMsg = $"failed to read sms with index {Index}";
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(responseMsg);
                        }
                        catch (Exception)
                        {
                            responseMsg = "Unknow command";
                        }
                        break;
                    case "/api/removeAll":
                        try
                        {
                            if (await RemoveAll())
                            {
                                responseMsg = "remove all sms successfully";
                            }
                            else
                            {
                                responseMsg = "failed to remove all sms successfully";
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(responseMsg);
                        }
                        catch (Exception)
                        {
                            responseMsg = "Unknow command";
                        }
                        break;
                    case "/api/remove":
                        try
                        {
                            Index = request.QueryString.GetValues(0)[0];
                            if (await Remove(Index))
                            {
                                responseMsg = $"remove sms with index {Index} successfully";
                            }
                            else
                            {
                                responseMsg = $"failed to remove sms with index {Index}";
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(responseMsg);
                        }
                        catch (Exception)
                        {
                            responseMsg = "Unknow command";
                        }
                        break;
                    default:
                        responseMsg = "Unknow command";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(responseMsg);
                        break;
                }

                HttpListenerResponse response = context.Response;
                string responseString = responseMsg;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);

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

        private static async Task<bool> Send(string phone, string body)
        {
            await Task.Delay(5000);
            return true;
        }

        private static async Task<bool> ReadAll()
        {
            await Task.Delay(5000);
            return true;
        }

        private static async Task<bool> ReadAllPhone(string phone)
        {
            await Task.Delay(5000);
            return true;
        }

        private static async Task<bool> Read(string index)
        {
            await Task.Delay(5000);
            return true;
        }

        private static async Task<bool> RemoveAll()
        {
            await Task.Delay(5000);
            return true;
        }

        private static async Task<bool> Remove(string index)
        {
            await Task.Delay(5000);
            return true;
        }
    }
}
