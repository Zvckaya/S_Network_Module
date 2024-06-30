using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace S_Network_Module
{
    class Defines
    {
        public static readonly short HEADERSIZE = 2; 
        // 임의의 헤더 사이즈
    }

    class CMessageResolver
    {
        public delegate void CompletedMessageCallback(byte[] buffer);

        //메세지 사이즈
        int message_size;

        //사용하는 버퍼
        byte[] message_buffer = new byte[1024];

        //버퍼의 인덱스를 가리키는 변수 
        //패킷 하나를 완성한후에 0으로 초기화
        int current_position;

        //읽어와야 할 목표 위치
        int positipon_to_read;

        //남은 사이즈
        int remain_byte;

        public CMessageResolver()
        {
            this.message_size = 0;
            this.current_position = 0;
            this.remain_byte = 0;
            this.positipon_to_read = 0;
        }

        public void on_recieve(byte[] buffer,int offset,int transffered, CompletedMessageCallback callback) //데이터 수신시마다 호출
        {
            this.remain_byte = transffered; // 실제 데이터 크기

            //원본 버퍼의 포지션 값 
            int src_position = offset;


            //남은 데이터가 있으면 
            while (this.remain_byte > 0)
            {
                bool completed = false;

                //헤더만큼 못읽었을 경우 헤더 먼저 읽기 
                if(this.current_position < Defines.HEADERSIZE)
                {
                    //헤더만 읽기
                    this.positipon_to_read = Defines.HEADERSIZE;

                    completed = read_until(buffer, ref src_position, offset, transffered);
                    if(completed == false)
                    {
                        //다 못읽었으므로 다음 수신을 기다림 
                        return;
                    }
                    //헤더를 읽었으면 실제 메세지 사이즈를 구함.
                    this.message_size = get_body_size();

                    //다음 목표 지점 설정 -> 헤더사이즈와 메세지사이즈만큼 더해준다.
                    this.positipon_to_read = this.message_size + Defines.HEADERSIZE;
                }

                //메세지를 다 읽었는가?
                completed = read_until(buffer,ref src_position, offset, transffered);

                if (completed)
                {
                    //패킷 완성
                    callback(message_buffer);

                    //버퍼 지우기
                    clear_buffer();
                }

            }
        }

        private void clear_buffer()
        {
            Array.Clear(this.message_buffer, 0, this.message_buffer.Length);
            this.current_position = 0;
            this.message_size = 0;
        }

        private int get_body_size()
        {
            //헤더에서 메세지 사이즈를 구함 
            Type type = Defines.HEADERSIZE.GetType();
            if (type.Equals(typeof(Int16)))
            {
                return BitConverter.ToInt16(this.message_buffer, 0);
            }

            return BitConverter.ToInt32(this.message_buffer, 0);
        }

        private bool read_until(byte[] buffer, ref int src_position, int offset, int transsffered) //소켓 버퍼를 유저 객체의 패킷 버퍼로 복사
        {
            if(this.current_position >= offset + transsffered)
            {
                //들어온 데이터만큼 완료, 
                return false;
            }

            //읽어와야할 바이트
            //데이터가 분리되어 올 경우 이전에 읽논 값을 빼줌(부족한 만큼 읽어올 수 있게)
            int copy_size = this.positipon_to_read - this.current_position;

            if(this.remain_byte < copy_size)
            {
                copy_size = this.remain_byte;
            }

            Array.Copy(buffer, src_position, this.message_buffer, this.current_position, copy_size);

            src_position += copy_size;

            this.current_position += copy_size;

            this.remain_byte -= copy_size;

            if(this.current_position < this.positipon_to_read)
            {
                return false;
            }
            return true;
        }
    }
}
