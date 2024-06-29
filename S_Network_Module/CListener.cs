using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace S_Network_Module
{
    class CListener
    {
        SocketAsyncEventArgs accept_args; // 비동기 accept을 eventargs

        Socket listen_socekt;

        //accept 처리 순서를 제어하기 위한 
        AutoResetEvent flow_control_event; //스레드 동기화

        //새로운 클라이언트가 접속했을 떄 호출
        public delegate void NewclientHandler(Socket client_socket, object tokent); //oncompletedAccept <--이거임
        public NewclientHandler callback_on_newclient;

        public CListener()
        {
            this.callback_on_newclient = null;
        }

        public void start(string host, int port, int backlog)
        {
            this.listen_socekt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress address;
            if (host == "0.0.0.0")
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(host);
            }
            IPEndPoint endpoint = new IPEndPoint(address, port);

            try
            {
                this.listen_socekt.Bind(endpoint);
                this.listen_socekt.Listen(backlog);

                this.accept_args = new SocketAsyncEventArgs();
                this.accept_args.Completed += new EventHandler<SocketAsyncEventArgs>(on_accept_completed);

                this.listen_socekt.AcceptAsync(this.accept_args); // 이렇게 처리할 시 입력을 위한 대기상테에서 accept 불가능 버그
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public void do_listen()
        {
            this.flow_control_event = new AutoResetEvent(false);

            while (true)
            {
                this.accept_args.AcceptSocket = null;

                bool pending = true;
                try
                {
                    pending = listen_socekt.AcceptAsync(this.accept_args); //일단 시도 성공시 true
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    continue;//계속 감지해야함
                }

                if (!pending)//대기가 없으면
                {
                    on_accept_completed(null, this.accept_args);
                }

                this.flow_control_event.WaitOne(); // 
            }

        }

        private void on_accept_completed(object value, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //새로 생긴 소켓을 보관해 놓음
                Socket client_socket = e.AcceptSocket;

                this.flow_control_event.Set();

                if(this.callback_on_newclient != null)
                {
                    this.callback_on_newclient(client_socket, e.UserToken);
                }
                return;
            }
            else
            {
                Console.WriteLine("Accept client failed");
            }

            this.flow_control_event.Set();
        }
    }
}
