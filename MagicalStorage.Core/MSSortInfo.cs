using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    /// <summary>
    /// Store sorting information, including sort by which property name and which direction.
    /// </summary>
    public class MSSortInfo
    {
        public string PropertyName { get; private set; }
        public MSSortDirection SortDirection { get; private set; }

        public MSSortInfo(string propertyName, MSSortDirection sortDirection = MSSortDirection.Asc)
        {
            MSParameterHelper.MakeSureInputParameterNotNull(propertyName, "propertyName");
            if (!MSStringHelper.IsValidIdentifier(propertyName))
            {
                throw new MSException("Param propertyName must be a valid identifier");
            }

            this.PropertyName = propertyName;
            this.SortDirection = sortDirection;
        }
    }

    public enum MSSortDirection: int
    {
        Asc = 1,
        Desc = 2
    }
}
