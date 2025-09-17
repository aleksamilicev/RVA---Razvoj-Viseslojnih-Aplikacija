using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Enums
{
    public enum RaftingState
    {
        Planned = 0,
        Boarding = 1,
        Paddling = 2,
        Resting = 3,
        Finished = 4
    }
}
