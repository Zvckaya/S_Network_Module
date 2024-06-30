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
        public Socket socket { get; set; }

        public SocketAsyncEventArgs receive_event_args { get; private set; }
        public SocketAsyncEventArgs send_event_args { get; private set; }


        Queue<CPacket> sending_queue;
        private object cs_sending_queue;

        public void set_event_args(SocketAsyncEventArgs receive_args, SocketAsyncEventArgs send_args)
        {
        }

        public void on_receive(byte[]? buffer, int offset, int bytesTransferred)
        {

        }

        public void send(CPacket msg)
        {
            CPacket clone = new CPacket();
            msg.copy_to(clone);

            lock (this.cs_sending_queue)
            {
                if (this.sending_queue.Count <= 0)
                {
                    this.sending_queue.Enqueue(msg);
                    start_send();
                    return;
                }

                this.sending_queue.Enqueue(msg);
            }

        }

        private void start_send()
        {
            lock (this.cs_sending_queue)
            {
                CPacket msg = this.sending_queue.Peek();

                msg.record_size();

                this.send_event_args.SetBuffer(this.send_event_args.Offset, msg.position);

                Array.Copy(msg.buffer, 0, this.send_event_args.Buffer, this.send_event_args.Offset, msg.position);

                bool pending = this.socket.SendAsync(this.send_event_args);
                if (!pending)
                {
                    process_send(this.send_event_args);
                }


            }
        }

        private void process_send(SocketAsyncEventArgs send_event_args)
        {
            lock (this.cs_sending_queue)
            {
                this.sending_queue.Dequeue();

                if (this.sending_queue.Count > 0)
                {
                    start_send();
                }
            }

        }
    }
}
