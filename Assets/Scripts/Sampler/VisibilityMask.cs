using System;
using System.Collections.Generic;

public class VisibilityMask
{
    private readonly List<ushort> bitMaskList = new List<ushort>();
    const int bitCount = 16;
    public readonly int length;
    public int weight { get; private set; }

    public VisibilityMask(int len)
    {
        length = len;
        GenerateBitMask();
    }

    public bool this[int key]
    {

        get
        {
            if (key < length)
            {
                var sect = bitMaskList[key / bitCount];
                return Convert.ToBoolean((sect >> (key % bitCount)) & 0x01);
            }
            else
            {
                throw new Exception("VisibilityMask out of index.");
            }
        }
        set
        {
            if (key < length)
            {
                var sectIdx = key / bitCount;
                ushort mask = (ushort)(0x01 << (key % bitCount));
                if (value)
                {
                    bitMaskList[sectIdx] |= mask;
                }
                else
                {
                    mask = (ushort)~mask;
                    bitMaskList[sectIdx] &= mask;
                }

                var w = 0;
                foreach (var m in bitMaskList)
                {
                    w += HammingWeight(m);
                }
                weight = w;
            }
            else
            {
                throw new Exception("VisibilityMask out of index.");
            }
        }
    }

    private void GenerateBitMask()
    {
        if (length < 0)
        {
            bitMaskList.Clear();
            return;
        }

        var size = length / bitCount;
        if (length % bitCount > 0)
        {
            size += 1;
        }

        for (int i = 0; i < size; i++)
        {
            bitMaskList.Add(0);
        }
    }

    private int HammingWeight(ushort value)
    {
        int sum = 0;

        while (value > 0)
        {
            sum += value & 0x01;
            value >>= 1;
        }

        return sum;
    }
}
