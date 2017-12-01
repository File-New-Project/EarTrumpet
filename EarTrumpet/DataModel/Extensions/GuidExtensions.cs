using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.Extensions
{
    public static class GuidExtensions
    {
        public static Guid FromLongFormat(uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h, uint i, uint j, uint k)
        {
            return new Guid((uint)a, (ushort)b, (ushort)c, (byte)d, (byte)e, (byte)f, (byte)g, (byte)h, (byte)i, (byte)j, (byte)k);
    
        }
    }
}
