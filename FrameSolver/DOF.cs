using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    [Flags]
    public enum DOF
    {
        Free = 0,
        UX = 1,
        UZ = 2,
        RY = 4,
        Pinned = UX | UZ,
        Fixed = UX | UZ | RY,
    }
}
