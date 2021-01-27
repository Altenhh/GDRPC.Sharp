using System;
using System.Collections.Generic;
using Tsubasa.Online.Http;

namespace Tsubasa.Online
{
    public class RequestStream : IDisposable
    {
        public Queue<TsubasaWebRequest> Queue = new Queue<TsubasaWebRequest>();
        public bool Busy => false;

        public RequestStream()
        {

        }

        /// <summary>
        /// Enqueues a request.
        /// </summary>
        /// <param name="request">The request to queue up.</param>
        public void Enqueue(TsubasaWebRequest request)
        {
            Queue.Enqueue(request);
        }

        #region IDisposable Implementation
        public void Dispose()
        {
            // Abort all un-aborted requests
            AbortAll();

            // Clear Queue
            Queue.Clear();
        }

        /// <summary>
        /// Aborts all requests, if they aren't already.
        /// </summary>
        public void AbortAll()
        {
            /*foreach (var request in Queue)
            {
                request.Abort();
            }*/

            return;
        }
        #endregion
    }
}