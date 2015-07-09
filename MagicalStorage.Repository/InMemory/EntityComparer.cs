using MagicalStorage.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Repository.InMemory
{
    class EntityComparer : IComparer
    {
        public List<MSSortInfo> SortInfos { get; private set; }

        public EntityComparer(List<MSSortInfo> sortInfos)
        {
            this.SortInfos = sortInfos;
        }

        public int Compare(object x, object y)
        {
            foreach (var sortInfo in this.SortInfos)
            {
                int cr = CompareByOnePropertyName(x, y, sortInfo.PropertyName);
                if (cr == 0)
                {
                    continue;
                }
                else
                {
                    // DESC
                    if (sortInfo.SortDirection == MSSortDirection.Desc)
                    {
                        return -cr;
                    }
                    // ASC
                    else
                    {
                        return cr;
                    }
                }
            }
            return 0;
        }

        private int CompareByOnePropertyName(object x, object y, string propertyName)
        {
            var valueX = MSPropertyHelper.GetValueOfPropertyWithName(propertyName, x);
            var valueY = MSPropertyHelper.GetValueOfPropertyWithName(propertyName, y);
            if (valueX == null)
            {
                if (valueY == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (valueY == null)
                {
                    return 1;
                }
                else
                {
                    if (valueX is string) // in case of string, then compare ignore case
                    {
                        string sX = (valueX as string);
                        string sY = (valueY as string);
                        return sX.ToLower().CompareTo(sY.ToLower());
                    }
                    else
                    {
                        IComparable icX = (valueX as IComparable);
                        IComparable icY = (valueY as IComparable);
                        return icX.CompareTo(icY);
                    }
                }
            }
        }
    }
}
