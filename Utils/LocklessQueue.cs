using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteelSeries.GameSense
{
    class LocklessQueue<T>
    {
        private T[] m_data;
        private int m_length;
        private int m_readIndex;
        private int m_maxReadIndex;
        private int m_writeIndex;

        public LocklessQueue(int size)
        {
            m_data = new T[size];
            m_length = size;
            m_readIndex = 0;
            m_maxReadIndex = 0;
            m_writeIndex = 0;
        }

        private int index(int i)
        {
            return i % m_length;
        }

        public bool Enqueue(T obj)
        {
            int readIdx;
            int writeIdx;

            do
            {
                // TODO beware of overflow
                writeIdx = m_writeIndex;
                readIdx = m_readIndex;

                if (index(writeIdx + 1) == index(readIdx))
                {
                    return false;
                }
                // repeat if _writeIdx has mutated
            }
            while (Interlocked.CompareExchange(ref m_writeIndex, index(writeIdx + 1), writeIdx) != writeIdx);

            m_data[index(writeIdx)] = obj;

            // commit
            while (Interlocked.CompareExchange(ref m_maxReadIndex, index(writeIdx + 1), writeIdx) != writeIdx)
            {
                // TODO yield? naaah!
            }

            return true;
        }

        public bool TryDequeue(out T obj)
        {
            int maxReadIdx;
            int readIdx;

            while (true)
            {
                readIdx = m_readIndex;
                maxReadIdx = m_maxReadIndex;

                if (index(readIdx) == index(maxReadIdx))
                {
                    obj = default;
                    return false;
                }

                obj = m_data[index(readIdx)];

                if (Interlocked.CompareExchange(ref m_readIndex, (readIdx + 1), readIdx) != m_readIndex)
                {
                    break;
                }
            }
            return true;
        }
    }
}