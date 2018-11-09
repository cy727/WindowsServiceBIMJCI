using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace WindowsServiceBIMJCI
{
    class ClassUdpBacnet
    {
        private IPEndPoint ipLocalPoint;
        private IPEndPoint ipRemotePoint;
        private EndPoint RemotePoint;
        private Socket mySocket;
        private bool RunningFlag = false;

        public cFacility cfFacility = new cFacility();
        public cFacilityAll cfFacilityAll = new cFacilityAll();

        //private string sNet, sMacDDC, sStyle, sNo;
        private string sValue = "";
        private int iNo = 0;


        int port = 47808;
        IPAddress localHostIP = IPAddress.Parse("192.168.10.200");
        IPAddress remoteHostIP = IPAddress.Parse("192.168.10.22");

        //基本库
        private System.Data.SqlClient.SqlConnection sqlConn = new System.Data.SqlClient.SqlConnection();
        private System.Data.SqlClient.SqlCommand sqlComm = new System.Data.SqlClient.SqlCommand();
        private System.Data.SqlClient.SqlDataReader sqldr;
        private System.Data.SqlClient.SqlDataAdapter sqlDA = new System.Data.SqlClient.SqlDataAdapter();
        public System.Data.DataSet dSet = new DataSet();

        public ClassUdpBacnet()
        {
            string strConn = "workstation id=CY;packet size=4096;user id=sa;password=Abc123;data source=\"172.18.11.200\";;initial catalog=B_bacnet";
            sqlConn.ConnectionString = strConn;
            sqlComm.Connection = sqlConn;
            sqlDA.SelectCommand = sqlComm;

            sqlConn.Open();
            sqlComm.CommandText = "SELECT 设备监控特性表.ID, 设备监控特性表.设备编号, 设备监控特性表.网络编号, 设备监控特性表.物理地址, 设备监控特性表.点位编号, 设备监控特性表.点位值, 设备监控特性表.显示ID, 设备监控特性表.警报ID, 显示表.显示名称, 显示表.显示转换ID, 显示表.显示单位 FROM 设备监控特性表 INNER JOIN 显示表 ON 设备监控特性表.显示ID = 显示表.显示ID ORDER BY 设备监控特性表.ID";
            if (dSet.Tables.Contains("设备监控特性表")) dSet.Tables.Remove("设备监控特性表");
            sqlDA.Fill(dSet, "设备监控特性表");

            sqlComm.CommandText = "SELECT DISTINCT 设备编号 FROM 设备监控特性表";
            if (dSet.Tables.Contains("设备表")) dSet.Tables.Remove("设备表");
            sqlDA.Fill(dSet, "设备表");

            sqlComm.CommandText = "SELECT ID, 显示转换ID, 获取值, 显示值 FROM 显示转换表";
            if (dSet.Tables.Contains("显示转换表")) dSet.Tables.Remove("显示转换表");
            sqlDA.Fill(dSet, "显示转换表");
            sqlConn.Close();




            //启动一个新的线程，执行方法this.ReceiveHandle，  
            //以便在一个独立的进程中执行数据接收的操作  
            //RunningFlag = true;
            //Thread thread = new Thread(new ThreadStart(this.ReceiveHandle));
            //thread.Start();  
        }

        private void BindPoint()
        {
            ipLocalPoint = new IPEndPoint(localHostIP, port);

            //定义网络类型，数据连接类型和网络协议UDP  
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //绑定网络地址  
            mySocket.Bind(ipLocalPoint);

            //得到客户机IP  
            ipRemotePoint = new IPEndPoint(remoteHostIP, port);
            RemotePoint = (EndPoint)(ipRemotePoint);

        }

        //全部參數
        public void InitFacilityValue()
        {
            var qFalicity = from dtFacity in dSet.Tables["设备监控特性表"].AsEnumerable()//查询楼层
                            select dtFacity;
            foreach (var itemFacility in qFalicity)//显示查询结果
            {
                dParameterAll p1 = new dParameterAll();
                p1.sFacilityNo = itemFacility.Field<string>("设备编号");

                p1.Para_Net = itemFacility.Field<string>("网络编号");
                p1.Para_DDC = itemFacility.Field<string>("物理地址");
                p1.Para_Type = itemFacility.Field<string>("点位编号");
                p1.Para_No = itemFacility.Field<string>("点位值");
                p1.Para_Name = itemFacility.Field<string>("显示名称");
                p1.Para_Unit = itemFacility.Field<string>("显示单位");

                cfFacilityAll.parameterall.Add(p1);

            }
            //cfFacility.parameter[0].Para_Net = "2006"; cfFacility.parameter[0].Para_DDC = "24"; cfFacility.parameter[0].Para_Type = "AI"; cfFacility.parameter[0].Para_No = "10005";
            //cfFacility.parameter[1].Para_Net = "2006"; cfFacility.parameter[1].Para_DDC = "24"; cfFacility.parameter[1].Para_Type = "AI"; cfFacility.parameter[1].Para_No = "10008";
        }


        //编号参数
        public void InitFacilityValue(string sNo)
        {
            cfFacility.sFacilityNo = sNo;

            cfFacility.parameter.Clear();
            var qFalicity = from dtFacity in dSet.Tables["设备监控特性表"].AsEnumerable()//查询楼层
                            where (dtFacity.Field<string>("设备编号") == sNo)//条件
                            select dtFacity;
            foreach (var itemFacility in qFalicity)//显示查询结果
            {
                dParameter p1 = new dParameter();
                p1.Para_Net = itemFacility.Field<string>("网络编号");
                p1.Para_DDC = itemFacility.Field<string>("物理地址");
                p1.Para_Type = itemFacility.Field<string>("点位编号");
                p1.Para_No = itemFacility.Field<string>("点位值");
                p1.Para_Name = itemFacility.Field<string>("显示名称");
                p1.Para_Unit = itemFacility.Field<string>("显示单位");



                if (itemFacility.Field<int?>("显示转换ID") != null)
                    p1.sConvert = itemFacility.Field<int>("显示转换ID").ToString();

                cfFacility.parameter.Add(p1);
            }
            //cfFacility.parameter[0].Para_Net = "2006"; cfFacility.parameter[0].Para_DDC = "24"; cfFacility.parameter[0].Para_Type = "AI"; cfFacility.parameter[0].Para_No = "10005";
            //cfFacility.parameter[1].Para_Net = "2006"; cfFacility.parameter[1].Para_DDC = "24"; cfFacility.parameter[1].Para_Type = "AI"; cfFacility.parameter[1].Para_No = "10008";
        }

        /*
        public void SendMessage(string sNetIn, string sMacDDCIn, string sStyleIn, string sNoIn)
        {
            byte[] data = new byte[22];
            byte[] redata = new byte[1024];
            int rlen = 0,i,iCount;
            string sNet1="", sMacDDC1="", sStyle1="", sNo1="";

            data = UDPSend(sNetIn, sMacDDCIn, sStyleIn, sNoIn);
            sNet = sNetIn;sMacDDC=sMacDDCIn;sStyle=sStyleIn;sNo=sNoIn;

            mySocket.SendTo(data, data.Length, SocketFlags.None, RemotePoint);

            iCount = 0;
            while(true)
            {

                if (mySocket == null || mySocket.Available < 1)
                {
                    Thread.Sleep(200);
                    continue;
                }
                rlen = mySocket.ReceiveFrom(redata, ref RemotePoint);

                GetReceivePara(out sNet1, out sMacDDC1, out sStyle1, out sNo1, redata);
                if (sNet == sNet1 && sMacDDC == sMacDDC1 && sStyle == sStyle1 && sNo == sNo1) //返回正确值
                {
                    foreach(dParameter dp in cfFacility.parameter)
                    {
                        
                        if (dp.Para_Net == sNet1 && dp.Para_DDC == sMacDDC1 && dp.Para_Type == sStyle1 && dp.Para_No == sNo1) //找到对应参数
                        {
                            dp.Para_Value = UDPReceive(sNet, sMacDDC, sStyle, sNo, redata);
                            break;
                        }
                    }
                    break;
                }
                else //非正确值
                {
                    iCount++;
                    Thread.Sleep(10);
                }

                if (iCount >= 10)
                    break;

            }

            return ;
        }
        */

        //获得值
        public string SendMessage(string sNetIn, string sMacDDCIn, string sStyleIn, string sNoIn)
        {
            byte[] data = new byte[22];
            byte[] redata = new byte[1024];
            int rlen = 0,  iCount;
            string sNet1 = "", sMacDDC1 = "", sStyle1 = "", sNo1 = "";

            string Para_Value = "";

            data = UDPSend(sNetIn, sMacDDCIn, sStyleIn, sNoIn);

            int iii = mySocket.SendTo(data, data.Length, SocketFlags.None, RemotePoint);

            iCount = 0;
            while (true)
            {
                if (iCount > 5)
                    break;
                if (mySocket == null || mySocket.Available < 1)
                {
                    Thread.Sleep(200);
                    iCount++;
                    continue;
                }
                rlen = mySocket.ReceiveFrom(redata, ref RemotePoint);

                GetReceivePara(out sNet1, out sMacDDC1, out sStyle1, out sNo1, redata);
                if (sNetIn == sNet1 && sMacDDCIn == sMacDDC1 && sStyleIn == sStyle1 && sNoIn == sNo1) //返回正确值
                {
                    Para_Value = UDPReceive(sNetIn, sMacDDCIn, sStyleIn, sNoIn, redata);
                    break;
                }
                else //非正确值
                {
                    //iCount++;
                    //Thread.Sleep(10);
                    break;
                }


            }


            mySocket.Close();
            return Para_Value;
        }

        public bool SendMessage()
        {
            byte[] data = new byte[22];
            byte[] redata = new byte[1024];
            int rlen = 0, iCount;
            string sNet1 = "", sMacDDC1 = "", sStyle1 = "", sNo1 = "";
            string sNetIn, sMacDDCIn, sStyleIn, sNoIn;

            BindPoint();
            foreach (dParameter dpara in cfFacility.parameter) //所有参数循环
            {
                sNetIn = dpara.Para_Net; sMacDDCIn = dpara.Para_DDC; sStyleIn = dpara.Para_Type; sNoIn = dpara.Para_No;

                data = UDPSend(sNetIn, sMacDDCIn, sStyleIn, sNoIn);

                mySocket.SendTo(data, data.Length, SocketFlags.None, RemotePoint);

                iCount = 0;
                while (true)
                {
                    if (iCount > 5)
                        break;
                    if (mySocket == null || mySocket.Available < 1)
                    {
                        Thread.Sleep(200);
                        iCount++;
                        continue;
                    }
                    rlen = mySocket.ReceiveFrom(redata, ref RemotePoint);

                    GetReceivePara(out sNet1, out sMacDDC1, out sStyle1, out sNo1, redata);
                    if (sNetIn == sNet1 && sMacDDCIn == sMacDDC1 && sStyleIn == sStyle1 && sNoIn == sNo1) //返回正确值
                    {
                        foreach (dParameter dp in cfFacility.parameter)
                        {

                            if (dp.Para_Net == sNet1 && dp.Para_DDC == sMacDDC1 && dp.Para_Type == sStyle1 && dp.Para_No == sNo1) //找到对应参数
                            {
                                dp.Para_Value = UDPReceive(sNetIn, sMacDDCIn, sStyleIn, sNoIn, redata);

                                dp.Para_Display = dp.Para_Value;
                                //显示
                                var qparaDisp = from dtDisp in dSet.Tables["显示转换表"].AsEnumerable()//查询楼层
                                                where (dtDisp.Field<int?>("显示转换ID").ToString() == dp.sConvert) && (dtDisp.Field<string>("获取值") == dp.Para_Value)//条件
                                                select dtDisp;
                                foreach (var itemDisp in qparaDisp)//显示查询结果
                                {
                                    dp.Para_Display = itemDisp.Field<string>("显示值");
                                    break;
                                }

                                dp.Para_Display += dp.Para_Unit;

                                dp.bRead = true;
                                break;
                            }
                        }
                        break;
                    }
                    else //非正确值
                    {
                        //iCount++;
                        //Thread.Sleep(10);
                        break;
                    }


                }
            }

            mySocket.Close();
            return true;
        }

        //public delegate void MyInvoke(string strRecv);
        //private void ReceiveHandle()
        //{
        //    //接收数据处理线程  
        //    string msg;
        //    int i;
        //    byte[] data = new byte[1024];
        //    string sNet, sMacDDC, sStyle, sNo, sValue;

        //    while (RunningFlag)
        //    {

        //        if (mySocket == null || mySocket.Available < 1)
        //        {
        //            Thread.Sleep(200);
        //            continue;
        //        }
        //        //跨线程调用控件  
        //        //接收UDP数据报，引用参数RemotePoint获得源地址 
        //        sNet=""; sMacDDC=""; sStyle=""; sNo=""; 
        //        int rlen = mySocket.ReceiveFrom(data, ref RemotePoint);

        //        GetReceivePara(out sNet, out sMacDDC, out sStyle, out sNo, data);
        //        sValue = UDPReceive(sNet, sMacDDC, sStyle, sNo, data);




        //    }
        //}



        //格式转换

        private byte[] UDPSend(string sNet, string sMacDDC, string sStyle, string sNo)
        {

            if (sNet.Trim() == "" || sMacDDC.Trim() == "" || sStyle.Trim() == "" || sNo.Trim() == "")
                return null;

            byte[] data = new byte[22];
            data[0] = 0x81; data[1] = 0xA; data[2] = 0x0; data[3] = 0x16; data[4] = 0x1; data[5] = 0x24;
            data[6] = (byte)(int.Parse(sNet) / 256); data[7] = (byte)(int.Parse(sNet) % 256);
            data[8] = 0x1;
            data[9] = byte.Parse(sMacDDC);
            data[10] = 0xff; data[11] = 0x0; data[12] = 0x1; data[13] = 0x2c; data[14] = 0xc; data[15] = 0xc;

            switch (sStyle.ToUpper())
            {
                case "AI":
                    data[16] = 0x0; data[17] = 0x0;
                    break;
                case "BI":
                    data[16] = 0x0; data[17] = 0xc0;
                    break;
                case "AO":
                    data[16] = 0x0; data[17] = 0x40;
                    break;
                case "BO":
                    data[16] = 0x1; data[17] = 0x0;
                    break;
                case "AV":
                    data[16] = 0x0; data[17] = 0x80;
                    break;
                case "BV":
                    data[16] = 0x1; data[17] = 0x40;
                    break;
            }



            //data[18] = 0x27; data[19] = 0x15; data[20] = 0x19; data[21] = 0x1c;
            data[18] = (byte)(int.Parse(sNo) / 256); data[19] = (byte)(int.Parse(sNo) % 256);
            data[20] = 0x19; data[21] = 0x55;

            return data;
        }


        private string UDPReceive(string sNet, string sMacDDC, string sStyle, string sNo, byte[] redata)
        {
            //int iHZcount=0;
            string sResult = "";


            if (sNet.Trim() == "" || sMacDDC.Trim() == "" || sStyle.Trim() == "" || sNo.Trim() == "" || redata == null)
                return "";

            if (redata.Length < 27)
                return "";

            byte[] reRedata = new byte[redata.Length];

            if (redata[9] == (byte)(int.Parse(sMacDDC)))
            {
                if (redata[10] != 0x50)
                {
                    if (redata[19] == 0x1C) //描述
                    {
                        //iHZcount = 0;
                        switch (sStyle.ToUpper())
                        {
                            case "AI":
                            case "BI":
                            case "BO":
                            case "BV":
                                if (redata[21] > 0x74)
                                {
                                    //If rebyte(21) > &H74 Then

                                    Array.Copy(redata, 25, reRedata, 0, redata.Length - 24);
                                    sResult = System.Text.Encoding.Unicode.GetString(reRedata);
                                }
                                else
                                    sResult = "";
                                break;
                            case "A0":
                            case "AV":
                                if (redata[21] > 0x74)
                                {
                                    sResult = bytes4toreal(redata[22], redata[23], redata[24], redata[25]).ToString();
                                }
                                else
                                {
                                    sResult = "";
                                }
                                break;
                        }
                    }
                    else //取值
                    {
                        switch (sStyle.ToUpper())
                        {
                            case "AI":
                            case "AO":
                            case "AV":
                                sResult = bytes4toreal(redata[22], redata[23], redata[24], redata[25]).ToString();
                                break;
                            case "BI":
                            case "BO":
                            case "BV":
                                sResult = ((int)redata[22]).ToString();
                                break;

                        }
                    }
                }

            }



            return sResult;
        }

        private Single bytes4toreal(byte realdotpartbyte1, byte realdotpartbyte2, byte realdotpartbyte3, byte realdotpartbyte4)
        {
            byte realepart;

            Single realdotpart;

            Single sResult = 0;
            int i;
            byte b1temp, b2temp, b3temp, b4temp;

            realdotpart = 0;

            b1temp = realdotpartbyte1;
            b2temp = realdotpartbyte2;
            b3temp = realdotpartbyte3;
            b4temp = realdotpartbyte4;

            if (b1temp == 0 && b2temp == 0 && b3temp == 0 && b4temp == 0)
            {
                sResult = 0;
            }
            else
            {
                realepart = byteleft(b1temp, 1, b2temp);
                for (i = 1; i <= 7; i++)
                {
                    b2temp = realdotpartbyte2;
                    if ((b2temp & 0x40 / (int)Math.Pow(2, (i - 1))) == (0x40 / (int)Math.Pow(2, (i - 1))))
                    {
                        realdotpart = realdotpart + 0x1 / (Single)Math.Pow(2, i);
                    }

                }

                for (i = 1; i <= 8; i++)
                {
                    b3temp = realdotpartbyte3;
                    if ((b3temp & 0x80 / (int)Math.Pow(2, (i - 1))) == (0x80 / (int)Math.Pow(2, (i - 1))))
                    {
                        realdotpart = realdotpart + 0x1 / (Single)Math.Pow(2, 7 + i);
                    }

                }

                for (i = 1; i <= 8; i++)
                {
                    b4temp = realdotpartbyte4;
                    if ((b4temp & 0x80 / (int)Math.Pow(2, (i - 1))) == (0x80 / (int)Math.Pow(2, (i - 1))))
                    {
                        realdotpart = realdotpart + 0x1 / (Single)Math.Pow(2, 15 + i);
                    }
                }
                realdotpart = realdotpart + 1;

                sResult = (Single)Math.Pow(2, (realepart - 127)) * realdotpart;

            }

            return sResult;

        }

        private byte byteleft(byte byte1, int n, byte byte2)
        {
            byte intem = 0; //临时变量
            byte intem1 = 0; //临时变量

            int X, Y;

            intem1 = byte1;
            for (X = 1; X <= n; X++) //移多少位就循环多少次
            {
                for (Y = 8; Y >= 1; Y--) //从第八位(左边第一位)开始循环左移
                {
                    switch (Y)
                    {
                        case 8:
                            if ((intem1 & 0x80) == 0x80) //如果临时变量intem1的第八位是1
                            {
                                intem = 0x1; //则将临时变量intem置1,
                            }
                            else
                            {
                                intem = 0x0; //反之置0
                            }
                            break;
                        case 7:
                            if ((intem1 & 0x40) == 0x40) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x80); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0x7f); //反之将第八位置0(其它位不变)
                            }
                            break;
                        case 6: //操作与上面相同
                            if ((intem1 & 0x20) == 0x20) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x40); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xBf); //反之将第八位置0(其它位不变)
                            }
                            break;
                        case 5: //操作与上面相同
                            if ((intem1 & 0x10) == 0x10) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x20); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xDf); //反之将第八位置0(其它位不变)
                            }
                            break;
                        case 4: //操作与上面相同
                            if ((intem1 & 0x8) == 0x8) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x10); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xEf); //反之将第八位置0(其它位不变)
                            }
                            break;
                        case 3: //操作与上面相同
                            if ((intem1 & 0x4) == 0x4) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x8); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xF7); //反之将第八位置0(其它位不变)
                            }
                            break;
                        case 2: //操作与上面相同
                            if ((intem1 & 0x2) == 0x2) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x4); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xFB); //反之将第八位置0(其它位不变)
                            }
                            break;
                        case 1: //操作与上面相同
                            if ((intem1 & 0x1) == 0x1) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x2); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xFD); //反之将第八位置0(其它位不变)
                            }

                            if ((byte2 & 0x80) == 0x80) //如果临时变量intem1的第七位是1
                            {
                                intem1 = (byte)(intem1 | 0x1); //则将其第八位置1(其它位不变),
                            }
                            else
                            {
                                intem1 = (byte)(intem1 & 0xFE); //反之将第八位置0(其它位不变)
                            }

                            break;
                    }
                }
            }

            return intem1;
        }

        private void GetReceivePara(out string sNet, out string sMacDDC, out string sStyle, out string sNo, byte[] data)
        {
            sNet = ""; sMacDDC = ""; sStyle = ""; sNo = "";
            if (data == null)
                return;
            if (data.Length < 20)
                return;
            try
            {
                sNet = (data[6] * 256 + data[7]).ToString();
                sMacDDC = data[9].ToString();

                if (data[14] == 0x0 && data[15] == 0x0)
                    sStyle = "AI";
                if (data[14] == 0x0 && data[15] == 0xC0)
                    sStyle = "BI";
                if (data[14] == 0x0 && data[15] == 0x40)
                    sStyle = "AO";
                if (data[14] == 0x1 && data[15] == 0x0)
                    sStyle = "BO";
                if (data[14] == 0x0 && data[15] == 0x80)
                    sStyle = "AV";
                if (data[14] == 0x1 && data[15] == 0x40)
                    sStyle = "BV";

                sNo = (data[16] * 256 + data[17]).ToString();
            }
            catch
            {

            }


        }
    }

    public class dParameter
    {
        public string Para_Name = ""; //参数名称
        public string Para_Value = ""; //参数值
        public string Para_Unit = ""; //单位

        public string Para_Net = ""; //参数值
        public string Para_DDC = ""; //拨码
        public string Para_Type = ""; //类型
        public string Para_No = ""; //编号

        public bool bWarn = false; //警报
        public bool bRead = false; //读取标记
        public string Para_remark = ""; //备注

        public string sConvert = "";
        public string Para_Display = ""; //显示值

    }

    public class cFacility
    {
        public string sFacilityNo = ""; //编号
        public string dtReadingTime = ""; //时间
        public List<dParameter> parameter = new List<dParameter>();

    }

    public class dParameterAll
    {
        public string sFacilityNo = ""; //编号

        public string Para_Name = ""; //参数名称
        public string Para_Value = ""; //参数值
        public string Para_Unit = ""; //单位

        public string Para_Net = ""; //参数值
        public string Para_DDC = ""; //拨码
        public string Para_Type = ""; //类型
        public string Para_No = ""; //编号

        public bool bWarn = false; //警报
        public bool bRead = false; //读取标记
        public string Para_remark = ""; //备注



    }

    public class cFacilityAll
    {
        public List<dParameterAll> parameterall = new List<dParameterAll>();
    }
}
