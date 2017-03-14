using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace ObservationHub
{
    public class MyHub1 : Hub
    {
        /// <summary>
        /// クライアントから受信した文字列を全クライアントにブロードキャストする
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            Clients.All.recieveMessage(message);
        }
    }
}