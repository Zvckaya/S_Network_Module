using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace S_Network_Module
{
    public class CUserToken
    {
        public Socket socket;

        public void set_event_args(SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
        {
        }

        internal void on_receive(byte[]? buffer, int offset, int bytesTransferred)
        {
            throw new NotImplementedException();
        }
    }
}
