using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;

namespace CapNhatDetail
{
    public class CapNhatDetail : ICData  
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        GridView gvDetail;


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
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            // đẩy dữ liệu vào detail
            if (drMaster.RowState == DataRowState.Modified)
            {
                db.UpdateByNonQuery(string.Format(" DELETE FROM DTTHUPHI WHERE MTTPID = '{0}' ",drMaster["MTTPID"].ToString()));
            }
            data.DsData.Tables[1].Clear();
            using (DataTable dt = db.GetDataTable(@"SELECT  MaNV, TK1, TKDU1 
                                                    FROM    DMNV 
                                                    WHERE   MaNV IN('THUPHI','THUQTK')"))
            {
                DataRow row;
                foreach (DataRow _row in dt.Rows)
                {
                    switch (_row["MaNV"].ToString().ToUpper())
                    {
                        case "THUPHI":
                            row = data.DsData.Tables[1].NewRow();
                            row["MTTPID"] = drMaster["MTTPID"];
                            row["MaNV"] = "THUPHI";
                            row["TKDU"] = _row["TKDU1"].ToString();
                            row["SoTien"] = drMaster["TienPhi"];
                            data.DsData.Tables[1].Rows.Add(row);
                            break;
                        case "THUQTK":
                            row = data.DsData.Tables[1].NewRow();
                            row["MTTPID"] = drMaster["MTTPID"];
                            row["MaNV"] = "THUQTK";
                            row["TKDU"] = _row["TKDU1"].ToString();
                            row["SoTien"] = (decimal)drMaster["TienBB"] + (decimal)drMaster["TienTN"];
                            if ((decimal)row["SoTien"] != 0)
                                data.DsData.Tables[1].Rows.Add(row);
                            break;
                    }
                }
            }
        }

        public InfoCustomData Info
        {
            get { return info ; }
        }

        #endregion
    }
}
