using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KangarooWindowService
{
    partial class KangarooService : ServiceBase
    {
        private static Log_Sytems logs = new Log_Sytems();
        private int typeRun = 0;
        private string strConnect1 = ConfigurationManager.AppSettings["ConnectERP"].ToString();
        private static string strConnect2 = ConfigurationManager.AppSettings["ConnectKangaroLH"].ToString();
        bool stopping_GetData = false;
        private static DataAccess db2 = new DataAccess(strConnect2);
        ManualResetEvent stoppedEvent;

        public KangarooService()
        {
            InitializeComponent();

            //1f= 60s = 100.000
            //timmer = new Timer(60000 * value_time);
            //timmer.Elapsed += new ElapsedEventHandler(timmer_Elapsed);
            this.stopping_GetData = false;

            this.stoppedEvent = new ManualResetEvent(false);

        }

        private void ThreadGetData(object state)
        {
            DataAccess db1 = new DataAccess(strConnect1);
            // Periodically check if the service is stopping.
            logs.ErrorLog("LHSVGuiTinNhan GetData Begin loop", "");


            string err = "";


            while (!this.stopping_GetData)
            {
                // Perform main service function here...
                double LHSVGetAll_TANSUAT_PHUT = 60;

                if (ConfigurationManager.AppSettings["TANSUAT_PHUT"] != null && ConfigurationManager.AppSettings["TANSUAT_PHUT"] != "")
                {
                    LHSVGetAll_TANSUAT_PHUT = double.Parse(ConfigurationManager.AppSettings["TANSUAT_PHUT"]);
                }

                try
                {
                    DataTable dt01 = db1.ExecuteQueryDataSet("select * from MKT_ASM", CommandType.Text, null);
                    DataTable dt02 = db1.ExecuteQueryDataSet("select * from MKT_AGENCY", CommandType.Text, null);
                    DataTable dt03 = db1.ExecuteQueryDataSet("select * from MKT_CUSTOMER", CommandType.Text, null);
                    DataTable dt04 = db1.ExecuteQueryDataSet("select * from MKT_ASM_CUSTOMER", CommandType.Text, null);
                    DataTable dt05 = db1.ExecuteQueryDataSet("select * from MKT_CUSTOMER_AGENCY", CommandType.Text, null);
                    DataTable dt06 = db1.ExecuteQueryDataSet("select * from MKT_SUPPLIER", CommandType.Text, null);
                    DataTable dt07 = db1.ExecuteQueryDataSet("select * from MKT_KENH", CommandType.Text, null);
                    DataTable dt08 = db2.ExecuteQueryDataSet("select top 5000 * from KhachHang where KinhDo = 0 and ViDo = 0", CommandType.Text, null);

                    if(dt08.Rows.Count > 0)
                    {
                        foreach(DataRow dr in dt08.Rows)
                        {
                            CapNhatToaDoDiemBan(dr);
                        }
                    }

                    if (dt07.Rows.Count > 0)
                    {
                        foreach(DataRow dr in dt07.Rows)
                        {
                            DongBo_NganhHang(dr);
                        }
                    }


                    if (dt01.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt01.Rows)
                        {
                            DongBoASM(dr);
                        }
                    }

                    if (dt02.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt02.Rows)
                        {
                            DongBoAGENCY(dr);
                        }
                    }

                    if (dt03.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt03.Rows)
                        {
                            DongBoCUSTOMER(dr);
                        }
                    }

                    if (dt04.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt04.Rows)
                        {
                            DongBo_ASM_CUSTOMER(dr);
                        }
                    }

                    if (dt05.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt05.Rows)
                        {
                            DongBo_CUSTOMER_AGENCY(dr);
                        }
                    }

                    if (dt06.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt06.Rows)
                        {
                            DongBo_SUPPLIER(dr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logs.ErrorLog(ex.Message, ex.StackTrace);
                }



                // string thoigian = ConfigurationManager.AppSettings["sophut"].ToString();
                //int value_time = 0;

                logs.ErrorLog("Dong bo ERP luc :" + DateTime.Now.AddMinutes(LHSVGetAll_TANSUAT_PHUT), "");
                Thread.Sleep((int)(60000 * LHSVGetAll_TANSUAT_PHUT));  // Simulate some lengthy operations.
            }
            logs.ErrorLog("Dong bo ERP End loop", "");

            // Signal the stopped event.
            this.stoppedEvent.Set();
        }

        private void CapNhatToaDoDiemBan(DataRow dr)
        {
            string err = "";
            int macdinh = 0, loaikhachhang = 1, idcty = 1, idtinh = 0, idquan = 0, idphuong = 0;
            double kinhdo = 0, vido = 0;

            try
            {
                Location lc = Utils.GetToaDoByDiaDiem(dr["DiaChi"].ToString().Trim());
                vido = lc.Latitude;
                kinhdo = lc.Longitude;

                if (lc.Longitude > 0)
                {
                    idtinh = int.Parse(db2.MyExecuteScalar(string.Format("select Top 1 * from TinhThanh where   N'%'+TenTinh+'%'   LIKE N'%{0}%'", lc.Tinh), CommandType.Text, ref err, null).ToString());
                    idquan = int.Parse(db2.MyExecuteScalar(string.Format("select Top 1 * from QuanHuyen where   N'%'+TenQuan+'%'   LIKE N'%{0}%' AND({1} = 0 OR ID_Tinh = {1})", lc.QuanHuyen, idtinh), CommandType.Text, ref err, null).ToString());
                    idphuong = int.Parse(db2.MyExecuteScalar(string.Format("select Top 1 * from PhuongXa where   N'%'+TenPhuong+'%'   LIKE N'%{0}%' AND({1} = 0 OR ID_Quan = {1})", lc.PhuongXa, idquan), CommandType.Text, ref err, null).ToString());
                }
            }
            catch (Exception ex)
            {
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Data + ex.Message + ex.Source + ex.InnerException, ex.StackTrace);
            }

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@idkhachhang", int.Parse(dr["ID_KhachHang"].ToString())));
            param.Add(new SqlParameter("@kinhdo", kinhdo));
            param.Add(new SqlParameter("@vido", vido));
            param.Add(new SqlParameter("@idtinh", idtinh));
            param.Add(new SqlParameter("@idquan", idquan));
            param.Add(new SqlParameter("@idphuong", idphuong));

            db2.MyExecuteNonQuery("sp_DongBo_CapNhatToaDoKhachHang", CommandType.StoredProcedure, ref err, param);
        }

        //Đồng bộ Nhân Viên ASM
        private void DongBoASM(DataRow dr)
        {
            string err = "";
            int idqllh = 1, idnhanvien = 0, idql = 1, idnhom = 2, truongnhom = 0;

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@ID_QLLH", idqllh));
            param.Add(new SqlParameter("@ID_NhanVien", idnhanvien));
            param.Add(new SqlParameter("@TenNhanVien", (!string.IsNullOrEmpty(dr["ten_asm"].ToString())) ? dr["ten_asm"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@TenDangNhap", (!string.IsNullOrEmpty(dr["ma_asm"].ToString())) ? dr["ma_asm"].ToString().Trim().ToLower() + "@kangaroo.vn" : ""));
            param.Add(new SqlParameter("@MatKhau", "12345678"));
            param.Add(new SqlParameter("@DiaChi", ""));
            param.Add(new SqlParameter("@QueQuan", ""));
            param.Add(new SqlParameter("@NgaySinh", ""));
            param.Add(new SqlParameter("@Email", (!string.IsNullOrEmpty(dr["email"].ToString())) ? dr["email"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@DienThoai", (!string.IsNullOrEmpty(dr["sdt"].ToString())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@ID_QuanLy", idql));
            param.Add(new SqlParameter("@ID_Nhom", idnhom));
            param.Add(new SqlParameter("@TruongNhom", truongnhom));

            db2.MyExecuteNonQuery("sp_DongBo_NhanVien", CommandType.StoredProcedure, ref err, param);

        }

        //Đồng bộ Đại lý
        private void DongBoAGENCY(DataRow dr)
        {
            string err = "";
            int macdinh = 0, loaikhachhang = 1, idcty = 1, idtinh = 0, idquan = 0, idphuong = 0;
            double kinhdo = 0, vido = 0;
            

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@iddaily", macdinh));
            param.Add(new SqlParameter("@idnhanvien", macdinh));
            param.Add(new SqlParameter("@idnganhhang", ""));
            param.Add(new SqlParameter("@idnpp", macdinh));
            param.Add(new SqlParameter("@coden", macdinh));
            param.Add(new SqlParameter("@loaibienbang", macdinh));
            param.Add(new SqlParameter("@idct", idcty));
            param.Add(new SqlParameter("@tendaily", (!string.IsNullOrEmpty(dr["Ten_dl"].ToString().Trim())) ? dr["Ten_dl"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@diachi", (!string.IsNullOrEmpty(dr["dia_chi_full"].ToString().Trim())) ? dr["dia_chi_full"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoai", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@kinhdo", kinhdo));
            param.Add(new SqlParameter("@vido", vido));
            param.Add(new SqlParameter("@nguoilienhe", (!string.IsNullOrEmpty(dr["nguoi_dai_dien"].ToString().Trim())) ? dr["nguoi_dai_dien"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@chucvu", (!string.IsNullOrEmpty(dr["chuc_vu"].ToString().Trim())) ? dr["chuc_vu"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@email", (!string.IsNullOrEmpty(dr["email"].ToString().Trim())) ? dr["email"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@fax", ""));
            param.Add(new SqlParameter("@website", ""));
            param.Add(new SqlParameter("@tknganhang", ""));
            param.Add(new SqlParameter("@masothue", ""));
            param.Add(new SqlParameter("@ghichu", (!string.IsNullOrEmpty(dr["ghi_chu"].ToString().Trim())) ? dr["ghi_chu"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@ngaybatdauhopdong", ""));
            param.Add(new SqlParameter("@ngayketthuchopdong", ""));
            param.Add(new SqlParameter("@tiendien", ""));
            param.Add(new SqlParameter("@tienthue", ""));
            param.Add(new SqlParameter("@soden", macdinh));
            param.Add(new SqlParameter("@tinhthanhid", idtinh));
            param.Add(new SqlParameter("@quanhuyenid", idquan));
            param.Add(new SqlParameter("@phuongxaid", idphuong));
            param.Add(new SqlParameter("@duongpho", (!string.IsNullOrEmpty(dr["dia_chi_full"].ToString().Trim())) ? dr["dia_chi_full"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@makh", (!string.IsNullOrEmpty(dr["ma_dl"].ToString().Trim())) ? dr["ma_dl"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoai2", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoai3", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoaimacdinh", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@idloaikhachhang", loaikhachhang));

            db2.MyExecuteNonQuery("sp_DongBo_DaiLy", CommandType.StoredProcedure, ref err, param);
        }

        //Đồng bộ ASM-NPP
        private void DongBo_ASM_CUSTOMER(DataRow dr)
        {
            string err = "";

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@MaAsm", (!string.IsNullOrEmpty(dr["ma_asm"].ToString())) ? dr["ma_asm"].ToString().Trim().ToLower() + "@kangaroo.vn" : ""));
            param.Add(new SqlParameter("@MaNPP", dr["Ma_kh"].ToString().Trim()));
            db2.MyExecuteNonQuery("sp_DongBo_ASM_NPP", CommandType.StoredProcedure, ref err, param);

            string idnganhhang = "";
            string idnganhhangnv = db2.MyExecuteScalar(string.Format("select ID_NganhHang from NhanVien where  TenDangNhap like '{0}'", dr["Ma_asm"].ToString().Trim()), CommandType.Text, ref err, null).ToString().Trim();
            try
            {
                idnganhhang = db2.MyExecuteScalar(string.Format("select ID_NganhHang from KhachHang where  MaKH like '{0}'", dr["Ma_kh"].ToString().Trim()), CommandType.Text, ref err, null).ToString();

                if(!idnganhhangnv.Contains(idnganhhangnv) && !string.IsNullOrEmpty(idnganhhangnv))
                {
                    idnganhhangnv += "," + idnganhhang;
                }
                else if (string.IsNullOrEmpty(idnganhhangnv))
                {
                    idnganhhangnv += idnganhhang;
                }

                db2.MyExecuteNonQuery(string.Format("update NhanVien set ID_NganhHang = '{0}' where TenDangNhap like '{1}'", idnganhhangnv, dr["Ma_asm"].ToString().Trim()), CommandType.Text, ref err, null);
            }
            catch (Exception ex)
            {
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Data + ex.Message + ex.Source + ex.InnerException, ex.StackTrace);
            }


        }

        //Đồng bộ NPP
        private void DongBoCUSTOMER(DataRow dr)
        {
            string err = "";
            int macdinh = 0, loaikhachhang = 1, idcty = 1, idtinh = 0, idquan = 0, idphuong = 0;
            double kinhdo = 0, vido = 0;


            //try
            //{
            //    Location lc = Utils.GetToaDoByDiaDiem(dr["dia_chi"].ToString().Trim());

            //    vido = lc.Latitude;
            //    kinhdo = lc.Longitude;

            //    if (lc.Longitude > 0)
            //    {
            //        idtinh = int.Parse(db2.MyExecuteScalar(string.Format("select Top 1 * from TinhThanh where   N'%'+TenTinh+'%'   LIKE N'%{0}%'", lc.Tinh), CommandType.Text, ref err, null).ToString());
            //        idquan = int.Parse(db2.MyExecuteScalar(string.Format("select Top 1 * from QuanHuyen where   N'%'+TenQuan+'%'   LIKE N'%{0}%' AND({1} = 0 OR ID_Tinh = {1})", lc.QuanHuyen, idtinh), CommandType.Text, ref err, null).ToString());
            //        idphuong = int.Parse(db2.MyExecuteScalar(string.Format("select Top 1 * from PhuongXa where   N'%'+TenPhuong+'%'   LIKE N'%{0}%' AND({1} = 0 OR ID_Quan = {1})", lc.PhuongXa, idquan), CommandType.Text, ref err, null).ToString());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logs = new Log_Sytems();
            //    logs.ErrorLog(ex.Data + ex.Message + ex.Source + ex.InnerException, ex.StackTrace);
            //}


            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@iddaily", macdinh));
            param.Add(new SqlParameter("@idnhanvien", macdinh));

            string idnganhhang = "";
            try
            {
                idnganhhang = (!string.IsNullOrEmpty(dr["ma_kenh"].ToString().Trim())) ? db2.MyExecuteScalar(string.Format("select ID_NganhHang from NganhHang where  MaNganh like N'%{0}%'", dr["ma_kenh"].ToString().Trim()), CommandType.Text, ref err, null).ToString() : "";
            }
            catch (Exception ex)
            {
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Data + ex.Message + ex.Source + ex.InnerException, ex.StackTrace);
            }

            param.Add(new SqlParameter("@idnganhhang", idnganhhang));
            param.Add(new SqlParameter("@idnpp", macdinh));
            param.Add(new SqlParameter("@coden", macdinh));
            param.Add(new SqlParameter("@loaibienbang", macdinh));
            param.Add(new SqlParameter("@idct", idcty));
            param.Add(new SqlParameter("@tendaily", (!string.IsNullOrEmpty(dr["Ten_kh"].ToString().Trim())) ? dr["Ten_kh"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@diachi", (!string.IsNullOrEmpty(dr["dia_chi"].ToString().Trim())) ? dr["dia_chi"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoai", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@kinhdo", kinhdo));
            param.Add(new SqlParameter("@vido", vido));
            param.Add(new SqlParameter("@nguoilienhe", (!string.IsNullOrEmpty(dr["nguoi_dai_dien"].ToString().Trim())) ? dr["nguoi_dai_dien"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@chucvu", (!string.IsNullOrEmpty(dr["chuc_vu"].ToString().Trim())) ? dr["chuc_vu"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@email", (!string.IsNullOrEmpty(dr["email"].ToString().Trim())) ? dr["email"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@fax", ""));
            param.Add(new SqlParameter("@website", ""));
            param.Add(new SqlParameter("@tknganhang", ""));
            param.Add(new SqlParameter("@masothue", ""));
            param.Add(new SqlParameter("@ghichu", (!string.IsNullOrEmpty(dr["ghi_chu"].ToString().Trim())) ? dr["ghi_chu"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@ngaybatdauhopdong", ""));
            param.Add(new SqlParameter("@ngayketthuchopdong", ""));
            param.Add(new SqlParameter("@tiendien", ""));
            param.Add(new SqlParameter("@tienthue", ""));
            param.Add(new SqlParameter("@soden", macdinh));

            param.Add(new SqlParameter("@tinhthanhid", idtinh));
            param.Add(new SqlParameter("@quanhuyenid", idquan));
            param.Add(new SqlParameter("@phuongxaid", idphuong));

            param.Add(new SqlParameter("@duongpho", (!string.IsNullOrEmpty(dr["dia_chi"].ToString().Trim())) ? dr["dia_chi"].ToString().Trim(): ""));
            param.Add(new SqlParameter("@makh", (!string.IsNullOrEmpty(dr["Ma_kh"].ToString().Trim())) ? dr["Ma_kh"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoai2", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoai3", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@sodienthoaimacdinh", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@idloaikhachhang", loaikhachhang));

            db2.MyExecuteNonQuery("sp_DongBo_DaiLy", CommandType.StoredProcedure, ref err, param);
        }

        //Đồng bộ NPP với Đại lý
        private void DongBo_CUSTOMER_AGENCY(DataRow dr)
        {
            string err = "";

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@MaDL", dr["Ma_dl"].ToString().Trim()));
            param.Add(new SqlParameter("@MaNPP", dr["Ma_ncc"].ToString().Trim()));
            db2.MyExecuteNonQuery("sp_DongBo_CUSTOMER_AGENCY", CommandType.StoredProcedure, ref err, param);
        }

        //Đồng bộ NCC
        private void DongBo_SUPPLIER(DataRow dr)
        {
            string err = "";
            int idqllh = 1, idql = 1, idnhom = 6, macdinh = 0;

            List<SqlParameter> par = new List<SqlParameter>();
            par.Add(new SqlParameter("@mancc", (!string.IsNullOrEmpty(dr["Ma_ncc"].ToString().Trim())) ? dr["Ma_ncc"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@mathang", macdinh));
            par.Add(new SqlParameter("@tenviettat", ""));
            par.Add(new SqlParameter("@congty", (!string.IsNullOrEmpty(dr["Ten_ncc"].ToString().Trim())) ? dr["Ten_ncc"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@tinh", macdinh));
            par.Add(new SqlParameter("@diachi", (!string.IsNullOrEmpty(dr["dia_chi"].ToString().Trim())) ? dr["dia_chi"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@giamdoc", (!string.IsNullOrEmpty(dr["ng_lien_he"].ToString().Trim())) ? dr["ng_lien_he"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@chucvu", (!string.IsNullOrEmpty(dr["chuc_vu"].ToString().Trim())) ? dr["chuc_vu"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@email", (!string.IsNullOrEmpty(dr["email"].ToString().Trim())) ? dr["email"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@didong", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            par.Add(new SqlParameter("@nguoigiaodich", ""));
            par.Add(new SqlParameter("@sdtgiaodich", ""));
            par.Add(new SqlParameter("@emailgiaodich", ""));
            par.Add(new SqlParameter("@ghichu", ""));

            int idncc = int.Parse(db2.MyExecuteScalar("sp_DongBo_NCC", CommandType.StoredProcedure, ref err, par).ToString());

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@ID_QLLH", idqllh));
            param.Add(new SqlParameter("@ID_NhanVien", macdinh));
            param.Add(new SqlParameter("@TenNhanVien", (!string.IsNullOrEmpty(dr["Ten_ncc"].ToString().Trim())) ? dr["Ten_ncc"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@TenDangNhap", (!string.IsNullOrEmpty(dr["Ma_ncc"].ToString().Trim())) ? dr["Ma_ncc"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@MatKhau", "12345678"));
            param.Add(new SqlParameter("@DiaChi", (!string.IsNullOrEmpty(dr["dia_chi"].ToString().Trim())) ? dr["dia_chi"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@QueQuan", ""));
            param.Add(new SqlParameter("@NgaySinh", ""));
            param.Add(new SqlParameter("@Email", (!string.IsNullOrEmpty(dr["email"].ToString().Trim())) ? dr["email"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@DienThoai", (!string.IsNullOrEmpty(dr["sdt"].ToString().Trim())) ? dr["sdt"].ToString().Trim() : ""));
            param.Add(new SqlParameter("@ID_QuanLy", idql));
            param.Add(new SqlParameter("@ID_Nhom", idnhom));
            param.Add(new SqlParameter("@TruongNhom", macdinh));
            param.Add(new SqlParameter("@NCC", idncc));

            db2.MyExecuteNonQuery("sp_DongBo_NVNCC", CommandType.StoredProcedure, ref err, param);
        }

        private void DongBo_NganhHang(DataRow dr)
        {
            string err = "";

            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@manganh", dr["ma_kenh"].ToString().Trim()));
            param.Add(new SqlParameter("@tennganh", dr["ten_kenh"].ToString().Trim()));
            db2.MyExecuteNonQuery("sp_DongBo_NganhHang", CommandType.StoredProcedure, ref err, param);
        }

        protected override void OnStart(string[] args)
        {
            logs.ErrorLog("KangarooService OnStart: " + DateTime.Now, null);


            // Log a service start message to the Application log. 
            // Queue the main service function for execution in a worker thread. 
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadGetData));
        }

        protected override void OnStop()
        {
            logs.ErrorLog("KangarooService OnStop: " + DateTime.Now, null);
            // Log a service stop message to the Application log. 

            // Indicate that the service is stopping and wait for the finish  
            // of the main service function (ThreadGetData). 
            this.stopping_GetData = true;
            this.stoppedEvent.WaitOne();
        }

        protected override void OnPause()
        {
            logs.ErrorLog("KangarooService OnPause: " + DateTime.Now, null);

            base.OnPause();
            // timmer.Stop();
            this.stoppedEvent.WaitOne();
        }
        protected override void OnShutdown()
        {
            logs.ErrorLog("KangarooService OnShutdown: " + DateTime.Now, null);
            base.OnShutdown();
            //timmer.Stop();
            this.stoppedEvent.WaitOne();
        }
    }
}
