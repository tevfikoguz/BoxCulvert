using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public abstract class SerializableItem
    {
        internal Guid ID { get; private set; }

        protected SerializableItem()
        {
            ID = Guid.NewGuid();
        }
    }
}
