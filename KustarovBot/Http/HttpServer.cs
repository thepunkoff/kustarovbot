using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace KustarovBot.Http
{
    public class HttpServer : IAsyncDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string Http = "http";

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

            Logger.Info($"[{Http}] started listening http requests on port {_port}");
        }

        private async Task ServerWorker()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var context = await _httpListener.GetContextAsync();
                    var request = context.Request;

                    if (request.HttpMethod != "GET")
                    {
                        await ReturnBadRequest(context.Response);
                        continue;
                    }

                    if (request.Url is null)
                    {
                        Logger.Warn($"[{Http}] incoming request url was null.");
                        continue;
                    }

                    // ToDo: Вынести.
                    switch (request.Url.AbsolutePath)
                    {
                        case "/status":
                        {
                            Logger.Trace($"[{Http}] got status request from {request.RemoteEndPoint}");
                            var response = context.Response;
                            response.StatusCode = 200;
                            await response.WriteString("ok");
                            response.OutputStream.Close();
                            Logger.Trace($"[{Http}] responded ok");
                            break;
                        }
                        case "/iambusy/changeText":
                        {
                            Logger.Trace($"[{Http}] got changeText request from {request.RemoteEndPoint}");
                            
                            var queryString = request.QueryString;
                            if (queryString.Keys.Count is 0 or > 1)
                            {
                                await ReturnBadRequest(context.Response, "Query string should contain only one key 'text'.");
                                continue;
                            }

                            if (queryString.Keys[0] != "text")
                            {
                                await ReturnBadRequest(context.Response, "Query string should contain only one key 'text'.");
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
                            Logger.Trace($"[{Http}] changed iambusy module response text to '{value}'");
                            context.Response.Close();

                            continue;
                        }
                        case "/iambusy/changeSchedule":
                        {
                            Logger.Trace($"[{Http}] got chengeSchedule request from {request.RemoteEndPoint}");
                            
                            var state = Resources.LoadState();
                            var queryString = request.QueryString;
                            if (queryString.Count == 0)
                                await ReturnBadRequest(context.Response, "Query string didn't contain any keys.");

                            var notNullValueDic = new Dictionary<string, string>();
                            foreach (var key in queryString.AllKeys)
                            {
                                if (key is null)
                                {
                                    Logger.Warn($"[{Http}] found a null key in query string.");
                                    continue;
                                }

                                if (queryString[key] is null)
                                {
                                    Logger.Warn($"[{Http}] query string value for key '{key}' was null.");
                                    continue;
                                }

                                notNullValueDic.Add(key, queryString[key]);
                            }

                            state = notNullValueDic.Keys.Aggregate(state, (current, key) => key switch
                            {
                                "monday" => current with { Monday = bool.Parse(notNullValueDic[key]) },
                                "tuesday" => current with { Tuesday = bool.Parse(notNullValueDic[key]) },
                                "wednesday" => current with { Wednesday = bool.Parse(notNullValueDic[key]) },
                                "thursday" => current with { Thursday = bool.Parse(notNullValueDic[key]) },
                                "friday" => current with { Friday = bool.Parse(notNullValueDic[key]) },
                                "saturday" => current with { Saturday = bool.Parse(notNullValueDic[key]) },
                                "sunday" => current with { Sunday = bool.Parse(notNullValueDic[key]) },
                                _ => current
                            });

                            await Resources.SaveState(state);
                            Logger.Trace($"[{Http}] saved new state.");
                            context.Response.StatusCode = 200;
                            context.Response.Close();
                            
                            continue;
                        }
                        case "/iambusy/getSchedule":
                        {
                            Logger.Trace($"[{Http}] got getSchedule request from {request.RemoteEndPoint}");
                            
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
                            Logger.Trace($"[{Http}] raw url was '{request.RawUrl}'. responding 400 bad request.");
                            await ReturnBadRequest(context.Response);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[{Http}] {nameof(ServerWorker)} crashed:\n{ex}");
            }
        }

        private static async Task ReturnBadRequest(HttpListenerResponse response, string message = "404 bad request")
        {
            response.StatusCode = 400;
            await response.WriteString(message);
            response.OutputStream.Close();
            response.Close();
        }

        public async ValueTask DisposeAsync()
        {
            await _serverWorker;
            _cts.Cancel();
            _cts.Dispose();
            _httpListener.Stop();
            Logger.Trace($"[{Http}] stopped http server on port 8080.");
        }
    }
}