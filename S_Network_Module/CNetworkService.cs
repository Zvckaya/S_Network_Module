using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace S_Network_Module
{
    public class CNetworkService
    {

        int max_connections;

        CListener client_listenr;

        //메세지 수신,전송 시 사용함
        SocketAsyncEventArgsPool receive_event_args_pool;
        SocketAsyncEventArgsPool send_event_args_pool;

        BufferManager buffer_manager;

        //클라이언트 접속시 호출되는 delegate
        public delegate void SessionHandler(CUserToken token);
        public SessionHandler session_created_call_back { get; set; }
        
        public void initailize()
        {
            this.receive_event_args_pool = new SocketAsyncEventArgsPool(this.max_connections);
            this.send_event_args_pool = new SocketAsyncEventArgsPool(this.max_connections);

            SocketAsyncEventArgs args;

            for(int i=0; i< this.max_connections; i++)
            {
                CUserToken token = new CUserToken();

                //커넥션마다 풀이 있어야함
                {
                    //args = new SocketAsyncEventArgs();
                    //args.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
                    //args.UserToken = token;

                    
                }
            }
        }

        public void listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callback_on_newclient += on_new_client;
            listener.start(host, port, backlog);
        }

        public void on_new_client(Socket client_socket, object token)
        {

        }
    }
}
