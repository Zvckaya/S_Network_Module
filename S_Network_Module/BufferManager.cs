﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace S_Network_Module
{
    internal class BufferManager
    {

        int m_numByte;
        byte[] m_buffer;
        Stack<byte> m_freeIndexPool;
        int m_currentIndex;
        int m_bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numByte = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<byte>();
        }

         public void InitBuffer()
        {
            m_buffer = new byte[m_bufferSize]; //버퍼 사이즈 만큼의 배열 생성
        }

        public bool SetBuffer(SocketAsyncEventArgs args)  // args 에 버퍼 설정, index를 조정하여 다음 버퍼 위치를 가르킬 수 있음 .
        {
            if(m_freeIndexPool.Count> 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if((m_numByte - m_bufferSize) < m_currentIndex) // 남은 공간이 필요한 버퍼 크기보다 작을떄 
                {
                    return false; 
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;

            }
            return true;
        }

        public void FreeBuffer(SocketAsyncEventArgs args) //사용하지 않는 버퍼 반환 
        {
            m_freeIndexPool.Push((byte)args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
