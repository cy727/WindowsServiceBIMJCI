using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsServiceBIMJCI
{
    public partial class ServiceBIMJCI : ServiceBase
    {
        System.Timers.Timer timer;

        private System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
        private System.Data.SqlClient.SqlCommand sqlComm = new System.Data.SqlClient.SqlCommand();
        private System.Data.SqlClient.SqlDataReader sqldr;
        private System.Data.SqlClient.SqlDataAdapter sqlDA = new System.Data.SqlClient.SqlDataAdapter();
        private System.Data.DataSet dSet = new DataSet();

        ClassUdpBacnet cBacnet = new ClassUdpBacnet();


        public string strConn = "workstation id=CY;packet size=4096;user id=sa;password=Bim.12345;data source=\"172.18.11.200,14333\";;initial catalog=B_bacnet";
        public ServiceBIMJCI()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //EventLog.WriteEntry("BIM JCI 服务启动");
            cBacnet.InitFacilityValue();
            

            sqlConn.ConnectionString = strConn;
            sqlComm.Connection = sqlConn;
            sqlDA.SelectCommand = sqlComm;

            timer = new System.Timers.Timer();
            //timer.Interval = 600000; //十分钟
            timer.Interval = 600000;

            timer.Elapsed += new ElapsedEventHandler(ScanTimer_Elapsed);
            timer.AutoReset = true;
            timer.Enabled = true;

            UpdateDataPool("S1", "P1", "V1", "U", "Disp", "N1", "N2", "N3", "N4");
            refreshDataPool();
            
        }

        
        public void ScanTimer_Elapsed(object source, ElapsedEventArgs e)
        {

            //UpdateDataPool("S1", "P1", "V1");

            refreshDataPool();


        }

        private void refreshDataPool()
        {
            int i = 0;
            for (i = 0; i < cBacnet.dSet.Tables["设备表"].Rows.Count; i++)
            {
                cBacnet.InitFacilityValue(cBacnet.dSet.Tables["设备表"].Rows[i][0].ToString());
                cBacnet.SendMessage();

                foreach (dParameter dp in cBacnet.cfFacility.parameter)
                {
                    UpdateDataPool(cBacnet.cfFacility.sFacilityNo, dp.Para_Name, dp.Para_Value, dp.Para_Unit, dp.Para_Display, dp.Para_Net, dp.Para_DDC, dp.Para_Type, dp.Para_No);
                }

            }
        }

        //记录参数值
        private void UpdateDataPool(string sFacilityCode, string sParaName, string sValue, string sUnit, string sDisp, string N1, string N2, string N3, string N4)
        {
            sqlComm.CommandText = "SELECT ID, 设备编号, 参数名称, 参数值, 取值时间 FROM 参数值表 WHERE (设备编号 = N'" + sFacilityCode + "') AND (参数名称 = N'" + sParaName + "')";
            try
            {
                sqlConn.Open();
                sqldr = sqlComm.ExecuteReader();

                if (!sqldr.HasRows) //调整参数值
                {
                    sqldr.Close();
                    sqlComm.CommandText = "INSERT INTO 参数值表 (设备编号, 参数名称, 参数值, 取值时间, 单位, 显示值, 网络编号, 物理地址, 点位编号, 点位值) VALUES (N'" + sFacilityCode + "', N'" + sParaName + "', N'" + sValue + "', CONVERT(DATETIME, '" + System.DateTime.Now.ToString() + "', 102), N'" + sUnit + "', N'" + sDisp + "', N'" + N1 + "', N'" + N2 + "', N'" + N3 + "', N'" + N4 + "')";
                    sqlComm.ExecuteNonQuery();

                }
                else //增加参数值
                {
                    sqldr.Close();
                    sqlComm.CommandText = "UPDATE 参数值表 SET 参数值 = N'" + sValue + "', 取值时间 = CONVERT(DATETIME, '" + System.DateTime.Now.ToString() + "', 102), 显示值 = N'" + sDisp + "', 网络编号 = N'" + N1 + "',  物理地址 = N'" + N2 + "', 点位编号 = N'" + N3 + "', 点位值 = N'" + N4 + "' WHERE (设备编号 = N'" + sFacilityCode + "') AND (参数名称 = N'" + sParaName + "')";
                    sqlComm.ExecuteNonQuery();
                }
            }
            finally
            {
                sqlConn.Close();
            }
        }
         

        protected override void OnStop()
        {
            //EventLog.WriteEntry("BIM JCI 服务停止");
        }
    }
}
