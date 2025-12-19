/*
 * Copyright (c) <2020> Side Effects Software Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * 2. The name of Side Effects Software may not be used to endorse or
 *    promote products derived from this software without specific prior
 *    written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY SIDE EFFECTS SOFTWARE "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN
 * NO EVENT SHALL SIDE EFFECTS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HoudiniEngineUnity
{
    public class HEU_CookLogs
    {
        private static HEU_CookLogs _instance;

        public static HEU_CookLogs Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    _instance = new HEU_CookLogs();
                    return _instance;
                }
            }
        }

        private StringBuilder _cookLogs = new StringBuilder();
        private int _currentCookLogCount = 0;
        private const int MAX_COOK_LOG_COUNT = 9001;

        private string _lastLogStr = "";

        private bool _uniqueStrOnly = true;

        public string GetCookLogString()
        {
            return _cookLogs.ToString();
        }

        public const long MaxLogSize = 50 * 1000 * 1000; // 50 MB

        public void AppendCookLog(string logStr)
        {
            if (!HEU_PluginSettings.WriteCookLogs)
            {
                return;
            }

            if (_uniqueStrOnly && logStr == _lastLogStr)
            {
                return;
            }

            if (_currentCookLogCount == MAX_COOK_LOG_COUNT)
            {
                string cur = _cookLogs.ToString();
                int newLine = cur.IndexOf('\n');
                cur = cur.Substring(newLine);
                _cookLogs.Remove(0, newLine + 1);
                _cookLogs.AppendLine(logStr);
            }
            else
            {
                _cookLogs.AppendLine(logStr);
                _currentCookLogCount++;
            }

            WriteToLogFile(logStr, false);

            if (_uniqueStrOnly)
            {
                _lastLogStr = logStr;
            }
        }

        public void ClearCookLog()
        {
            _cookLogs = new StringBuilder();
            _currentCookLogCount = 0;
        }

        public string GetCookLogFilePath()
        {
            return System.IO.Path.Combine(Application.dataPath, ".." + System.IO.Path.DirectorySeparatorChar + HEU_Defines.COOK_LOGS_FILE);
        }

        public void DeleteCookingFile()
        {
            string filePath = HEU_CookLogs.Instance.GetCookLogFilePath();
            FileInfo fi = new FileInfo(filePath);
            fi.Delete();
            fi.Refresh();
        }

        public void WriteToLogFile(string logStr, bool checkLastLogStr = true)
        {
            if (_uniqueStrOnly && checkLastLogStr && logStr == _lastLogStr)
            {
                return;
            }

            if (GetFileSizeOfLogFile() > MaxLogSize)
            {
                Debug.LogWarning("Deleting cook log file because it is taking too much space!");
                DeleteCookingFile();
            }

            string filePath = GetCookLogFilePath();
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, true))
            {
                writer.WriteLine(logStr);
            }

            if (_uniqueStrOnly && checkLastLogStr)
            {
                _lastLogStr = logStr;
            }
        }

        public long GetFileSizeOfLogFile()
        {
            string filePath = GetCookLogFilePath();
            if (!File.Exists(filePath))
            {
                return 0;
            }

            FileInfo fi = new FileInfo(filePath);
            return fi.Length;
        }
    }
}