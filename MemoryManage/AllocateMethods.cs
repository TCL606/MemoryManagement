using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MemoryManage
{
    public abstract class Fit
    {
        public abstract bool Allocate(int memorySize, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock);
        public abstract bool Free(int startAddr, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock);
    }

    public class FirstFit: Fit
    {
        public override bool Allocate(int memorySize, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            for (int i = 0; i < blankList.Count; i++)
            {
                if (blankList[i].MemorySize >= memorySize)
                {
                    var memoryBlock = new MemoryBlock(memorySize, blankList[i].StartAddr);
                    allocateList.Add(memoryBlock);
                    retBlock = memoryBlock;
                    blankList[i].MemorySize -= memoryBlock.MemorySize;
                    if (blankList[i].MemorySize == 0)
                    {
                        blankList.RemoveAt(i);
                    }
                    else
                    {
                        blankList[i].StartAddr += memoryBlock.MemorySize;
                    }
                    return true;
                }
            }
            retBlock = null;
            return false;
        }

        public override bool Free(int startAddr, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            retBlock = null;
            var freeIdx = allocateList.FindIndex(x => x.StartAddr == startAddr);
            if (freeIdx == -1)
                return false;
            var freeBlock = allocateList[freeIdx];
            retBlock = freeBlock;
            allocateList.RemoveAt(freeIdx);
            int index = blankList.FindIndex(item => item.StartAddr > freeBlock.StartAddr);
            if (index == -1)
                blankList.Insert(blankList.Count, freeBlock);
            else
                blankList.Insert(index, freeBlock);
            int i = 1;
            while (i < blankList.Count)
            {
                if (blankList[i - 1].StartAddr + blankList[i - 1].MemorySize == blankList[i].StartAddr)
                {
                    blankList[i - 1].MemorySize += blankList[i].MemorySize;
                    blankList.RemoveAt(i);
                    continue;
                }
                i++;
            }
            return true;
        }
    }
    
    public class NextFit: Fit
    {
        private int lastAllocIdx = 0;

        public override bool Allocate(int memorySize, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            for (int j = 0; j < blankList.Count; j++)
            {
                int i = (j + lastAllocIdx) % blankList.Count;
                if (blankList[i].MemorySize >= memorySize)
                {
                    var memoryBlock = new MemoryBlock(memorySize, blankList[i].StartAddr);
                    allocateList.Add(memoryBlock);
                    retBlock = memoryBlock;
                    lastAllocIdx = i;
                    blankList[i].MemorySize -= memoryBlock.MemorySize;
                    if (blankList[i].MemorySize == 0)
                    {
                        blankList.RemoveAt(i);
                    }
                    else
                    {
                        blankList[i].StartAddr += memoryBlock.MemorySize;
                    }
                    return true;
                }
            }
            retBlock = null;
            return false;
        }

        public override bool Free(int startAddr, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            retBlock = null;
            var freeIdx = allocateList.FindIndex(x => x.StartAddr == startAddr);
            if (freeIdx == -1)
                return false;
            var freeBlock = allocateList[freeIdx];
            retBlock = freeBlock;
            allocateList.RemoveAt(freeIdx);
            int index = blankList.FindIndex(item => item.StartAddr > freeBlock.StartAddr);
            if (index == -1)
                blankList.Insert(blankList.Count, freeBlock);
            else
                blankList.Insert(index, freeBlock);
            if (index <= lastAllocIdx)  // record the last allcate index of blank list
                lastAllocIdx++;
            int i = 1;
            while (i < blankList.Count)
            {
                if (blankList[i - 1].StartAddr + blankList[i - 1].MemorySize == blankList[i].StartAddr)
                {
                    blankList[i - 1].MemorySize += blankList[i].MemorySize;
                    blankList.RemoveAt(i);
                    if (i <= lastAllocIdx)  // lastAllocIdx-- when merging the blank blocks
                        lastAllocIdx--;
                    continue;
                }
                i++;
            }
            return true;
        }
    }

    public class BestFit: Fit
    {
        public override bool Allocate(int memorySize, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            retBlock = null;
            int idx = -1;
            for (int i = 0; i < blankList.Count; i++)
            {
                if (blankList[i].MemorySize >= memorySize)
                {
                    idx = i;
                    break;
                }
            }
            if (idx == -1)
                return false;

            var allocBlock = new MemoryBlock(memorySize, blankList[idx].StartAddr);
            allocateList.Add(allocBlock);
            retBlock = allocBlock;
            blankList[idx].MemorySize -= allocBlock.MemorySize;
            if (blankList[idx].MemorySize == 0)
            {
                blankList.RemoveAt(idx);
            }
            else
            {
                blankList[idx].StartAddr += allocBlock.MemorySize;
                blankList.Sort((x, y) => x.MemorySize.CompareTo(y.MemorySize));
            }
            return true;
        }

        public override bool Free(int startAddr, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            retBlock = null;
            var freeIdx = allocateList.FindIndex(x => x.StartAddr == startAddr);
            if (freeIdx == -1)
                return false;
            var freeBlock = allocateList[freeIdx];
            retBlock = freeBlock;
            allocateList.RemoveAt(freeIdx);
            blankList.Insert(blankList.Count, freeBlock);
            int i = 1;
            blankList.Sort((x, y) => x.StartAddr.CompareTo(y.StartAddr));
            while (i < blankList.Count)
            {
                if (blankList[i - 1].StartAddr + blankList[i - 1].MemorySize == blankList[i].StartAddr)
                {
                    blankList[i - 1].MemorySize += blankList[i].MemorySize;
                    blankList.RemoveAt(i);
                    continue;
                }
                i++;
            }
            blankList.Sort((x, y) => x.MemorySize.CompareTo(y.MemorySize));
            return true;
        }
    }

    public class WorstFit : Fit
    {
        public override bool Allocate(int memorySize, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            retBlock = null;
            int idx = blankList.Count - 1;
            if (idx == -1 || blankList[idx].MemorySize < memorySize)
                return false;
            var allocBlock = new MemoryBlock(memorySize, blankList[idx].StartAddr);
            allocateList.Add(allocBlock);
            retBlock = allocBlock;
            blankList[idx].MemorySize -= allocBlock.MemorySize;
            if (blankList[idx].MemorySize == 0)
            {
                blankList.RemoveAt(idx);
            }
            else
            {
                blankList[idx].StartAddr += allocBlock.MemorySize;
                blankList.Sort((x, y) => x.MemorySize.CompareTo(y.MemorySize));
            }
            return true;
        }

        public override bool Free(int startAddr, List<MemoryBlock> blankList, List<MemoryBlock> allocateList, out MemoryBlock? retBlock)
        {
            retBlock = null;
            var freeIdx = allocateList.FindIndex(x => x.StartAddr == startAddr);
            if (freeIdx == -1)
                return false;
            var freeBlock = allocateList[freeIdx];
            retBlock = freeBlock;
            allocateList.RemoveAt(freeIdx);
            blankList.Insert(blankList.Count, freeBlock);
            int i = 1;
            blankList.Sort((x, y) => x.StartAddr.CompareTo(y.StartAddr));
            while (i < blankList.Count)
            {
                if (blankList[i - 1].StartAddr + blankList[i - 1].MemorySize == blankList[i].StartAddr)
                {
                    blankList[i - 1].MemorySize += blankList[i].MemorySize;
                    blankList.RemoveAt(i);
                    continue;
                }
                i++;
            }
            blankList.Sort((x, y) => x.MemorySize.CompareTo(y.MemorySize));
            return true;
        }
    }
}
