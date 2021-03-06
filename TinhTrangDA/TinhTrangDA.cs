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

namespace TinhTrangDA
{
    public class TinhTrangDA : ICData 
    {
        Database db = Database.NewDataDatabase();
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        DataRow drMaster;

        int vaOld;
        int vaNew;
        //int TongSoHo = 0;
        //int TongSoLD = 0;
        //int TongSoLDTH = 0;
        //int TongSoNN = 0;
        //decimal TongVonCo = 0;
        //decimal TongVonVay = 0;
        //decimal TongVonDA = 0;
        //int isHoTro = 0;
        //decimal TongVonDN;

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
            if (data.CurMasterIndex < 0) return;
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if ((drMaster.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Modified) 
                && (bool)drMaster["GiaiNgan", DataRowVersion.Original])
            {
                XtraMessageBox.Show("Bạn không được sửa/xóa dữ liệu khi dự án đã giải ngân", Config.GetValue("PackageName").ToString());
                info.Result = false;
                return;
            }
            if (drMaster.RowState != DataRowState.Deleted)
                CapNhatSoTong();
            //ChangeTT();
        }

        private void CapNhatSoTong()
        {
            int TrongTrot = 0;
            int VatNuoi = 0;
            int CayTrong = 0;
            int Khac = 0;
            int DichVu = 0;
            int ThuySan = 0;
            int TieuThuCong = 0;

            DataView dvDT = new DataView(data.DsData.Tables[1]);
            dvDT.RowFilter = "MTID = '" + drMaster["MTID"].ToString() + "'";
            if (dvDT.Count == 0)
                return;
            // Tính lại phần tổng hợp
            for (int i = 0; i < dvDT.Count; i++)
            {
                if (dvDT[i].Row["NNChinh"].ToString() == "1. Chăn nuôi")
                    VatNuoi += 1;

                if (dvDT[i].Row["NNChinh"].ToString() == "2. Trồng trọt")
                    TrongTrot += 1;

                if (dvDT[i].Row["NNChinh"].ToString() == "3. Rau màu")
                    CayTrong += 1;

                if (dvDT[i].Row["NNChinh"].ToString() == "4. Thủy sản")
                    ThuySan += 1;

                if (dvDT[i].Row["NNChinh"].ToString() == "5. Tiểu thủ công")
                    TieuThuCong += 1;

                if (dvDT[i].Row["NNChinh"].ToString() == "6. Dịch vụ")
                    DichVu += 1;

                if (dvDT[i].Row["NNChinh"].ToString() == "7. Khác")
                    Khac += 1;
            }

            drMaster["VatNuoi"] = VatNuoi;
            drMaster["TrongTrot"] = TrongTrot;
            drMaster["CayTrong"] = CayTrong;
            drMaster["ThuySan"] = ThuySan;
            drMaster["TCNghiep"] = TieuThuCong;
            drMaster["DichVu"] = DichVu;
            drMaster["Khac"] = Khac;
            //tong so ho
            drMaster["TSoHo"] = dvDT.Count;
            dvDT.RowFilter += " and IsHoTro = 1";
            drMaster["TSoHDN"] = dvDT.Count;
        }

        public InfoCustomData Info
        {
            get {return info; }
        }

        #endregion
    }
}
