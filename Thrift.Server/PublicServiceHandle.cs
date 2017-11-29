using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThriftInterface;
using Newtonsoft.Json.Linq;

namespace Thrift.Server
{
    public class PublicServiceHandle : ThriftInterface.PublicService.Iface
    {
        #region Constructor

        /// <summary>
        /// 实例化一个对象
        /// </summary>
        public PublicServiceHandle(Func<string,int,bool> write)
        {
            // 初始化数据
            list = new List<MachineOne>()
            {
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "1#",
                    IpAddress = "192.168.1.195",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "2#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "3#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "4#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "5#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "6#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "7#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "8#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "9#",
                },
                new MachineOne()
                {
                    Name = "测试设备",
                    Id = "10#",
                },
            };

            hybirdLock = new HslCommunication.Core.SimpleHybirdLock();

            FuncWriteIntoPlc = write ?? throw new ArgumentNullException("write");
        }


        #endregion
        
        #region Private Member

        private List<MachineOne> list;                              // 总的数据仓库
        private HslCommunication.Core.SimpleHybirdLock hybirdLock;  // 混合同步锁，比Lock性能要高的多
        private Func<string, int, bool> FuncWriteIntoPlc;           // 写入数据的委托，最终实现在外层

        #endregion

        #region Public Method

        /// <summary>
        /// 更新一台设备的数据，这个数据最终来自PLC
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        public void UpdateMachineOne(string id, byte[] content)
        {
            if (content == null) return;

            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    byte[] buffer = new byte[4];
                    // 获取运行状态
                    Array.Copy(content, 0, buffer, 0, 4);
                    Array.Reverse(buffer);
                    list[i].RunState = BitConverter.ToInt32(buffer, 0);
                    // 获取报警状态
                    Array.Copy(content, 4, buffer, 0, 4);
                    Array.Reverse(buffer);
                    list[i].AlarmState = BitConverter.ToInt32(buffer, 0);

                    // 其实信息参照这个就行
                    break;
                }
            }
            hybirdLock.Leave();
        }

        #endregion

        #region PublicService.Interface


        /// <summary>
        /// 获取当前报警的机台数
        /// </summary>
        /// <returns></returns>
        public int GetAlarmCount()
        {
            int count = 0;
            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].AlarmState != 0) count++;
            }
            hybirdLock.Leave();
            return count;
        }

        /// <summary>
        /// 获取所有设备的所有信息，一般不建议这么做
        /// </summary>
        /// <returns></returns>
        public List<MachineOne> GetAllMachineOnes()
        {
            return new List<MachineOne>(list);
        }

        /// <summary>
        /// 获取当前所有机台的报警信息
        /// </summary>
        /// <returns></returns>
        public string GetJsonMachineAlarm()
        {
            JArray jArray = new JArray();
            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                JObject json = new JObject();
                json.Add(nameof(MachineOne.Name), new JValue(list[i].Name));
                json.Add(nameof(MachineOne.Id), new JValue(list[i].Id));
                json.Add(nameof(MachineOne.AlarmState), new JValue(list[i].AlarmState));
                jArray.Add(json);
            }
            hybirdLock.Leave();
            return jArray.ToString();
        }

        /// <summary>
        /// 获取当前所有机台的压力值
        /// </summary>
        /// <returns></returns>
        public string GetJsonMachinePress()
        {
            JArray jArray = new JArray();
            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                JObject json = new JObject();
                json.Add(nameof(MachineOne.Name), new JValue(list[i].Name));
                json.Add(nameof(MachineOne.Id), new JValue(list[i].Id));
                json.Add(nameof(MachineOne.Press), new JValue(list[i].Press));
                jArray.Add(json);
            }
            hybirdLock.Leave();
            return jArray.ToString();
        }

        /// <summary>
        /// 获取当前所有机台的状态
        /// </summary>
        /// <returns></returns>
        public string GetJsonMachineState()
        {
            JArray jArray = new JArray();
            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                JObject json = new JObject();
                json.Add(nameof(MachineOne.Name), new JValue(list[i].Name));
                json.Add(nameof(MachineOne.Id), new JValue(list[i].Id));
                json.Add(nameof(MachineOne.RunState), new JValue(list[i].RunState));
                jArray.Add(json);
            }
            hybirdLock.Leave();
            return jArray.ToString();
        }

        /// <summary>
        /// 获取当前所有机台的温度
        /// </summary>
        /// <returns></returns>
        public string GetJsonMachineTemp()
        {
            JArray jArray = new JArray();
            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                JObject json = new JObject();
                json.Add(nameof(MachineOne.Name), new JValue(list[i].Name));
                json.Add(nameof(MachineOne.Id), new JValue(list[i].Id));
                json.Add(nameof(MachineOne.Temp), new JValue(list[i].Temp));
                jArray.Add(json);
            }
            hybirdLock.Leave();
            return jArray.ToString();
        }

        /// <summary>
        /// 获取单独的一台设备信息
        /// </summary>
        /// <param name="machineId"></param>
        /// <returns></returns>
        public MachineOne GetMachineOne(string machineId)
        {
            // 这里需要不需要使用克隆对象？不太清楚，直接返回列表的对象会不会有影响？
            return list.Find(m => m.Id == machineId);
        }

        /// <summary>
        /// 获取当前正在运行的总的机台数
        /// </summary>
        /// <returns></returns>
        public int GetRunningCount()
        {
            int count = 0;
            hybirdLock.Enter();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].RunState == 1) count++;
            }
            hybirdLock.Leave();
            return count;
        }
        
        /// <summary>
        /// 设置设备的运行状态
        /// </summary>
        /// <param name="machineId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool SetMachineRunState(string machineId, int state)
        {
            // 按道理说这个方法应该向PLC进行数据写入，但是具体的实现不应该在这一层
            return FuncWriteIntoPlc(machineId, state);
        }
        
        #endregion
    }
}
