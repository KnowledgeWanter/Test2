using UnityEngine;
using System;
using System.Net;
using System.Collections;

namespace Zone.AB
{
    public static class WebRequestExtensions
    {
        public static WebResponse BetterEndGetResponse(this WebRequest request, IAsyncResult asyncResult)
        {
            try
            {
                return request.EndGetResponse(asyncResult);
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    return wex.Response;
                }
                throw;
            }
        }

        public static WebResponse BetterGetResponse(this WebRequest request)
        {
            try
            {
                return request.GetResponse();
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    return wex.Response;
                }
                throw;
            }
        }
    }
}