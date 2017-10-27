using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PLCReader.Model
{
    public class Plc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public int Rack { get; set; }
        public int Slot { get; set; }
        public int Type { get; set; }
        public int ServerId { get; set; }
        public string ServerAddress { get; set; }
        public string CnnString { get; set; }
    }

    public class Sensor
    {
        public int SensorId { get; set; }
        public string Tag { get; set; }
        public int PlcId { get; set; }
        public int Db { get; set; }
        public string Address { get; set; }
        public double Deadband { get; set; }
        public int IsLine { get; set; }
        public int IdLine { get; set; }
        public string typeLine { get; set; }
    }

    public class Server
    {
        public int Id { get; set; }
        public string Address { get; set; }
    }

    public class TimerTag
    {
        public TimerTag()
        {
            Buffer = new byte[65536];
        }
        public Sharp7.S7Client Client { get; set; }
        public List<Sensor> sensorList { get; set; }
        public byte[] Buffer { get; set; }
        public int plcId { get; set; }
        public string cnnString { get; set; }
        public int plcCounter { get; set; }
        public SensorValueInsertModel sensorValue {get;set;}
        public List<SensorValueModel> counterValue2mList { get; set; }
        public List<LineStateInsertModel> lineStateList { get; set; }
    }

    [DataContract]
    public class SensorValueModel
    {
        [DataMember(Name = "c", Order = 1)]
        public int CounterId { get; set; }
        [DataMember(Name = "t", Order = 2)]
        public long Time { get; set; }
        [DataMember(Name = "v", Order = 3)]
        public double Value { get; set; }
    }

    public class SensorValueInsertModel
    {
        public string connectionStringName { get; set; }
        public List<SensorValueModel> counterValue { get; set; }
    }

    [DataContract]
    public class LineStateValueModel
    {
        [DataMember(Name = "tp", Order = 1)]
        public string typeInfo { get; set; } //1:"brand", 2:"lineState", 3:"lineMode"
        [DataMember(Name = "dt", Order = 2)]
        public long dtFrom { get; set; } //"2017-02-17 08:55:48.000"
        [DataMember(Name = "idL", Order = 3)]
        public int idLine { get; set; }
        [DataMember(Name = "idS", Order = 4)]
        public int idState { get; set; } //idBrandState, idLineState, idLineMode //зн-е приходит из PLC
    }

    [DataContract]
    public class LineStateInsertModel
    {
        [DataMember(Name = "cnn", Order = 1)]
        public string connectionStringName { get; set; } //"Org-Test-1"
        [DataMember(Name = "tp", Order = 2)]
        public string typeInfo { get; set; } //1:"brand", 2:"lineState", 3:"lineMode"
        [DataMember(Name = "dt", Order = 3)]
        public long dtFrom { get; set; } //"2017-02-17 08:55:48.000"
        [DataMember(Name = "idL", Order = 4)]
        public int idLine { get; set; }
        [DataMember(Name = "idS", Order = 5)]
        public int idState { get; set; } //idBrandState, idLineState, idLineMode //зн-е приходит из PLC
    }
}
