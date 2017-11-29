using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Thrift.Protocol;
using Thrift.Transport;
using Newtonsoft.Json.Linq;


namespace Thrift.Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ThriftInterface.PublicService.Client client;

        private void Form1_Load(object sender, EventArgs e)
        {
            var transport = new TSocket("localhost", 9090);
            var protocol = new TBinaryProtocol(transport);
            client = new ThriftInterface.PublicService.Client(protocol);

            transport.Open();

            // 启动后台线程实时更新机器状态
            thread = new System.Threading.Thread(ThreadRead);
            thread.IsBackground = false;
            thread.Start();
        }

        System.Threading.Thread thread;
        private void ThreadRead()
        {
            while(true)
            {
                System.Threading.Thread.Sleep(1000);

                JArray jArray = JArray.Parse(client.GetJsonMachineState());
                int[] values = new int[10];
                // 解析开始
                for (int i = 0; i < jArray.Count; i++)
                {
                    JObject json = (JObject)jArray[i];
                    values[i] = json[nameof(ThriftInterface.MachineOne.RunState)].ToObject<int>();
                }

                if(IsHandleCreated) Invoke(new Action(() =>
                {
                    label1.Text = values[0].ToString();
                    label2.Text = values[1].ToString();
                    label3.Text = values[2].ToString();
                    label4.Text = values[3].ToString();
                    label5.Text = values[4].ToString();
                    label6.Text = values[5].ToString();
                    label7.Text = values[6].ToString();
                    label8.Text = values[7].ToString();
                    label9.Text = values[8].ToString();
                    label10.Text = values[9].ToString();
                }));
            }
        }


        private void ShowMessage(string msg)
        {
            if(textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(ShowMessage), msg);
                return;
            }

            textBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  ") + msg + Environment.NewLine);
        }

        private void userButton1_Click(object sender, EventArgs e)
        {
            // 读取运行中机台总数
            ShowMessage(client.GetRunningCount().ToString());
        }

        private void userButton2_Click(object sender, EventArgs e)
        {
            // 读取报警中机台总数
            ShowMessage(client.GetAlarmCount().ToString());
        }

        private void userButton3_Click(object sender, EventArgs e)
        {
            // 读取所有的报警信息
            ShowMessage(client.GetJsonMachineAlarm());
        }

        private void userButton4_Click(object sender, EventArgs e)
        {
            // 读取所有的压力信息
            ShowMessage(client.GetJsonMachinePress());
        }

        private void userButton5_Click(object sender, EventArgs e)
        {
            // 读取所有的运行信息
            ShowMessage(client.GetJsonMachineState());
        }

        private void userButton6_Click(object sender, EventArgs e)
        {
            // 读取所有的温度信息
            ShowMessage(client.GetJsonMachineTemp());
        }

        private void userButton7_Click(object sender, EventArgs e)
        {
            // 读取指定机台信息
            ThriftInterface.MachineOne machine = client.GetMachineOne("1#");
        }

        private void userButton8_Click(object sender, EventArgs e)
        {
            // 强制机台启动
            if(client.SetMachineRunState("1#",1))
            {
                ShowMessage("写入成功！");
            }
            else
            {
                ShowMessage("写入失败！");
            }
        }

        private void userButton10_Click(object sender, EventArgs e)
        {
            // 强制机台停止
            if(client.SetMachineRunState("1#",0))
            {
                ShowMessage("写入成功！");
            }
            else
            {
                ShowMessage("写入失败！");
            }
        }



        private void userButton9_Click(object sender, EventArgs e)
        {
            // 用于高频多线程压力测试
            new System.Threading.Thread(ThreadReadManyTimes) { IsBackground = true, Name = "1" }.Start();
            //new System.Threading.Thread(ThreadReadManyTimes) { IsBackground = true, Name = "2" }.Start();
            //new System.Threading.Thread(ThreadReadManyTimes) { IsBackground = true, Name = "3" }.Start();
        }

        private void ThreadReadManyTimes()
        {
            for (int i = 0; i < 1000; i++)
            {
                client.GetRunningCount();
            }

            ShowMessage(System.Threading.Thread.CurrentThread.Name + "完成！");
        }


    }
}
