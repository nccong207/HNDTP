using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data;
using CDTLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.IO;
using System.Diagnostics;
using CDTDatabase;
using System.Globalization;

namespace ImportDA
{
    public class ImportDA : ICControl
    {
        DataRow drCur;
        System.Data.DataTable _dtSource, dmNoiCap;
        DataCustomFormControl _data;
        InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        GridView gvMain;
        DateTimeFormatInfo _dtfi = new DateTimeFormatInfo();
        #region ICControl Members

        public void AddEvent()
        {
            _dtfi.ShortDatePattern = "dd/MM/yyyy";
            gvMain = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnXL = new SimpleButton();
            btnXL.Text = "Nhập từ excel";
            btnXL.Name = "btnXL";
            btnXL.Click += new EventHandler(btnXL_Click);
            LayoutControlItem lci = lcMain.AddItem("", btnXL);
            lci.Name = "cusXL";
            SimpleButton btnLayFile = new SimpleButton();
            btnLayFile.Text = "Lấy file mẫu";
            btnLayFile.Name = "btnLayFile";
            btnLayFile.Click += new EventHandler(btnLayFile_Click);
            LayoutControlItem lci1 = lcMain.AddItem("", btnLayFile);
            lci1.Name = "cusLayFile";
            Database db = Database.NewDataDatabase();
            dmNoiCap = db.GetDataTable("select * from DMTP");
        }

