using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
                    args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(receive_completed);
                    args.UserToken = token;

                    this.buffer_manager.SetBuffer(args);

                    this.receive_event_args_pool.Push(args);
                }

                {
                    args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(send_completed);
                    args.UserToken = token;

                    this.buffer_manager.SetBuffer(args);

                    this.send_event_args_pool.Push(args);
                }
            }
        }

        private void send_completed(object? sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void receive_completed(object? sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void listen(string host, int port, int backlog)
        {
            CListener listener = new CListener();
            listener.callback_on_newclient += on_new_client;
            listener.start(host, port, backlog);
        }

        public void on_new_client(Socket client_socket, object token)
        {
            SocketAsyncEventArgs receive_args = this.receive_event_args_pool.Pop();
            SocketAsyncEventArgs send_args = this.send_event_args_pool.Pop();

            if(this.session_created_call_back != null)
            {
                CUserToken user_token = receive_args.UserToken as CUserToken;
                this.session_created_call_back(user_token);
            }

            begin_recive(client_socket, receive_args, send_args);
        }

        void begin_recive(Socket client_socket, SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
        {
            CUserToken token = receive_args.UserToken as CUserToken;
            token.set_event_args(receive_args, send_args);

            //생성된 클라이언트 소켓 보관
            token.socket = client_socket;

            bool pending = client_socket.ReceiveAsync(receive_args);
            if (pending == false)
            {
                process_recive(receive_args);
            }
        }

        private void process_recive(SocketAsyncEventArgs e)
        {
            //호스트가 연결을 끊었는지 확인한다.    
            CUserToken token = e.UserToken as CUserToken;

            if(e.BytesTransferred >0 && e.SocketError == SocketError.Success)
            {
                
                token.on_receive(e.Buffer, e.Offset, e.BytesTransferred);
                // 데이터가 들어있는 buffer, 데이터의 시작위치, 수신된 데이터 바이트 수
                //일단 메세지 수신
                bool pending = token.socket.ReceiveAsync(e);
                if (!pending)
                {
                    process_recive(e);  //연속적으로 데이터를 받기 위함.
                }
            }
            else
            {
                Console.WriteLine($"{e.SocketError} , {e.BytesTransferred}");
                close_clientsocket(token);
            }
        }

        private void close_clientsocket(CUserToken? token)
        {
            throw new NotImplementedException();
        }
    }
}
