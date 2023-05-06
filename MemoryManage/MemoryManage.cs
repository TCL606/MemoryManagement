using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryManage
{
    public class MemoryManager
    {
        public List<MemoryBlock> BlankList { get; set; } = new();

        public List<MemoryBlock> AllocateList { get; set; } = new();

        public int MemorySize { get; init; }

        public int StartAddr { get; init; }

        private Fit fit;
        public bool Allocate(int memorySize) => fit.Allocate(memorySize, BlankList, AllocateList, out _);
        public bool Allocate(int memorySize, out MemoryBlock? retBlock) => fit.Allocate(memorySize, BlankList, AllocateList, out retBlock);
        public bool Free(int startAddr) => fit.Free(startAddr, BlankList, AllocateList, out _);
        public bool Free(int startAddr, out MemoryBlock? retBlock) => fit.Free(startAddr, BlankList, AllocateList, out retBlock);

        public MemoryManager(int memorySize, int startAddr, Fit fit)
        {
            MemorySize = memorySize;
            StartAddr = startAddr;
            this.fit = fit;  
            BlankList.Add(new MemoryBlock(memorySize, startAddr));
        }

    }

    public class MemoryBlock
    {
        public int StartAddr { get; set; }

        public int MemorySize { get; set; }

        public MemoryBlock(int memorySize, int startAddr = -1)
        {
            MemorySize = memorySize;
            StartAddr = startAddr;
        }
    }
}
