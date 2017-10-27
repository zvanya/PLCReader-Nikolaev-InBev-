using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
using JsonRequest;
using System.Web;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Runtime.InteropServices;
using Sharp7;
using PLCReader.Services;
using PLCReader.Model;

namespace PLCReader
{
    public partial class MainForm : Form
    {

        #region Fields (Global data)

        private readonly DateTimeService _dateTimeService;

        readonly string serverIIS = "http://192.168.71.127";
        readonly string cnnString = "counters_board_db";

        int dbNumber = 0;
        int dbSize = 0;

        S7Client client;// = new S7Client();

        byte[] Buffer;// = new byte[65536];

        List<int> timerStartSendCounter = new List<int>();

        Plc plc = new Plc();
        List<Sensor> sensorList = new List<Sensor>();

        SensorValueInsertModel sensorValueInsert;// = new SensorValueInsertModel();
        List<SensorValueModel> sensorValue2mList = new List<SensorValueModel>();

        List<LineStateInsertModel> lineStateInsertList = new List<LineStateInsertModel>();

        string strSQL = string.Empty;

        bool isDataExist = false;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            DBControl.OpenConnection();
            _dateTimeService = new DateTimeService();

            InitializeUserData();
        }

        private void InitializeUserData()
        {
            DataSet dsPLC = new DataSet();
            DataSet dsSensor = new DataSet();
            DataSet dsDB = new DataSet();

            strSQL = "SELECT id, name, ip, rack, slot, type, serverId, cnnString FROM plc WHERE id = 1";
            dsPLC = DBControl.Select(strSQL);

            if (dsPLC == null || dsPLC.Tables[0].Rows.Count == 0)
            {
                listBox1.Items.Add("В БД нет PLC");
            }
            else
            {
                foreach (DataRow p in dsPLC.Tables[0].Rows)
                {
                    plc.Id = Convert.ToInt32(p["id"].ToString());
                    plc.Name = p["name"].ToString().Trim();
                    plc.Ip = p["ip"].ToString().Trim();
                    plc.Rack = Convert.ToInt32(p["rack"].ToString());
                    plc.Slot = Convert.ToInt32(p["slot"].ToString());
                    plc.Type = Convert.ToInt32(p["type"].ToString());
                    plc.CnnString = p["cnnString"].ToString().Trim();
                }

                listBox1.Items.Add("PLC:[" + plc.Name + "] " + plc.Ip + "; cnn:" + plc.CnnString);

                strSQL = "SELECT sensor_id, tag, (SELECT number FROM datablock WHERE id = 1) as db, address, deadband, isLine, idLine, typeLine FROM sensor WHERE plc_id = 1";
                dsSensor = DBControl.Select(strSQL);

                if (dsSensor == null || dsSensor.Tables[0].Rows.Count == 0)
                {
                    listBox1.Items.Add("В БД нет сенсоров для PLC: " + plc.Name);
                }
                else
                {
                    foreach (DataRow s in dsSensor.Tables[0].Rows)
                    {
                        sensorList.Add(
                            new Model.Sensor
                            {
                                SensorId = Convert.ToInt32(s["sensor_id"].ToString()),
                                Tag = s["tag"].ToString().Trim(),
                                PlcId = plc.Id,
                                Db = Convert.ToInt32(s["db"].ToString()),
                                Address = s["address"].ToString().Trim(),
                                Deadband = Convert.ToDouble(s["deadband"].ToString()),
                                IsLine = Convert.ToInt32(s["isLine"].ToString()),
                                IdLine = Convert.ToInt32(s["idLine"].ToString()),
                                typeLine = s["typeLine"].ToString()
                            }
                            );
                    }

                    strSQL = "SELECT number, size FROM datablock WHERE id = 1";
                    dsDB = DBControl.Select(strSQL);

                    foreach (DataRow db in dsDB.Tables[0].Rows)
                    {
                        dbNumber = Convert.ToInt32(db["number"].ToString());
                        dbSize = Convert.ToInt32(db["size"].ToString());
                    }

                    sensorValueInsert = new SensorValueInsertModel()
                    {
                        connectionStringName = plc.CnnString.Trim(),
                        counterValue = new List<Model.SensorValueModel>()
                    };

                    sensorValue2mList.Add(new SensorValueModel());
                    lineStateInsertList.Add(new LineStateInsertModel());

                    client = new S7Client();

                    Buffer = new byte[65536];

                    isDataExist = true;
                }
            }
            dsPLC.Clear();
            dsPLC.Dispose();
            dsSensor.Clear();
            dsSensor.Dispose();
            dsDB.Clear();
            dsDB.Dispose();
        }