        void btnLayFile_Click(object sender, EventArgs e)
        {
            string f = Application.StartupPath + "//Reports//HTA//MauHoVay.xls";
            if (File.Exists(f))
            {
                if (XtraMessageBox.Show("Nhấn Có để mở file mẫu, nhấn Không để copy file mẫu",
                    Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Process.Start(f);
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.RestoreDirectory = true;
                    sfd.DefaultExt = "xls";
                    if (sfd.ShowDialog() == DialogResult.OK)
                        File.Copy(f, sfd.FileName);
                }
            }
            else
                XtraMessageBox.Show("Không tìm thấy file mẫu", Config.GetValue("PackageName").ToString());
        }

        void btnXL_Click(object sender, EventArgs e)
        {
            if (!gvMain.Editable)
            {
                XtraMessageBox.Show("Vui lòng thực hiện khi đang thêm/sửa số liệu",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            drCur = (_data.BsMain.Current as DataRowView).Row;
            if (drCur["PhuongXa"] == DBNull.Value || drCur["BoPhan"] == DBNull.Value || drCur["NgayLap"] == DBNull.Value)
            {
                XtraMessageBox.Show("Vui lòng nhập thông tin dự án trước",
                    Config.GetValue("PackageName").ToString());
                return;
            }
            OpenFileDialog f = new OpenFileDialog();
            f.RestoreDirectory = true;
            f.Filter = "Excel files (*.xls)|*.xls";
            if (f.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //nap data sheet vao _dtSource
                    string cnn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + f.FileName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
                    OleDbDataAdapter myCommand = new OleDbDataAdapter("SELECT * FROM [DanhSach$] WHERE [Họ tên] is not null AND [Số CMND] is not null", cnn);
                    _dtSource = new System.Data.DataTable();
                    myCommand.Fill(_dtSource);
                }
                catch(Exception ex)
                {
                    XtraMessageBox.Show("Lỗi lấy số liệu từ excel\n" + ex.Message,
                        Config.GetValue("PackageName").ToString());
                    return;
                }
                if (_dtSource.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Không tìm thấy hộ vay nào", Config.GetValue("PackageName").ToString());
                    return;
                }
                if (KiemTraHV())
                    ImportHV();
            }
        }

        // Ktra thông tin Hộ vay - người thừa kế
        bool CheckNguoiVay(Database db, string SoCMND, string SoHK)
        {
            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            string NgayLap = drCur["NgayLap"].ToString();
            using (DataTable dt = db.GetDataTable(string.Format(@"EXEC sp_CheckNguoiVay '{0}','{1}','{2}'", SoCMND, SoHK, NgayLap)))
            {
                if (dt.Rows.Count == 0) return true;

                XtraMessageBox.Show("Điều kiện vay của người vay có số CMND " + SoCMND + " chưa hợp lệ, vui lòng kiểm tra lại các quy định sau:\nĐộ tuổi: Nam từ 18-65 tuổi, Nữ: 18-60 tuổi. Hộ dân không thuộc dự án vay vốn khác. Hộ dân không là người thừa kế trong dự án khác.\n\nNỘI DUNG LỖI: "
                    + dt.Rows[0][0].ToString() , Config.GetValue("PackageName").ToString());
                return false;
            }
        }

        private bool KiemTraHV()
        {
            Database db = Database.NewDataDatabase();
            using (DataView dv = new DataView(_dtSource))
            {
                //kiem tra ho ten
                string s = "Một số hộ chưa có họ tên\nSTT: ";
                dv.RowFilter = "[Họ tên] is null";
                if (dv.Count > 0)
                {
                    foreach (DataRowView drv in dv)
                    {
                        int n = _dtSource.Rows.IndexOf(drv.Row) + 1;
                        s += n.ToString() + ", ";
                    }
                    s = s.Remove(s.Length - 2);
                    XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                    return false;
                }

                //kiem tra cmnd
                s = "Một số hộ chưa có số CMND\n";
                dv.RowFilter = "[Số CMND] is null";
                if (dv.Count > 0)
                {
                    foreach (DataRowView drv in dv)
                    {
                        s += drv[1].ToString() + ", ";
                    }
                    s = s.Remove(s.Length - 2);
                    XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                    return false;
                }

                //kiem tra dia chi
                s = "Một số hộ chưa có địa chỉ\n";
                dv.RowFilter = "[Địa chỉ] is null";
                if (dv.Count > 0)
                {
                    foreach (DataRowView drv in dv)
                    {
                        s += drv[1].ToString() + ", ";
                    }
                    s = s.Remove(s.Length - 2);
                    XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                    return false;
                }

                //kiem tra von
                s = "Một số hộ chưa đủ thông tin vốn\n";
                dv.RowFilter = "[Vốn tự có] is null or [Vốn xin vay] is null or [Vốn được duyệt] is null";
                if (dv.Count > 0)
                {
                    foreach (DataRowView drv in dv)
                    {
                        s += drv[1].ToString() + ", ";
                    }
                    s = s.Remove(s.Length - 2);
                    XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                    return false;
                }

                //kiem tra thoi gian vay
                s = "Một số hộ chưa có thời gian vay\n";
                dv.RowFilter = "[Thời gian vay] is null";
                if (dv.Count > 0)
                {
                    foreach (DataRowView drv in dv)
                    {
                        s += drv[1].ToString() + ", ";
                    }
                    s = s.Remove(s.Length - 2);
                    XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                    return false;
                }

                //kiem tra ngay cap CMND neu co
                s = "Ngày cấp CMND không hợp lệ: \n";
                dv.RowFilter = "[Ngày cấp] is not null";
                if (dv.Count > 0)
                {
                    bool rs = true;
                    foreach (DataRowView drv in dv)
                    {
                        DateTime dt;
                        if (!DateTime.TryParse(drv[6].ToString(), _dtfi, DateTimeStyles.None, out dt))
                        {
                            s += drv[1].ToString() + ", ";
                            rs = false;
                        }
                    }
                    if (rs == false)
                    {
                        s = s.Remove(s.Length - 2);
                        XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                        return false;
                    }
                }

                //kiem tra ngay cap CMND cua NTK neu co
                s = "Ngày cấp CMND của người thừa kế không hợp lệ: \n";
                dv.RowFilter = "[Ngày cấp1] is not null";
                if (dv.Count > 0)
                {
                    bool rs = true;
                    foreach (DataRowView drv in dv)
                    {
                        DateTime dt;
                        if (!DateTime.TryParse(drv[24].ToString(), _dtfi, DateTimeStyles.None, out dt))
                        {
                            s += drv[1].ToString() + ", ";
                            rs = false;
                        }
                    }
                    if (rs == false)
                    {
                        s = s.Remove(s.Length - 2);
                        XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                        return false;
                    }
                }

                //kiem tra noi cap CMND cua NTK neu co
                s = "Nơi cấp CMND của người thừa kế không hợp lệ: \n";
                dv.RowFilter = "[Nơi cấp1] is not null";
                if (dv.Count > 0)
                {
                    bool rs = true;
                    foreach (DataRowView drv in dv)
                    {
                       var ck = dmNoiCap.Select(string.Format("TenTP = '{0}'", drv[25].ToString()));
                        if (ck.Length == 0)
                        {
                            s += drv[1].ToString() + ", ";
                            rs = false;
                        }
                    }
                    if (rs == false)
                    {
                        s = s.Remove(s.Length - 2);
                        XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                        return false;
                    }
                }

                //kiem tra noi cap CMND cua Ho dan neu co
                s = "Nơi cấp CMND không hợp lệ: \n";
                dv.RowFilter = "[Nơi cấp] is not null";
                if (dv.Count > 0)
                {
                    bool rs = true;
                    foreach (DataRowView drv in dv)
                    {
                        var ck = dmNoiCap.Select(string.Format("TenTP = '{0}'", drv[7].ToString()));
                        if (ck.Length == 0)
                        {
                            s += drv[1].ToString() + ", ";
                            rs = false;
                        }
                    }
                    if (rs == false)
                    {
                        s = s.Remove(s.Length - 2);
                        XtraMessageBox.Show(s, Config.GetValue("PackageName").ToString());
                        return false;
                    }
                }

                //kiem tra dieu kien vay
                dv.RowFilter = string.Empty;
                foreach (DataRowView drv in dv)
                {
                    if (!CheckNguoiVay(db, drv["Số CMND"].ToString(), drv["Số hộ khẩu"].ToString()))
                        return false;
                }
            }
            string skt = "select SoCMND from DMHoDan where SoCMND = '{0}'";
            string si = @"insert into DMHoDan ([HoTen],[NgaySinh],[GioiTinh],[SDT],[SoCMND],[NgayCap],[NoiCap],[DiaChi],[SoHoKhau],[HKTT],[XaPhuong],[QuanHuyen]) 
                            values(@HoTen,@NgaySinh,@GioiTinh,@SDT,@SoCMND,@NgayCap,@NoiCap,@DiaChi,@SoHoKhau,@HKTT,@XaPhuong,@QuanHuyen)";
            string su = @"update DMHoDan set [HoTen] = @HoTen,[NgaySinh] = @NgaySinh,[GioiTinh] = @GioiTinh,[SDT] = @SDT,[NgayCap] = @NgayCap,[NoiCap] = @NoiCap,
                            [DiaChi] = @DiaChi,[SoHoKhau] = @SoHoKhau,[HKTT] = @HKTT,[XaPhuong] = @XaPhuong,[QuanHuyen] = @QuanHuyen where [SoCMND] = @SoCMND";
            string[] paras = new string[] {"HoTen","NgaySinh","GioiTinh","SDT","SoCMND","NgayCap","NoiCap", "DiaChi","SoHoKhau","HKTT","XaPhuong","QuanHuyen"};
            foreach (DataRow dr in _dtSource.Rows)
            {
                DataTable dtHD = db.GetDataTable(string.Format(skt, dr[5]));
                object oNgayCap = DBNull.Value;
                DateTime dtNgayCap;
                if (DateTime.TryParse(dr[6].ToString(), _dtfi, DateTimeStyles.None, out dtNgayCap))
                    oNgayCap = dtNgayCap;
                object oNgaySinh = DBNull.Value;
                if (dr[3].ToString() != "")
                    oNgaySinh = "1/1/" + dr[3].ToString();
                object oNoiCap = DBNull.Value;
                if (dr[7].ToString() != "")
                    oNoiCap = GetIdNoiCap(dr[7].ToString());
                object[] values = new object[] {dr[1], oNgaySinh, dr[2], dr[4], dr[5], oNgayCap, oNoiCap, dr[8], dr[9], dr[10], drCur["PhuongXa"], drCur["BoPhan"] };
                if (dtHD.Rows.Count == 0)
                    db.UpdateDatabyPara(si, paras, values);
                else
                    db.UpdateDatabyPara(su, paras, values);
                if (dr[19] != DBNull.Value && dr[23] != DBNull.Value)
                {
                    dtHD = db.GetDataTable(string.Format(skt, dr[23]));
                    oNgayCap = DBNull.Value;
                    if (DateTime.TryParse(dr[24].ToString(), _dtfi, DateTimeStyles.None, out dtNgayCap))
                        oNgayCap = dtNgayCap;
                    oNgaySinh = DBNull.Value;
                    if (dr[21].ToString() != "")
                        oNgaySinh = "1/1/" + dr[21].ToString();
                    oNoiCap = DBNull.Value;
                    if (dr[25].ToString() != "")
                        oNoiCap = GetIdNoiCap(dr[25].ToString());
                    values = new object[] { dr[19], oNgaySinh, dr[20], dr[22], dr[23], oNgayCap, oNoiCap, dr[26], dr[27], dr[28], drCur["PhuongXa"], drCur["BoPhan"] };
                    if (dtHD.Rows.Count == 0)
                        db.UpdateDatabyPara(si, paras, values);
                    else
                        db.UpdateDatabyPara(su, paras, values);
                }
                if (db.HasErrors)
                    break;
            }
            return !db.HasErrors;
        }

        private void ImportHV()
        {
            foreach (DataRow dr in _dtSource.Rows)
            {
                gvMain.AddNewRow();
                gvMain.SetFocusedRowCellValue(gvMain.Columns["NgayLap"], drCur["NgayLap"]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["HoTen"], dr[1]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SDT"], dr[4]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoCMND"], dr[5]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoHoKhau"], dr[9]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["HKTT"], dr[10]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["NNChinh"], dr[11]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["VonTuCo"], dr[12]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["CanVay"], dr[13]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["DuocVay"], dr[14]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["TGVay"], dr[15]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoNK"], dr[16]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLD"], dr[17]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoLDTH"], dr[18]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["HoTenTK"], dr[19]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["CMNDTK"], dr[23]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["SoHKTK"], dr[27]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["HKTTTK"], dr[28]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["QuanHe"], dr[29]);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["GhiChu"], dr[30]);
                gvMain.UpdateCurrentRow();
            }
        }

        private int GetIdNoiCap(string noicap)
        {
            var ck = dmNoiCap.Select(string.Format("TenTP = '{0}'", noicap));
            return Convert.ToInt32(ck[0]["TPID"].ToString()); 
        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }

        #endregion
    }
}
