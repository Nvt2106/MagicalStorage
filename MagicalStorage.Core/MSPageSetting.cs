using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalStorage.Core
{
    public class MSPageSetting
    {
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public List<MSSortInfo> SortInfos { get; private set; }

        public MSPageSetting() : this(-1, 1) { }

        public MSPageSetting(int pageSize, int pageIndex)
        {
            if (pageSize == 0 || pageSize < -1)
            {
                throw new MSException("Param pageSize must be positive number or -1 (not paging)");
            }
            if (pageIndex <= 0)
            {
                throw new MSException("Param pageIndex must be positive number and starts from 1");
            }

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.SortInfos = new List<MSSortInfo>();
        }

        public void AddSortInfo(MSSortInfo sortInfo, bool throwExceptionIfNull = false)
        {
            if (sortInfo != null)
            {
                this.SortInfos.Add(sortInfo);
            }
            else if (throwExceptionIfNull)
            {
                MSParameterHelper.MakeSureInputParameterNotNull(sortInfo, "sortInfo");
            }
        }
    }
}
