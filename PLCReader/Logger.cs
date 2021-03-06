﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;

namespace PLCReader
{
    /// <summary>
    /// Логгер
    /// </summary>
    public class Logger
    {
        //Logger.WriteEventLog("Application", "CDATA", "test message", EventLogEntryType.Information);

        /// <summary>
        /// Запись в EventLog
        /// </summary>
        /// <param name="eventLogName">Название лога</param>
        /// <param name="sourceName">Название источника</param>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип сообщения</param>
        public static void WriteEventLog(string eventLogName, string sourceName, string message, EventLogEntryType type)
        {
            try
            {
                EventLog log = new EventLog { Log = eventLogName };
                if (string.IsNullOrEmpty(sourceName))
                {
                    sourceName = eventLogName;
                }
                log.Source = sourceName;

                try
                {
                    log.WriteEntry(message, type, 0, 0);
                    return;
                }
                catch
                {

                }

                // Создание логгера и источника

                byte[] rawEventData = Encoding.ASCII.GetBytes("");
                string keyName = @"SYSTEM\CurrentControlSet\Services\EventLog\" + eventLogName;

                var rkEventSource = Registry.LocalMachine.OpenSubKey(keyName + @"\" + sourceName);
                if (rkEventSource == null)
                {
                    rkEventSource = Registry.LocalMachine.CreateSubKey(keyName + @"\" + sourceName);
                }

                object eventMessageFile = rkEventSource.GetValue("EventMessageFile");
                if (eventMessageFile == null)
                {
                    using (var dotNetFrameworkSettings = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NetFramework\"))
                    {
                        if (dotNetFrameworkSettings != null)
                        {
                            object dotNetInstallRoot = dotNetFrameworkSettings.GetValue("InstallRoot", null, RegistryValueOptions.None);

                            if (dotNetInstallRoot != null)
                            {
                                string eventMessageFileLocation =
                                    dotNetInstallRoot.ToString() +
                                    "v" +
                                    System.Environment.Version.Major.ToString() + "." +
                                    System.Environment.Version.Minor.ToString() + "." +
                                    System.Environment.Version.Build.ToString() +
                                    @"\EventLogMessages.dll";

                                if (File.Exists(eventMessageFileLocation))
                                {
                                    rkEventSource = Registry.LocalMachine.OpenSubKey(keyName + @"\" + sourceName, true);
                                    rkEventSource.SetValue("EventMessageFile", eventMessageFileLocation, RegistryValueKind.String);
                                }
                            }
                        }
                    }
                }

                rkEventSource.Close();
                log.WriteEntry(message, type, 0, 0, rawEventData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message);
            }
        }
    }
}
