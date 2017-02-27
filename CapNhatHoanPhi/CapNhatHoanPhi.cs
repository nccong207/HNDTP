using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using Plugins;
using System.Data;

namespace CapNhatHoanPhi
{
    public class CapNhatHoanPhi:ICData
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        public DataCustomData Data
        {
            set { data = value; }
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
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;

            using (DataTable dt = db.GetDataTable(@"SELECT * FROM DMNV 
                                                    WHERE   MaNV IN('HOANQTK','HOANLQTK')"))
            {
                if (drMaster.RowState == DataRowState.Added)
                {
                    DataRow row;
                    foreach (DataRow _row in dt.Rows)
                    {
                        // Tiền tiết kiệm
                        if (_row["MaNV"].ToString().ToUpper() == "HOANQTK" && ((decimal)drMaster["TTienBB"] + (decimal)drMaster["TTienTN"]) > 0)
                        {
                            row = data.DsData.Tables[1].NewRow();
                            row["MTHPID"] = drMaster["MTHPID"];
                            row["MaNV"] = "HOANQTK";
                            row["TK"] = _row["TK1"];
                            row["Ps"] = (decimal)drMaster["TTienBB"] + (decimal)drMaster["TTienTN"];
                            row["GhiChu"] = "Hoàn trả tiền quỹ tiết kiệm";
                            data.DsData.Tables[1].Rows.Add(row);
                        }
                        // Tiền lãi tiết kiệm
                        if (_row["MaNV"].ToString().ToUpper() == "HOANLQTK" && ((decimal)drMaster["PsBB"] + (decimal)drMaster["PsTN"] > 0))
                        {
                            row = data.DsData.Tables[1].NewRow();
                            row["MTHPID"] = drMaster["MTHPID"];
                            row["MaNV"] = "HOANLQTK";
                            row["TK"] = _row["TK1"];
                            row["Ps"] = (decimal)drMaster["PsBB"] + (decimal)drMaster["PsTN"];
                            row["GhiChu"] = "Hoàn trả lãi quỹ tiết kiệm";
                            data.DsData.Tables[1].Rows.Add(row);
                        }
                    }
                }
                if (drMaster.RowState == DataRowState.Modified)
                {
                    foreach (DataRow row in data.DsData.Tables[1].Select(string.Format("MTHPID = '{0}'", drMaster["MTHPID"])))
                    {
                        switch (row["MaNV"].ToString().ToUpper())
                        {
                            case "HOANQTK":
                                row["Ps"] = (decimal)drMaster["TTienBB"] + (decimal)drMaster["TTienTN"];
                                break;

                            case "HOANLQTK":
                                row["Ps"] = (decimal)drMaster["PsBB"] + (decimal)drMaster["PsTN"];
                                break;
                        }
                    }
                }
            }
        }

    }
}
