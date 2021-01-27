using System;
using System.Collections.Generic;
using Tsubasa.Online.Http;

namespace Tsubasa.Online
{
    public class RequestManager : IDisposable
    {
        /// <summary>
        /// A dictionary of requests, bound to a url as it's Key, and a Queue as the value.
        /// </summary>
        public static IDictionary<string, RequestStream> Requests = new Dictionary<string, RequestStream>();

        public RequestManager()
        {

        }

        /// <summary>
        /// Enqueues a request.
        /// </summary>
        /// <param name="request">The request to queue up.</param>
        public void Enqueue(TsubasaWebRequest request)
        {
            /*RequestStream queue;
            if (Requests.TryGetValue(request.Url, out queue))
            {
                queue.Enqueue(request);
                return;
            }

            queue = new RequestStream();*/
        }

        #region IDisposable Implementation
        public void Dispose()
        {
            // Abort all un-aborted requests
            AbortAll();

            // Clear Requests Dictionary
            Requests.Clear();
        }

        /// <summary>
        /// Aborts all requests, if they aren't already.
        /// </summary>
        public void AbortAll()
        {
            foreach (var entry in Requests)
            {
                entry.Value.AbortAll();
            }
        }
        /// <summary>
        /// Aborts all requests under a specific URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void AbortAll(string url)
        {
            RequestStream queue;
            if (Requests.TryGetValue(url, out queue))
            {
                queue.AbortAll();
            }
        }
        #endregion
    }
}