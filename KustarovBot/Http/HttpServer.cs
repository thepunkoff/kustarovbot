using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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

                    // ToDo: Вынести 
                    switch (request.Url.AbsolutePath)
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
                            await Resources.SaveState(Resources.LoadState() with {ReplyText = value});
                            Console.WriteLine($"changed iambusy module response text to '{value}'");
                            context.Response.Close();

                            continue;
                        }
                        case "/iambusy/changeSchedule":
                        {
                            var state = Resources.LoadState();
                            var queryString = request.QueryString;
                            state = queryString.AllKeys.Aggregate(state, (current, key) => key switch
                            {
                                "monday" => current with {Monday = bool.Parse(queryString[key])},
                                "tuesday" => current with {Tuesday = bool.Parse(queryString[key])},
                                "wednesday" => current with {Wednesday = bool.Parse(queryString[key])},
                                "thursday" => current with {Thursday = bool.Parse(queryString[key])},
                                "friday" => current with {Friday = bool.Parse(queryString[key])},
                                "saturday" => current with {Saturday = bool.Parse(queryString[key])},
                                "sunday" => current with {Sunday = bool.Parse(queryString[key])},
                                _ => current
                            });

                            await Resources.SaveState(state);
                            context.Response.StatusCode = 200;
                            context.Response.Close();
                            
                            continue;
                        }
                        case "/iambusy/getSchedule":
                        {
                            var response = context.Response;
                            response.StatusCode = 200;
                            var state = Resources.LoadState();
                            await response.WriteString("{" +
                                                       $"\"Monday\": {state.Monday.ToString().ToLower()}, " +
                                                       $"\"Tuesday\": {state.Tuesday.ToString().ToLower()}, " +
                                                       $"\"Wednesday\": {state.Wednesday.ToString().ToLower()}, " +
                                                       $"\"Thursday\": {state.Thursday.ToString().ToLower()}, " +
                                                       $"\"Friday\": {state.Friday.ToString().ToLower()}, " +
                                                       $"\"Saturday\": {state.Saturday.ToString().ToLower()}, " +
                                                       $"\"Sunday\": {state.Sunday.ToString().ToLower()} " +
                                                       "}");
                            response.OutputStream.Close();
                            response.Close();

                            continue;
                        }
                        default:
                        {
                            Console.WriteLine($"raw url was '{request.RawUrl}'. responding 400 bad request");
                            await ReturnBadRequest(context.Response);
                            continue;
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
            response.Close();
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