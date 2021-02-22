using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KustarovBot.MessageProcessing;

namespace KustarovBot.Http
{
    public class HttpServer : IAsyncDisposable
    {
        private readonly int _port;
        private readonly HttpListener _httpListener = new();
        private readonly CancellationTokenSource _cts = new();
        private Task _serverWorker;


        public HttpServer(int port)
        {
            _port = port;
        }
        
        public void Start()
        {
            _httpListener.Prefixes.Add($"http://*:{_port}/");
            _httpListener.Start();

            _serverWorker = Task.Run(ServerWorker, _cts.Token);

            Console.WriteLine($"started listening http requests on port {_port}");
        }

        private async Task ServerWorker()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var context = await _httpListener.GetContextAsync();
                    var request = context.Request;

                    Console.WriteLine($"got request from {request.RemoteEndPoint}");

                    if (request.HttpMethod != "GET")
                    {
                        await ReturnBadRequest(context.Response);
                        continue;
                    }

                    switch (request.RawUrl)
                    {
                        case "/status":
                        {
                            var response = context.Response;
                            response.StatusCode = 200;
                            await response.WriteString("ok");
                            response.OutputStream.Close();
                            Console.WriteLine("responded ok");
                            break;
                        }
                        case "/iambusy/changeText":
                        {
                            
                            var queryString = request.QueryString;
                            Console.WriteLine($"query string was '{queryString}'");
                            Console.WriteLine($"GetValues(0) was '{queryString.GetValues(0)}'");
                            if (queryString.Keys.Count == 0 || queryString.Keys.Count > 1)
                            {
                                await ReturnBadRequest(context.Response);
                                continue;
                            }

                            if (queryString.Keys[0] != "text")
                            {
                                await ReturnBadRequest(context.Response);
                                continue;
                            }

                            var value = queryString.GetValues(0)?[0];
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                await ReturnBadRequest(context.Response);
                                continue;
                            }

                            context.Response.StatusCode = 200;
                            IAmBusyModule.ResponseText = value;
                            Console.WriteLine($"changed iambusy module response text to '{value}'");
                            
                            break;
                        } 
                        default:
                        {
                            Console.WriteLine($"raw url was '{request.RawUrl}'. responding 400 bad request");
                            await ReturnBadRequest(context.Response);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(ServerWorker)} crashed:\n{ex}");
                throw;
            }
        }

        private static async Task ReturnBadRequest(HttpListenerResponse response)
        {
            response.StatusCode = 400;
            await response.WriteString("400 bad request");
            response.OutputStream.Close();
        }

        public async ValueTask DisposeAsync()
        {
            await _serverWorker;
            _cts.Cancel();
            _cts.Dispose();
            _httpListener.Stop();
            Console.WriteLine("stopped http server on port 8080");
        }
    }
}