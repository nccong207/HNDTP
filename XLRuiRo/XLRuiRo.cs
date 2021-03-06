using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;

namespace XLRuiRo
{
    public class XLRuiRo : ICData 
    {

        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        DataRow drMaster;

        #region ICData Members

        public DataCustomData Data
        {
            set { data = value ; }
        }

        public void ExecuteAfter()
        {
            
        }

        public void ExecuteBefore()
        {
            // cập nhật tiền vay khi được duyệt xử lý nợ

        }

        public InfoCustomData Info
        {
            get { return info; }
        }

        #endregion
    }
}
