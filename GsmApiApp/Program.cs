﻿using GsmApiApp.Models;
using GsmApiApp.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GsmApiApp
{
    class Program
    {
        static string GSMPort;
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            //var connectionString = config["ConnectionString"];
            GSMPort = config["GSM:Port"];

            Console.WriteLine("GSM Modem Api");
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("This app does not support on your system, try again with Admin user or other OS...");
                return;
            }
            Console.WriteLine("Listening on : http://localhost:2470/");
            while (true)
            {
                string mode = null;

                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:2470/");
                listener.Start();
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"new {request.HttpMethod} request");

                string Phone, Body, Index, responseMsg = "";
                Tuple<List<SMS>, bool> resultReadAll = null;
                Tuple<List<SMS>, bool> resultReadAllPhone = null;
                Tuple<SMS, bool> resultRead = null;

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
                        mode = "send";
                        break;
                    case "/api/readAll":
                        try
                        {
                            resultReadAll = await ReadAll();
                            if (resultReadAll.Item2)
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
                        catch (Exception e)
                        {
                            responseMsg = $"Error: {e.Message}";
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(responseMsg);
                        }
                        mode = "readAll";
                        break;
                    case "/api/readAllPhone":
                        try
                        {
                            Phone = request.QueryString.GetValues(0)[0];
                            resultReadAllPhone = await ReadAllPhone(Phone);
                            if (resultReadAllPhone.Item2)
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
                        mode = "readAllPhone";
                        break;
                    case "/api/read":
                        try
                        {
                            Index = request.QueryString.GetValues(0)[0];
                            resultRead = await Read(Index);
                            if (resultRead.Item2)
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
                        mode = "read";
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
                        mode = "removeAll";
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
                        mode = "remove";
                        break;
                    default:
                        responseMsg = "Unknow command";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(responseMsg);
                        mode = "default";
                        break;
                }

                string result = null;
                switch (mode)
                {
                    case "send":
                        result = "";
                        break;
                    case "readAll":
                        result = JsonSerializer.Serialize(resultReadAll.Item1);
                        break;
                    case "readAllPhone":
                        result = JsonSerializer.Serialize(resultReadAllPhone.Item1);
                        break;
                    case "read":
                        result = JsonSerializer.Serialize(resultRead.Item1);
                        break;
                    case "removeAll":
                        result = "";
                        break;
                    case "remove":
                        result = "";
                        break;
                    case "default":
                        result = "";
                        break;
                    default:
                        result = "";
                        break;
                }

                HttpListenerResponse response = context.Response;
                System.IO.Stream output = response.OutputStream;
                try
                {
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(result);
                    response.ContentLength64 = buffer.Length;
                    await output.WriteAsync(buffer, 0, buffer.Length);

                    if (response.StatusCode == 200)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("status 200/OK");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"status {response.StatusCode}");
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {e.Message}");
                }
                finally
                {
                    output.Close();
                    listener.Stop();
                }


            }
        }

        private static async Task<bool> Send(string phone, string body)
        {
            return true;
        }

        private static async Task<Tuple<List<SMS>, bool>> ReadAll()
        {
            using (SerialPort serialPort = new SerialPort())
            {
                try
                {
                    List<SMS> smsList = new List<SMS>();
                    string portNo = GSMPort;
                    serialPort.PortName = portNo;
                    serialPort.BaudRate = 9600;
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                    }
                    serialPort.WriteLine("AT+CSCS=\"UCS2\"");
                    await Task.Delay(2000);
                    string output = "";
                    serialPort.WriteLine("AT" + System.Environment.NewLine);
                    await Task.Delay(2000);
                    serialPort.WriteLine("AT+CMGF=1\r" + System.Environment.NewLine);
                    await Task.Delay(2000);
                    serialPort.WriteLine("AT+CMGL=\"ALL\"" + System.Environment.NewLine);
                    await Task.Delay(5000);
                    output = serialPort.ReadExisting();
                    string[] receivedMessages = null;
                    receivedMessages = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    for (int i = 0; i < receivedMessages.Length - 1; i++)
                    {
                        if (receivedMessages[i].StartsWith("+CMGL"))
                        {
                            string[] message = receivedMessages[i].Split(',');

                            SMS sms = new SMS
                            {
                                Index = int.Parse(message[0].Substring(message[0].IndexOf(':') + 1)),
                                Status = message[1].Replace("\"", string.Empty) == "REC READ" ? "Read" : "UnRead",
                                Phone = GSMUtils.Translate(message[2].Replace("\"", string.Empty)),
                                Date = "20" + message[4].Replace("\"", string.Empty),
                                Time = message[5].Replace("\"", string.Empty),
                                Body = GSMUtils.Translate(receivedMessages[i + 1])
                            };

                            smsList.Add(sms);
                        }
                    }
                    return Tuple.Create(smsList, true);
                }
                catch (Exception)
                {
                    return Tuple.Create(new List<SMS>(), false);
                }
                finally
                {
                    serialPort.Close();
                }
            }
        }

        private static async Task<Tuple<List<SMS>, bool>> ReadAllPhone(string phone)
        {
            using (SerialPort serialPort = new SerialPort())
            {
                try
                {
                    string fixedPhone = null;
                    if (phone.Trim().StartsWith("98"))
                    {
                        fixedPhone = "+" + phone.Trim();
                    }
                    else if (phone.Trim().StartsWith("0"))
                    {
                        fixedPhone = "+98" + (phone.Trim().Substring(1));
                    }

                    List<SMS> smsList = new List<SMS>();
                    string portNo = GSMPort;
                    serialPort.PortName = portNo;
                    serialPort.BaudRate = 9600;
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                    }
                    serialPort.WriteLine("AT+CSCS=\"UCS2\"");
                    await Task.Delay(2000);
                    string output = "";
                    serialPort.WriteLine("AT" + System.Environment.NewLine);
                    await Task.Delay(2000);
                    serialPort.WriteLine("AT+CMGF=1\r" + System.Environment.NewLine);
                    await Task.Delay(2000);
                    serialPort.WriteLine("AT+CMGL=\"ALL\"" + System.Environment.NewLine);
                    await Task.Delay(5000);
                    output = serialPort.ReadExisting();
                    string[] receivedMessages = null;
                    receivedMessages = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    for (int i = 0; i < receivedMessages.Length - 1; i++)
                    {
                        if (receivedMessages[i].StartsWith("+CMGL"))
                        {
                            string[] message = receivedMessages[i].Split(',');

                            SMS sms = new SMS
                            {
                                Index = int.Parse(message[0].Substring(message[0].IndexOf(':') + 1)),
                                Status = message[1].Replace("\"", string.Empty) == "REC READ" ? "Read" : "UnRead",
                                Phone = GSMUtils.Translate(message[2].Replace("\"", string.Empty)),
                                Date = "20" + message[4].Replace("\"", string.Empty),
                                Time = message[5].Replace("\"", string.Empty),
                                Body = GSMUtils.Translate(receivedMessages[i + 1])
                            };

                            if (sms.Phone == fixedPhone)
                            {
                                smsList.Add(sms);
                            }
                        }
                    }
                    return Tuple.Create(smsList, true);
                }
                catch (Exception)
                {
                    return Tuple.Create(new List<SMS>(), false);
                }
                finally
                {
                    serialPort.Close();
                }
            }
        }

        private static async Task<Tuple<SMS, bool>> Read(string index)
        {
            using (SerialPort serialPort = new SerialPort())
            {
                try
                {
                    SMS smsResult = new SMS();
                    string portNo = GSMPort;
                    serialPort.PortName = portNo;
                    serialPort.BaudRate = 9600;
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                    }
                    serialPort.WriteLine("AT+CSCS=\"UCS2\"");
                    await Task.Delay(2000);
                    string output = "";
                    serialPort.WriteLine("AT" + System.Environment.NewLine);
                    await Task.Delay(2000);
                    serialPort.WriteLine("AT+CMGF=1\r" + System.Environment.NewLine);
                    await Task.Delay(2000);
                    serialPort.WriteLine($"AT+CMGR={index}" + System.Environment.NewLine);
                    await Task.Delay(5000);
                    output = serialPort.ReadExisting();
                    string[] receivedMessages = null;
                    receivedMessages = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    for (int i = 0; i < receivedMessages.Length - 1; i++)
                    {
                        if (receivedMessages[i].StartsWith("+CMGR"))
                        {
                            string[] message = receivedMessages[i].Split(',');

                            SMS sms = new SMS
                            {
                                Index = int.Parse(index),
                                Status = message[0].Contains("REC READ") ? "Read" : "UnRead",
                                Phone = GSMUtils.Translate(message[1].Replace("\"", string.Empty)),
                                Date = "20" + message[3].Replace("\"", string.Empty),
                                Time = message[4].Replace("\"", string.Empty),
                                Body = GSMUtils.Translate(receivedMessages[i + 1])
                            };

                            smsResult = sms;

                            break;
                        }
                        else
                        {
                            smsResult = null;
                        }
                    }
                    return Tuple.Create(smsResult, true);
                }
                catch (Exception)
                {
                    return Tuple.Create(new SMS(), false);
                }
                finally
                {
                    serialPort.Close();
                }
            }
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
