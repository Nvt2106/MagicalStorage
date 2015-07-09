using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// Define interface for all proxy entities.
    /// </summary>
    public interface IMSEntity
    {
        /// <summary>
        /// Primary key for entity.
        /// </summary>
        [Required]
        Guid EntityId { get; set; }

        /// <summary>
        /// Reference to entity context.
        /// </summary>
        [NotStore]
        MSEntityContext EntityContext { get; set; }
    }
}
