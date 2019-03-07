using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    internal unsafe struct ChunkAllocator : IDisposable
    {
        private byte* m_FirstChunk;
        private byte* m_LastChunk;
        private int m_LastChunkUsedSize;
        private const int ms_ChunkSize = 64 * 1024;
        private const int ms_ChunkAlignment = 64;

        public void Dispose()
        {
            while (m_FirstChunk != null)
            {
                var nextChunk = ((byte**) m_FirstChunk)[0];
                UnsafeUtility.Free(m_FirstChunk, Allocator.Persistent);
                m_FirstChunk = nextChunk;
            }

            m_LastChunk = null;
        }

        public byte* Allocate(int size, int alignment)
        {
            // 计算当前已使用的空间对齐后的值，对齐的大小应当为2的幂，否则会有问题
            var alignedChunkSize = (m_LastChunkUsedSize + alignment - 1) & ~(alignment - 1);
            // 当前操作chunk剩余空间是否够，不够需要开辟新的chunk 
            if (m_LastChunk == null || size > ms_ChunkSize - alignedChunkSize)
            {
                // 按照标准的chunk容量和对齐大小开辟一块内存作为新的chunk
                var newChunk = (byte*) UnsafeUtility.Malloc(ms_ChunkSize, ms_ChunkAlignment, Allocator.Persistent);
                ((byte**) newChunk)[0] = null;
                // 判断是否为第一个chunk，不是第一个chunk的话将新的chunk地址存到前一个chunk第一个位置上(每个chunk都会预留第一个位置来存放后续chunk的指针)
                if (m_LastChunk != null)
                    ((byte**) m_LastChunk)[0] = newChunk;
                else
                    m_FirstChunk = newChunk;

                // 将新开辟的chunk作为当前操作的chunk
                m_LastChunk = newChunk;
                // 预留一个byte*指针的空间来存放后一个chunk的指针，64位系统一般为8字节
                m_LastChunkUsedSize = sizeof(byte*);
                alignedChunkSize = (m_LastChunkUsedSize + alignment - 1) & ~(alignment - 1);
            }
            // 返回对齐后可用的第一个位置的指针
            var ptr = m_LastChunk + alignedChunkSize;
            // 更新已用空间
            m_LastChunkUsedSize = alignedChunkSize + size;
            return ptr;
        }

        public byte* Construct(int size, int alignment, void* src)
        {
            var res = Allocate(size, alignment);
            UnsafeUtility.MemCpy(res, src, size);
            return res;
        }
    }
}
