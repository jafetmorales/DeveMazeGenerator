﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    /// <summary>
    /// Contains a position with a byte that describes how far in the maze this point is (to determine the color).
    /// Note: Struct really is faster then class
    /// </summary>
    public struct MazePointPos
    {
        public int X, Y;
        public byte RelativePos;


        public MazePointPos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            this.RelativePos = 0;
        }

        public MazePointPos(int X, int Y, byte RelativePos)
        {
            this.X = X;
            this.Y = Y;
            this.RelativePos = RelativePos;
        }

        public override string ToString()
        {
            return "MazePoint, X: " + X + ", Y: " + Y;
        }
    }
}
