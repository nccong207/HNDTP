using System;
using System.Collections.Generic;
using System.Text;
using CDTLib;
using Plugins;
using System.Data;
using CDTDatabase;

namespace LuuTaoMaXP
{
    public class LuuTaoMaXP : ICData
    {
        private InfoCustomData info = new InfoCustomData(IDataType.Single);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        public DataCustomData Data
        {
            set { data = value ; }
        }

        public InfoCustomData Info
        {
            get { return info; }
        }

        public void ExecuteAfter()
        {
            
        }

        public void ExecuteBefore()
        {
            info.Result = true;

            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Added)
            {
                if (drMaster["BoPhan"] == DBNull.Value)
                {
                    info.Result = false;
                    return;
                }
                string sql = string.Format(@"DECLARE @BP VARCHAR(16)
                                        SET @BP = '{0}'
                                        SELECT	TOP 1 CAST(REPLACE(ID,@BP,'') AS INT) [STT]
                                        FROM	DMXaPhuong
                                        WHERE	BoPhan = @BP AND ISNUMERIC(REPLACE(ID,@BP,''))=1
                                        ORDER BY CAST(REPLACE(ID,@BP,'') AS INT) DESC", drMaster["BoPhan"]);
                string iKq = db.GetValue(sql).ToString();
                int stt = 0;
                if (!string.IsNullOrEmpty(iKq))
                    stt = int.Parse(iKq);

                stt += 1;
                drMaster["ID"] = drMaster["BoPhan"].ToString() + stt.ToString();
            }
        }

    }
}
