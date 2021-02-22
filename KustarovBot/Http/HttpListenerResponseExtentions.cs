using System;
using System.Net;
using System.Threading.Tasks;

namespace KustarovBot.Http
{
    public static class HttpListenerResponseExtentions
    {
        public static async Task WriteString(this HttpListenerResponse response, string text)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            await output.WriteAsync(buffer.AsMemory(0, buffer.Length));
        }
    }
}