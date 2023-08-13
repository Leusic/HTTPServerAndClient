//libraries
using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
public class locationserver
{
    static void Main(string[] args)
    {
        runServer();
    }

    static void runServer()
    {
        TcpListener listener;
        Socket connection;
        Handler requestHandler;
        try
        {
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start();
            Console.WriteLine("Server started Listening");

            //dictionary for storing users and their locations
            IDictionary<string, string> userData = new Dictionary<string, string>()
            {
                {"Max", "is being tested"}
            };

            //connection is given a server thread
            while (true)
            {
                connection = listener.AcceptSocket();
                requestHandler = new Handler();
                Console.WriteLine("Connection recieved");
                Thread theThread = new Thread(() => requestHandler.doRequest(connection, ref userData));
                theThread.Name = "Thread";
                theThread.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

class Handler
    {
        //this method is called when the server recieves a connection on a listener,
        //it processes requests and provides their relevant responses.
        public void doRequest(Socket connection, ref IDictionary<string, string> userData)
        {
            NetworkStream socketStream;
            socketStream = new NetworkStream(connection);
            socketStream.ReadTimeout = 1000;
            socketStream.WriteTimeout = 1000;
            try
            {
                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);
                string request = "";
                try
                {
                    //reads from client character by character and ends once the stream ends
                    while (sr.Peek() > -1)
                    {
                        request = request + (char)sr.Read();
                    }
                }
                catch (Exception e)
                {
                    sw.WriteLine("Loading error: " + e);
                }

                Console.WriteLine("Response Recieved: " + request);
                //splits response into two parts after the first space
                String[] splitLine = request.Split(" ", 2);
                string user;
                string location;
                string[] splitRequest;
                switch (splitLine[0])
                {
                    case "NOOP":
                        Console.WriteLine("NOOP performed, doing nothing");
                        break;
                    case "ECHO":
                        Console.WriteLine("ECHO performed, returning input: " + "\n" + splitLine[1]);
                        break;
                    case "GET":
                        if (splitLine[1].Contains("HTTP/1.0"))
                        {
                            //HTTP 1.0 GET request
                            user = splitLine[1].Split(" ")[0].Trim(new char[] { '/', '?' });
                            if (userData.ContainsKey(user))
                            {
                                Console.WriteLine(user + " is valid user");
                                sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + userData[user] + "\r\n");
                            }
                            else
                            {
                                Console.WriteLine("invalid user");
                                sw.Write("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n");
                            }
                        }
                        else if (splitLine[1].Contains("HTTP/1.1"))
                        {
                            //HTTP 1.1 GET request
                            user = splitLine[1].Split(" ")[0].Trim(new char[] { '/', '?' });
                            user = user.Remove(0, 5);
                            Console.WriteLine("user: " + user);
                            if (userData.ContainsKey(user))
                            {
                                Console.WriteLine(user + " is valid user");
                                sw.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + userData[user] + "\r\n");
                            }
                            else
                            {
                                Console.WriteLine("invalid user");
                                sw.Write("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n");
                            }
                        }
                        else
                        {
                            //HTTP 0.9 GET request
                            user = splitLine[1].Trim(new Char[] { '\r', '\n', '/' });
                            if (userData.ContainsKey(user))
                            {
                                Console.WriteLine(user + " is valid user");
                                sw.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + userData[user]);
                            }
                            else
                            {
                                Console.WriteLine("invalid user");
                                sw.Write("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            }
                        }
                        sw.Flush();
                        break;
                    case "PUT":
                        //HTTP 0.9 PUT request
                        splitRequest = splitLine[1].Split("\r\n".ToCharArray());
                        user = splitRequest[0].Trim(new Char[] { '\r', '\n', '/' });
                        location = splitRequest[4].Trim(new Char[] { '\r', '\n', '/' });
                        if (userData.ContainsKey(user))
                        {
                            sw.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            Console.WriteLine("location: " + location);
                            userData[user] = location;
                        }
                        else
                        {
                            userData.Add(user, location);
                            sw.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        }
                        sw.Flush();
                        break;
                    case "POST":
                        if (splitLine[1].Contains("HTTP/1.0"))
                        {
                            //HTTP 1.0 POST request
                            splitRequest = splitLine[1].Split("\r\n".ToCharArray());
                            user = splitLine[1].Split(" ")[0].Trim(new char[] { '/', '?' });
                            location = splitRequest[6];
                            if (userData.ContainsKey(user))
                            {
                                sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                Console.WriteLine("location: " + location);
                                userData[user] = location;
                            }
                            else
                            {
                                userData.Add(user, location);
                                sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            }

                        }
                        else if (splitLine[1].Contains("HTTP/1.1"))
                        {
                            //HTTP 1.1 POST request
                            splitRequest = splitLine[1].Split("\r\n".ToCharArray());
                            user = splitRequest[8].Split('&')[0].Remove(0, 5);
                            location = splitRequest[8].Split('&')[1].Remove(0, 9);
                            if (userData.ContainsKey(user))
                            {
                                sw.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                Console.WriteLine("location: " + location);
                                userData[user] = location;
                            }
                            else
                            {
                                userData.Add(user, location);
                                sw.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            }
                        }
                        sw.Flush();
                        break;
                    default:
                        //default whois request handling
                        Console.WriteLine(splitLine[0] + "\r\n");
                        if (userData.ContainsKey(splitLine[0])) {
                            Console.WriteLine(splitLine[0] + " is valid user");
                            if (splitLine.Length > 1)
                            {
                                Console.WriteLine("Updating user " + splitLine[0] + " to be in location: " + splitLine[1]);
                                userData[splitLine[0]] = splitLine[1];
                                sw.WriteLine("OK");
                            }
                            else
                            {
                                sw.WriteLine(userData[splitLine[0]]);
                            }
                            sw.Flush();
                        }
                        else
                        {
                            if (splitLine.Length > 1)
                            {
                                Console.WriteLine("Adding user " + splitLine[0] + " and setting their location to " + splitLine[1]);
                                userData.Add(splitLine[0], splitLine[1]);
                                sw.WriteLine("OK");
                            }
                            else
                            {
                                //command /s for spamming the client for testing
                                if (splitLine[0] == "/s")
                                {
                                    while (true)
                                    {
                                        sw.WriteLine("spam");
                                    }
                                }
                                sw.WriteLine("ERROR: no entries found");
                            }
                            sw.Flush();
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error detected" + e);
            }
            finally
            {
                socketStream.Close();
                connection.Close();
                Console.WriteLine("Connection closed");
            }
        }
    }
}