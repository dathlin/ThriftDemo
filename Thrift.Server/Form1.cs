using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using Thrift.Transport;

namespace Thrift.Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region PLC Connection

        private SiemensTcpNet siemensTcp;                           // 和PLC的核心连接引擎
        private Timer timerReadPLC;                                 // 读取PLC的定时器

        #endregion


        private void Form1_Load(object sender, EventArgs e)
        {
            siemensTcp = new SiemensTcpNet(SiemensPLCS.S1200)
            {
                PLCIpAddress = System.Net.IPAddress.Parse("192.168.1.195")
            };

            // 连接到PLC
            siemensTcp.ConnectServer();

            timerReadPLC = new Timer();
            timerReadPLC.Interval = 1000;
            timerReadPLC.Tick += TimerReadPLC_Tick;
        }

        private void TimerReadPLC_Tick(object sender, EventArgs e)
        {
            // 每秒执行一次去读取PLC数据，此处简便操作，放在前台执行，正常逻辑应该放到后台
            HslCommunication.OperateResult<byte[]> read = siemensTcp.ReadFromPLC("M100", 24);
            if(read.IsSuccess)
            {
                handler.UpdateMachineOne("1#", read.Content);
            }
            else
            {
                // 读取失败，应该提示并记录日志，此处省略
                MessageBox.Show(read.ToMessageShowString());
            }
        }

        private bool WritePlc(string id, int value)
        {
            // 按道理根据不同的id写入不同的PLC，此处只有一个PLC，就直接写入到一个PLC中
            return siemensTcp.WriteIntoPLC("M100", value).IsSuccess;
        }



        private PublicServiceHandle handler;
        private TServer server;

        private void userButton1_Click(object sender, EventArgs e)
        {
            new System.Threading.Thread(() =>
            {
                // 启动服务
                handler = new PublicServiceHandle(WritePlc);
                var processor = new ThriftInterface.PublicService.Processor(handler);

                TServerTransport transport = new TServerSocket(9090);

                server = new TThreadPoolServer(processor, transport);
                server.Serve();
            })
            {
                IsBackground = true
            }.Start();

            // 启动定时器去读取PLC数据
            timerReadPLC.Start();
        }

        private void userButton2_Click(object sender, EventArgs e)
        {
            // 关闭服务
            server?.Stop();
        }
    }
}