        #endregion

        #region Forms events

        #region Forms

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (isDataExist == true) ConnectBtn.PerformClick();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Закрыть программу?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DisconnectBtn.PerformClick();

                DBControl.CloseConnection();
            }
            else
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Connect/Disconnect buttons events

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            if (isDataExist == true)
            {
                int result = -1;
                result = client.ConnectTo(plc.Ip, plc.Rack, plc.Slot);

                if (result != 0)
                {
                    listBox1.Items.Add(DateTime.Now + ": Не удалось подключиться к PLC. IP:" + plc.Ip + " Rack:" + plc.Rack + " Slot:" + plc.Slot);
                }
                else
                {
                    ConnectBtn.Enabled = false;
                    DisconnectBtn.Enabled = true;

                    timerPlcDataPolling.Enabled = true;
                }

                timerSendDataToPlc.Enabled = true;
                timerPlcConnectionCheck.Enabled = true;
            }
        }

        private void DisconnectBtn_Click(object sender, EventArgs e)
        {
            client.Disconnect();

            ConnectBtn.Enabled = true;
            DisconnectBtn.Enabled = false;

            timerPlcDataPolling.Enabled = false;
            timerSendDataToPlc.Enabled = false;
            timerPlcConnectionCheck.Enabled = false;
        }

        #endregion

        #endregion

        #region Timers event

        private void timerPlcConnectionCheck_Tick(object sender, EventArgs e)
        {
            if (!client.Connected)
            {
                int result = client.ConnectTo(plc.Ip, plc.Rack, plc.Slot);

                if (result != 0)
                {
                    timerPlcDataPolling.Enabled = false;
                    listBox1.Items.Add(DateTime.Now + ": Не удалось подключиться к PLC. IP:" + plc.Ip + " Rack:" + plc.Rack + " Slot:" + plc.Slot);
                }
                else
                {
                    if (!timerPlcDataPolling.Enabled)
                    {
                        timerPlcDataPolling.Enabled = true;
                    }
                }
            }
        }

        private void timerSendDataToPlc_Tick(object sender, EventArgs e)
        {
            string urlLineState = serverIIS + ":9030/write/line-state";
            string urlSensorValue = serverIIS + ":9030/values";

            if (!client.Connected)
            {
                listBox1.Items.Add(DateTime.Now + ": Отправка данных (line-state) на сервер. Нет подключения к PLC. IP:" + plc.Ip + " Rack:" + plc.Rack + " Slot:" + plc.Slot);
            }
            else
            {
                if (lineStateInsertList.Count > 0)
                {
                    var request = new Request();

                    bool lineStateListInsertOk = true;

                    try
                    {
                        request.Execute(urlLineState, lineStateInsertList, "POST");
                    }
                    catch (Exception ex)
                    {
                        listBox1.Items.Add(DateTime.Now + ": Ошибка отправки данных (line-state) на сервер: " + ex.Message);
                        lineStateListInsertOk = false;
                    }

                    if (lineStateListInsertOk)
                    {
                        listBox1.Items.Add(DateTime.Now + ": Отправка данных (line-state) на сервер. PLC IP:" + plc.Ip + " Rack:" + plc.Rack + " Slot:" + plc.Slot);
                        lineStateInsertList.Clear();
                    }
                }

                if (sensorValueInsert.counterValue.Count > 0)
                {
                    var request = new Request();

                    bool sensorValueInsertOk = true;

                    try
                    {
                        request.Execute(urlSensorValue, sensorValueInsert, "POST");
                    }
                    catch (Exception ex)
                    {
                        listBox1.Items.Add(DateTime.Now + ": Ошибка отправки данных (line-state) на сервер: " + ex.Message);
                        sensorValueInsertOk = false;
                    }

                    if (sensorValueInsertOk)
                    {
                        listBox1.Items.Add(DateTime.Now + ": Отправка данных (values) на сервер. PLC IP:" + plc.Ip + " Rack:" + plc.Rack + " Slot:" + plc.Slot);
                        sensorValueInsert.counterValue.Clear();
                    }
                }
            }
        }

        private void timerPlcDataPolling_Tick(object sender, EventArgs e)
        {
            if (!ReadArea(client, dbNumber, dbSize, Buffer))
            {
                listBox1.Items.Add("Ошибка чтения DB. PLC:" + plc.Id + "; DB:" + dbNumber + "; dbSize:" + dbSize);
            }
            else
            {
                foreach (Sensor s in sensorList)
                {
                    int S7Type = 0;
                    int Pos = 0;
                    if (!GetAddress(s.Address, ref S7Type, ref Pos))
                    {
                        //Переход к следующему элементу в списке сенсоров sensorList
                        listBox1.Items.Add("Ошибка парсинга адреса. PLC:" + plc.Id + "; address:" + s.Address);
                        continue;
                    }
                    else
                    {
                        string txtValue = string.Empty;
                        if (!GetValue(S7Type, Pos, Buffer, ref txtValue))
                        {
                            //Переход к следующему элементу в списке сенсоров sensorList
                            listBox1.Items.Add("Ошибка чтения зн-я по адресу. PLC:" + plc.Id + "; address:" + s.Address);
                            continue;
                        }
                        else //! Успешное чтение зн-я переменной
                        {
                            double value = 0;

                            if (txtValue == "False") value = 0.0;
                            else if (txtValue == "True") value = 1.0;
                            else value = double.Parse(txtValue);

                            //Последняя запись для данного sensor.id в текущем json (2-х минутном). Вычисляется ниже
                            SensorValueModel lastCounterValue2m = new SensorValueModel() { CounterId = -1 };

                            double diffValue = 0;

                            if(s.IsLine == 1)
                            {
                                lineStateInsertList.Add(
                                    new LineStateInsertModel()
                                    {
                                        connectionStringName = cnnString,
                                        dtFrom = _dateTimeService.UnixTimeNow(),
                                        idLine = s.IdLine,
                                        idState = Int32.Parse(value.ToString()),
                                        typeInfo = s.typeLine.Trim()
                                    });
                            }
                            else if (s.IsLine == 0)
                            {
                                if (sensorValue2mList.Where(x => x.CounterId == s.SensorId).Count() > 0)
                                {//Если запись присутствует в коллекции, которая набивается в течение 2мин., то сравнить текущее зн-е с последним значением этого id из коллекции
                                 //если оно больше или равно deadband этого сенсора, то внести его в коллекции 2мин и 20сек /json/ для отправки на сервер

                                    //Удаление из коллекции counterValue2mList элементов старше 2-х минут
                                    //((sender as Timer).Tag as Model.PlcData).counterValue2mList.RemoveAll(x => (DateTime.UtcNow - _dateTimeService.UnixTimeToDateTime(x.Time)).Seconds >= 120); //удобнее, но в 10 раз дольше
                                    //Циклом удаление элемента из коллекции в 10 раз быстрее //http://www.dotnetblog.ru/2014/03/blog-post_23.html
                                    for (int i = sensorValue2mList.Count - 1; i >= 0; i--)
                                    {
                                        if ((DateTime.UtcNow - _dateTimeService.UnixTimeToDateTime(sensorValue2mList[i].Time)).TotalSeconds >= 120)
                                        {
                                            sensorValue2mList.Remove(sensorValue2mList[i]);
                                        }
                                    }

                                    if (sensorValue2mList.Where(x => x.CounterId == s.SensorId).Count() > 0)
                                    {
                                        //Выбрать последнюю запись для данного сенсора из коллекции 2мин
                                        lastCounterValue2m = sensorValue2mList.Where(x => x.CounterId == s.SensorId).Last();

                                        diffValue = Math.Abs(value - lastCounterValue2m.Value);

                                        if (diffValue >= s.Deadband)
                                        {
                                            //Вставка в коллекцию 20сек для отправки на сервер
                                            sensorValueInsert.counterValue.Add(
                                                new Model.SensorValueModel()
                                                {
                                                    CounterId = s.SensorId,
                                                    Value = value,
                                                    Time = _dateTimeService.UnixTimeNow()
                                                }
                                                );

                                            //Вставка в коллекцию 2мин
                                            sensorValue2mList.Add(
                                                new Model.SensorValueModel()
                                                {
                                                    CounterId = s.SensorId,
                                                    Value = value,
                                                    Time = _dateTimeService.UnixTimeNow()
                                                }
                                                );
                                        }
                                    }
                                    else //Если записи для этого сенсора, после удаления старых зн-й >2мин, нет в коллекции 2мин.
                                    {    //то вставить в коллекции 2мин и 20сек /json/ для отправки на сервер

                                        //Вставка в коллекцию 20сек для отправки на сервер
                                        sensorValueInsert.counterValue.Add(
                                            new Model.SensorValueModel()
                                            {
                                                CounterId = s.SensorId,
                                                Value = value,
                                                Time = _dateTimeService.UnixTimeNow()
                                            }
                                            );

                                        //Вставка в коллекцию 2мин
                                        sensorValue2mList.Add(
                                            new Model.SensorValueModel()
                                            {
                                                CounterId = s.SensorId,
                                                Value = value,
                                                Time = _dateTimeService.UnixTimeNow()
                                            }
                                            );
                                    }
                                }
                                else //Если записи для этого сенсора нет в коллекции 2мин.
                                {    //то вставить в коллекции 2мин и 20сек /json/ для отправки на сервер

                                    //Вставка в коллекцию 20сек для отправки на сервер
                                    sensorValueInsert.counterValue.Add(
                                        new Model.SensorValueModel()
                                        {
                                            CounterId = s.SensorId,
                                            Value = value,
                                            Time = _dateTimeService.UnixTimeNow()
                                        }
                                        );

                                    //Вставка в коллекцию 2мин
                                    sensorValue2mList.Add(
                                        new Model.SensorValueModel()
                                        {
                                            CounterId = s.SensorId,
                                            Value = value,
                                            Time = _dateTimeService.UnixTimeNow()
                                        }
                                        );
                                }
                            }
                            lastCounterValue2m = null;
                        }
                    }
                }
            }

            listBox1.SelectedIndex = listBox1.Items.Count - 1;

            //TODO: очистить Buffer
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        #endregion

        #region S7 PLC reader

        private bool ReadArea(S7Client Client, int DBNumber, int ByteCount, byte[] Buffer)
        {
            // Declaration separated from the code for readability
            int Amount = ByteCount;
            int SizeRead = 0;
            int Result;
            int[] Area =
            {
                 S7Consts.S7AreaPE,
                 S7Consts.S7AreaPA,
                 S7Consts.S7AreaMK,
                 S7Consts.S7AreaDB,
                 S7Consts.S7AreaCT,
                 S7Consts.S7AreaTM
            };
            int[] WordLen =
            {
                 S7Consts.S7WLBit,
                 S7Consts.S7WLByte,
                 S7Consts.S7WLChar,
                 S7Consts.S7WLWord,
                 S7Consts.S7WLInt,
                 S7Consts.S7WLDWord,
                 S7Consts.S7WLDInt,
                 S7Consts.S7WLReal,
                 S7Consts.S7WLCounter,
                 S7Consts.S7WLTimer
            };

            Result = Client.ReadArea(Area[3], DBNumber, 0, Amount, WordLen[1], Buffer, ref SizeRead);

            if (Result != 0) return false;

            //ShowResult(Client, Result);

            return true;
        }

        private void ReadDB(S7Client Client, int DBNumber, byte[] Buffer) //не работает:(
        {
            int SizeRead = 65536;
            int Result;

            Result = Client.DBGet(DBNumber, Buffer, ref SizeRead);

            //ShowResult(Client, Result);
        }

        private bool GetValue(int S7Type, int Pos, byte[] Buffer, ref string TxtValue)
        {
            /*
            0 Byte    8 Bit Word                     (All)
            1 Word   16 Bit Word                     (All)
            2 DWord  32 Bit Word                     (All)
            3 LWord  64 Bit Word                     (S71500)
            4 USint   8 Bit Unsigned Integer         (S71200/1500)
            5 UInt   16 Bit Unsigned Integer         (S71200/1500)
            6 UDInt  32 Bit Unsigned Integer         (S71200/1500)
            7 ULint  64 Bit Unsigned Integer         (S71500)
            8 Sint    8 Bit Signed Integer           (S71200/1500)
            9 Int    16 Bit Signed Integer           (All)
            10 DInt   32 Bit Signed Integer           (S71200/1500)
            11 LInt   64 Bit Signed Integer           (S71500)
            12 Real   32 Bit Floating point           (All)
            13 LReal  64 Bit Floating point           (S71200/1500)
            14 Time   32 Bit Time elapsed ms          (All)
            15 LTime  64 Bit Time Elapsed ns          (S71500)
            16 Date   16 Bit days from 1990/1/1       (All)
            17 TOD    32 Bit ms elapsed from midnight (All)
            18 DT      8 Byte Date and Time           (All)
            19 LTOD   64 Bit time of day (ns)         (S71500)
            20 DTL    12 Byte Date and Time Long      (S71200/1500)
            21 LDT    64 Bit ns elapsed from 1970/1/1 (S71500)
            22 Bit
            */


            //When WordLen = S7WLBit the Offset(Start) must be expressed in bits.Ex.The Start for DB4.DBX 10.3 is (10 * 8) + 3 = 83.

            switch (S7Type)
            {
                case 0:
                    {
                        TxtValue = System.Convert.ToString(Buffer[Pos], 10).ToUpper();
                        break;
                    }
                case 1:
                    {
                        UInt16 Word = S7.GetWordAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(Word, 10).ToUpper();
                        break;
                    }
                case 2:
                    {
                        UInt32 DWord = S7.GetDWordAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(DWord, 10).ToUpper();
                        break;
                    }
                case 3:
                    {
                        UInt64 LWord = S7.GetLWordAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString((Int64)LWord, 10).ToUpper(); // <-- Convert.ToString does not handle UInt64
                        break;
                    }
                case 4:
                    {
                        UInt16 USInt = S7.GetUSIntAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(USInt);
                        break;
                    }
                case 5:
                    {
                        UInt16 UInt = S7.GetUIntAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(UInt);
                        break;
                    }
                case 6:
                    {
                        UInt32 UDInt = S7.GetDWordAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(UDInt);
                        break;
                    }
                case 7:
                    {
                        UInt64 ULInt = S7.GetLWordAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(ULInt);
                        break;
                    }
                case 8:
                    {
                        int SInt = S7.GetSIntAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(SInt);
                        break;
                    }
                case 9:
                    {
                        int S7Int = S7.GetIntAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(S7Int);
                        break;
                    }
                case 10:
                    {
                        int DInt = S7.GetDIntAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(DInt);
                        break;
                    }
                case 11:
                    {
                        Int64 LInt = S7.GetLIntAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(LInt);
                        break;
                    }
                case 12:
                    {
                        Single S7Real = S7.GetRealAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(S7Real);
                        break;
                    }
                case 13:
                    {
                        Double S7LReal = S7.GetLRealAt(Buffer, Pos);
                        TxtValue = System.Convert.ToString(S7LReal);
                        break;
                    }
                case 14:
                    {
                        Int32 TimeElapsed = S7.GetDIntAt(Buffer, Pos);
                        // TIME type is a 32 signed number of ms elapsed
                        // Can be added to a DateTime or used as Value.
                        TxtValue = System.Convert.ToString(TimeElapsed) + "MS";
                        break;
                    }
                case 15:
                    {
                        Int64 TimeElapsed = S7.GetLIntAt(Buffer, Pos);
                        // LTIME type is a 64 signed number of ns elapsed
                        // Can be added (after a conversion) to a DateTime or used as Value.
                        TxtValue = System.Convert.ToString(TimeElapsed) + "NS";
                        break;
                    }
                case 16:
                    {
                        DateTime DATE = S7.GetDateAt(Buffer, Pos);
                        TxtValue = DATE.ToString("D#yyyy-MM-dd");
                        break;
                    }
                case 17:
                    {
                        DateTime TOD = S7.GetTODAt(Buffer, Pos);
                        TxtValue = TOD.ToString("TOD#HH:mm:ss.fff");
                        break;
                    }
                case 18:
                    {
                        DateTime DT = S7.GetDateTimeAt(Buffer, Pos);
                        TxtValue = DT.ToString("DT#yyyy-MM-dd-HH:mm:ss.fff");
                        break;
                    }
                case 19:
                    {
                        DateTime LTOD = S7.GetLTODAt(Buffer, Pos);
                        TxtValue = LTOD.ToString("LTOD#HH:mm:ss.fffffff");
                        break;
                    }
                case 20:
                    {
                        DateTime DTL = S7.GetDTLAt(Buffer, Pos);
                        TxtValue = DTL.ToString("DTL#yyyy-MM-dd-HH:mm:ss.fffffff");
                        break;
                    }
                case 21:
                    {
                        DateTime LDT = S7.GetLDTAt(Buffer, Pos);
                        TxtValue = LDT.ToString("LDT#yyyy-MM-dd-HH:mm:ss.fffffff");
                        break;
                    }
                case 22:
                    {
                        bool bit = S7.GetBitAt(Buffer, Pos, 0);
                        TxtValue = bit.ToString();
                        break;
                    }
                default:
                    return false;
            }

            return true;
        }

        #endregion

        #region Helpers

        private void ShowResult(S7Client Client, int Result)
        {
            if (Result == 0)
            {
                listBox1.Items.Add(Client.ErrorText(Result) + " (" + Client.ExecutionTime.ToString() + " ms)");
            }
            else
            {
                listBox1.Items.Add(Client.ErrorText(Result));
                Logger.WriteEventLog("Application", "S7 PLC Read", Client.ErrorText(Result), EventLogEntryType.Error);
            }
        }

        private bool GetAddress(string address, ref int S7Type, ref int Pos)
        {
            /*
            0 Byte    8 Bit Word                     (All)
            1 Word   16 Bit Word                     (All)
            2 DWord  32 Bit Word                     (All)
            3 LWord  64 Bit Word                     (S71500)
            4 USint   8 Bit Unsigned Integer         (S71200/1500)
            5 UInt   16 Bit Unsigned Integer         (S71200/1500)
            6 UDInt  32 Bit Unsigned Integer         (S71200/1500)
            7 ULint  64 Bit Unsigned Integer         (S71500)
            8 Sint    8 Bit Signed Integer           (S71200/1500)
            9 Int    16 Bit Signed Integer           (All)
            10 DInt   32 Bit Signed Integer           (S71200/1500)
            11 LInt   64 Bit Signed Integer           (S71500)
            12 Real   32 Bit Floating point           (All)
            13 LReal  64 Bit Floating point           (S71200/1500)
            14 Time   32 Bit Time elapsed ms          (All)
            15 LTime  64 Bit Time Elapsed ns          (S71500)
            16 Date   16 Bit days from 1990/1/1       (All)
            17 TOD    32 Bit ms elapsed from midnight (All)
            18 DT      8 Byte Date and Time           (All)
            19 LTOD   64 Bit time of day (ns)         (S71500)
            20 DTL    12 Byte Date and Time Long      (S71200/1500)
            21 LDT    64 Bit ns elapsed from 1970/1/1 (S71500)
            22 Bit     1 Bit Word                     (All)
            */

            //When WordLen = S7WLBit the Offset(Start) must be expressed in bits.Ex.The Start for DB4.DBX 10.3 is (10 * 8) + 3 = 83.

            string sType = string.Empty;
            //Вырезать буквенное обозначение типа переменной из строки address и сохранить в sType
            int value1 = 0;
            int k = 0;
            for (int i = 0; i < address.Length; i++)
            {
                if (Int32.TryParse(address.Substring(i, 1), out value1))
                {
                    k = i;
                    break;
                }

                sType = address.Substring(0, i + 1);
            }

            if (sType != "DBX")
            {
                int value2 = 0;

                if (!Int32.TryParse(address.Substring(k, address.Length - k), out value2)) return false;

                Pos = value2;
            }
            else
            {
                //Ex.The Start for DBX10.3 is (10 * 8) + 3 = 83.
                string temp = address.Substring(k, address.Length - k); //temp == 10.3

                int value3 = 0;
                int value4 = 0;

                if (!Int32.TryParse(temp.Substring(0, temp.IndexOf('.')), out value3)) return false;
                if (!Int32.TryParse(temp.Substring(temp.IndexOf('.') + 1, temp.Length - temp.IndexOf('.') - 1), out value4)) return false;

                Pos = value3 * 8 + value4;
            }


            int type = -1;

            switch (sType)
            {
                case "DBB":
                    {
                        type = 0;
                        break;
                    }
                case "DBW":
                    {
                        type = 1;
                        break;
                    }
                case "DDW":
                    {
                        type = 2;
                        break;
                    }
                case "DLW":
                    {
                        type = 3;
                        break;
                    }
                case "DBUI":
                    {
                        type = 4;
                        break;
                    }
                case "DWUI":
                    {
                        type = 5;
                        break;
                    }
                case "DDUI":
                    {
                        type = 6;
                        break;
                    }
                case "DLUI":
                    {
                        type = 7;
                        break;
                    }
                case "DBSI":
                    {
                        type = 8;
                        break;
                    }
                case "DWSI":
                    {
                        type = 9;
                        break;
                    }
                case "DDSI":
                    {
                        type = 10;
                        break;
                    }
                case "DLSI":
                    {
                        type = 11;
                        break;
                    }
                case "DBD":
                    {
                        type = 12;
                        break;
                    }
                case "DLD":
                    {
                        type = 13;
                        break;
                    }
                case "DTime":
                    {
                        type = 14;
                        break;
                    }
                case "DDate":
                    {
                        type = 16;
                        break;
                    }
                case "DTOD":
                    {
                        type = 17;
                        break;
                    }
                case "DBX":
                    {
                        type = 22;
                        break;
                    }
                default:
                    return false;
            }

            S7Type = type;

            return true;
        }


        #endregion

    }
}
