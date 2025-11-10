using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManger.Core.Models.Interfaces
{
    public interface IArchivable
    {
        bool IsArchived { get; set; }
    }
}
