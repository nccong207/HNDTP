using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using System.Data;
using DevExpress.XtraGrid.Views.Grid;

namespace GetDuAn
{
    public class GetDuAn : ICControl
    {
        #region ICControl Members

        private InfoCustomControl info = new InfoCustomControl(IDataType.SingleDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();

        DataRow drMaster;
        GridLookUpEdit NguoiVay;
        GridLookUpEdit NguoiTK;
        TextEdit SoCMND;
        TextEdit SoHK;
        TextEdit CMNDTK;
        TextEdit SoHKTK; 

        public void AddEvent()
        { 
            NguoiVay = data.FrmMain.Controls.Find("NguoiVay",true) [0] as GridLookUpEdit;
            NguoiTK = data.FrmMain.Controls.Find("NguoiTK", true)[0] as GridLookUpEdit;
            SoCMND = data.FrmMain.Controls.Find("SoCMND",true)[0] as TextEdit;
            SoHK = data.FrmMain.Controls.Find("SoHoKhau", true)[0] as TextEdit;
            CMNDTK = data.FrmMain.Controls.Find("CMNDTK", true)[0] as TextEdit;
            SoHKTK = data.FrmMain.Controls.Find("SoHKTK", true)[0] as TextEdit;
            NguoiTK.QueryCloseUp += new System.ComponentModel.CancelEventHandler(NguoiTK_QueryCloseUp);
            NguoiVay.QueryCloseUp += new System.ComponentModel.CancelEventHandler(NguoiVay_QueryCloseUp);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void NguoiTK_QueryCloseUp(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // giới hạn độ tuổi người thừa kế
            GridLookUpEdit grd = sender as GridLookUpEdit;
            DataRow dr = grd.Properties.View.GetDataRow(grd.Properties.View.FocusedRowHandle);
            if (grd.Properties.ReadOnly || dr == null)
                return;
            string query = string.Format(@"SELECT DATEDIFF(YEAR,NgaySinh,GETDATE()) Tuoi,GioiTinh 
                                           FROM   DMHoDan WHERE ID = '{0}'", dr["ID"]);
            using (DataTable dt = db.GetDataTable(query))
            {
                if (dt.Rows.Count == 0)
                    return;
                int tuoi = int.Parse(dt.Rows[0]["Tuoi"].ToString());
                if (dt.Rows[0]["GioiTinh"].ToString() == "Nam")
                {
                    if (tuoi < 18 || tuoi > 65)
                    {
                        e.Cancel = true;
                        XtraMessageBox.Show("Độ tuổi không hợp lệ\nNgười thừa kế là nam giới phải nằm trong độ tuổi từ 18 - 60 tuổi ", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (dt.Rows[0]["GioiTinh"].ToString() == "Nữ")
                {
                    if (tuoi < 18 || tuoi > 60)
                    {
                        e.Cancel = true;
                        XtraMessageBox.Show("Độ tuổi không hợp lệ\nNgười thừa kế là nữ giới phải nằm trong độ tuổi từ 18 - 55 tuổi ", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            // Người thừa kế đứng tên thừa kế trong dự án khác
            string sql = string.Format(@"SELECT t.SoCMND,t.SoHoKhau,THTra,GETDATE() Time 
                                         FROM   DTHoVay t 
                                                INNER JOIN MTDuAn m ON t.MTID = m.MTID
                                         WHERE  (t.CMNDTK = '{0}' OR t.SoHKTK = '{1}') AND THTra > GETDATE()"
                                        , dr["SoCMND"], dr["SoHoKhau"]);
            using (DataTable dtDTTK = db.GetDataTable(sql))
            {
                if (dtDTTK.Rows.Count > 0)
                {
                    e.Cancel = true;
                    if (dr["GioiTinh"].ToString() == "Nam")
                    {
                        XtraMessageBox.Show("Ông: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác", Config.GetValue("PackageName").ToString());
                    }
                    else if (dr["GioiTinh"].ToString() == "Nữ")
                    {
                        XtraMessageBox.Show("Bà: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác", Config.GetValue("PackageName").ToString());
                    }
                    return;
                }
            }
            // kiểm tra người thừa kế có phải là người vay trong dự án khác
            sql = string.Format(@"SELECT   t.SoCMND,t.SoHoKhau,THTra,GETDATE() Time 
                                                   FROM     DTHoVay t 
                                                            INNER JOIN MTDuAn m ON t.MTID = m.MTID
                                                   WHERE    (t.SoCMND = '{0}' OR t.SoHoKhau = '{1}') AND THTra > GETDATE()"
                                                    , dr["SoCMND"], dr["SoHoKhau"]);
            using (DataTable dtNTK = db.GetDataTable(sql))
            {
                if (dtNTK.Rows.Count > 0)
                {
                    e.Cancel = true;
                    XtraMessageBox.Show("Hộ dân đang nằm trong dự án thu hồi vốn\n\t Không được đứng tên người thừa kế", Config.GetValue("PackageName").ToString());
                    return;
                }
            }
        }

        void NguoiVay_QueryCloseUp(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // giới hạn độ tuổi người vay
            GridLookUpEdit gr = sender as GridLookUpEdit;
            DataRow dr = gr.Properties.View.GetDataRow(gr.Properties.View.FocusedRowHandle);

            if (gr.Properties.ReadOnly || dr == null)
                return;
                        
            string query = string.Format(@"SELECT DATEDIFF(YEAR,NgaySinh,GETDATE()) Tuoi,GioiTinh 
                                           FROM   DMHoDan WHERE ID = '{0}'",dr["ID"]);
            using (DataTable dt = db.GetDataTable(string.Format(query)))
            {
                if (dt.Rows.Count == 0)
                    return;
                int tuoi = int.Parse(dt.Rows[0]["Tuoi"].ToString());
                if (dt.Rows[0]["GioiTinh"].ToString() == "Nam")
                {
                    if (tuoi < 18 || tuoi > 60)
                    {
                        XtraMessageBox.Show("Độ tuổi không hợp lệ\n\t Người vay vốn là nam giới phải nằm trong độ tuổi từ 18 - 60 tuổi ", Config.GetValue("PackageName").ToString());
                        e.Cancel = true;
                        return;
                    }
                }
                else if (dt.Rows[0]["GioiTinh"].ToString() == "Nữ")
                {
                    if (tuoi < 18 || tuoi > 55)
                    {
                        e.Cancel = true;
                        XtraMessageBox.Show("Độ tuổi không hợp lệ\n\tNgười vay vốn là nữ giới phải nằm trong độ tuổi từ 18 - 55 tuổi ", Config.GetValue("PackageName").ToString());
                        return;
                    }
                }
            }
            //
            string sql = string.Format(@"SELECT t.SoCMND,t.SoHoKhau,THTra,GETDATE() Time 
                                         FROM   DTHoVay t 
                                                INNER JOIN MTDuAn m ON t.MTID = m.MTID
                                         WHERE  (t.SoCMND = '{0}' OR t.SoHoKhau = '{1}') AND THTra > GETDATE()"
                                        , dr["SoCMND"], dr["SoHoKhau"]);
            using (DataTable dtKTHV = db.GetDataTable(sql))
            {
                if (dtKTHV.Rows.Count > 0)
                {
                    e.Cancel = true;
                    XtraMessageBox.Show("Hộ dân đang nằm trong dự án thu hồi vốn", Config.GetValue("PackageName").ToString());
                    return;
                }
            }
            // Là người thừa kế của dự án khác, ko đc vay vốn
            sql = string.Format(@"SELECT  t.SoCMND,t.SoHoKhau,THTra,GETDATE()Time
                                  FROM    DTHoVay t 
                                          INNER JOIN MTDuAn m ON t.MTID = m.MTID
                                  WHERE   (t.CMNDTK = '{0}' OR t.SoHoKhau = '{0}') AND THTra > GETDATE()"
                                , dr["SoCMND"], dr["SoHoKhau"]);
            using(DataTable dtKTNTK = db.GetDataTable(sql))
            {
                if (dtKTNTK.Rows.Count > 0)
                {
                    e.Cancel = true;
                    if (dr["GioiTinh"].ToString() == "Nam")
                    {
                        XtraMessageBox.Show("Ông: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác\n\t Không được vay vốn", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }
                    else if (dr["GioiTinh"].ToString() == "Nữ")
                    {
                        XtraMessageBox.Show("Bà: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác\n\t Không được vay vốn", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }
            }

            #region ... zzz
            //// kiểm tra số CMND và số hộ khẩu người vay
            //DataTable dthk = new DataTable();
            //DataTable dtcm = new DataTable();
            //dtcm = db.GetDataTable(string.Format("SELECT t.SoCMND,t.SoHoKhau,THTra,GETDATE() Time FROM DTHoVay t INNER JOIN MTDuAn m ON t.MTID = m.MTID  WHERE t.SoCMND = '{0}'", dr["SoCMND"].ToString()));
            //dthk = db.GetDataTable(string.Format("SELECT t.SoCMND,t.SoHoKhau,THTra,GETDATE() Time FROM DTHoVay t INNER JOIN MTDuAn m ON t.MTID = m.MTID   WHERE t.SoHoKhau = '{0}'", dr["SoHoKhau"].ToString()));
            //if (dtcm.Rows.Count > 0 && dtcm.Rows[0]["SoCMND"].ToString() != null || dthk.Rows.Count > 0 && dthk.Rows[0]["SoCMND"].ToString() != null)
            //{
            //    if (dthk.Rows[0]["THTra"] == DBNull.Value || dtcm.Rows[0]["THTra"] == DBNull.Value)
            //    {
            //        e.Cancel = true;
            //        XtraMessageBox.Show("Hộ dân đang nằm trong dự án thu hồi vốn", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    else if(DateTime.Compare( DateTime.Parse(dtcm.Rows[0]["THTra"].ToString()), DateTime.Parse(dtcm.Rows[0]["Time"].ToString())) > 0
            //        || DateTime.Compare( DateTime.Parse(dthk.Rows[0]["THTra"].ToString()) , DateTime.Parse(dthk.Rows[0]["Time"].ToString()))> 0)
            //    {
            //        e.Cancel = true;
            //        XtraMessageBox.Show("Hộ dân đang nằm trong dự án thu hồi vốn", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //}
            // người thừa kế không được vay vốn
            //DataTable cmtk = new DataTable();
            //DataTable hktk = new DataTable();
            //cmtk = db.GetDataTable(string.Format("SELECT t.SoCMND,t.SoHoKhau,THTra,GETDATE()Time FROM DTHoVay t INNER JOIN MTDuAn m ON t.MTID = m.MTID  WHERE t.CMNDTK = '{0}'", dr["SoCMND"].ToString()));
            //hktk = db.GetDataTable(string.Format("SELECT t.SoCMND,t.SoHoKhau,THTra,GETDATE() Time FROM DTHoVay t INNER JOIN MTDuAn m ON t.MTID = m.MTID  WHERE t.SoHKTK = '{0}'", dr["SoHoKhau"].ToString()));
            //if (cmtk.Rows.Count > 0 && cmtk.Rows[0]["SoCMND"].ToString() != null || hktk.Rows.Count > 0 && hktk.Rows[0]["SoCMND"].ToString() != null)
            //{
            //    if (cmtk.Rows.Count > 0 && cmtk.Rows[0]["THTra"] == DBNull.Value || hktk.Rows.Count > 0 && hktk.Rows[0]["THTra"] == DBNull.Value)
            //    {
            //        e.Cancel = true;
            //        if (dr["GioiTinh"].ToString() == "Nam")
            //        {
            //            XtraMessageBox.Show("Ông: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác\n\t Không được vay vốn", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);

            //        }
            //        else if (dr["GioiTinh"].ToString() == "Nữ")
            //        {
            //            XtraMessageBox.Show("Bà: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác\n\t Không được vay vốn", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        }
            //        return;
            //    }
            //    if (cmtk.Rows.Count > 0 && DateTime.Compare(DateTime.Parse(cmtk.Rows[0]["THTra"].ToString()), DateTime.Parse(cmtk.Rows[0]["Time"].ToString())) > 0
            //    || hktk.Rows.Count > 0 && DateTime.Compare(DateTime.Parse(hktk.Rows[0]["THTra"].ToString()), DateTime.Parse(hktk.Rows[0]["Time"].ToString())) > 0)
            //    {
            //        e.Cancel = true;
            //        if (dr["GioiTinh"].ToString() == "Nam")
            //        {
            //            XtraMessageBox.Show("Ông: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);

            //        }
            //        else if (dr["GioiTinh"].ToString() == "Nữ")
            //        {
            //            XtraMessageBox.Show("Bà: " + dr["HoTen"].ToString() + " Đang đứng tên người thừa kế dự án khác", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        }
            //        return;
            //    }
            //}
            #endregion
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = (data.BsMain.DataSource as BindingSource).DataSource as DataSet;            
            if (ds == null)
                return;            
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(GetDuAn_ColumnChanged);
        }

        void GetDuAn_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (drMaster == null)
                drMaster = e.Row;

            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName.ToUpper().Equals("NGUOIVAY"))
            {
                if (e.Row.RowState != DataRowState.Added) return;// Chỉ set giá trị khi thêm mới
                 
                string sql = string.Format("SELECT LaiSuat,THVay FROM MTDuAn WHERE MTID = '{0}'", e.Row["MTID"]);
                using (DataTable dt = db.GetDataTable(sql))
                {
                    if (dt.Rows.Count > 0)
                    {
                        e.Row["LaiSuat"] = dt.Rows[0]["LaiSuat"];
                        e.Row["TGVay"] = dt.Rows[0]["THVay"];
                    }
                }
            }
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
