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
using System.Linq;
using System.Text;
using UnityEngine;

namespace HoudiniEngineUnity
{
    // Wrapper around Unity's Debug.Log to catch Houdini-specific errors and warnings better
    public class HEU_Logger
    {
        public static void Log(string text)
        {
            Debug.Log(text);
        }

        public static void LogFormat(string text, params object[] args)
        {
            Debug.LogFormat(text, args);
        }

        public static void LogWarning(string text)
        {
            Debug.LogWarning(text);
            LogToCookLogsIfOn(text);
        }

        public static void LogWarningFormat(string text, params object[] args)
        {
            Debug.LogWarningFormat(text, args);
            LogToCookLogsIfOnFormat(text, args);
        }

        public static void LogError(string text)
        {
            Debug.LogError(text);
            LogToCookLogsIfOn(text);
        }

        public static void LogErrorFormat(string text, params object[] args)
        {
            Debug.LogErrorFormat(text, args);
            LogToCookLogsIfOnFormat(text, args);
        }

        public static void LogError(System.Exception ex)
        {
            Debug.LogError(ex.ToString());
            LogToCookLogsIfOn(ex.ToString());
        }

        public static void LogAssertion(string text)
        {
            Debug.LogAssertion(text);
            LogToCookLogsIfOn(text);
        }

        public static void LogAssertionFormat(string text, params object[] args)
        {
            Debug.LogAssertionFormat(text, args);
            LogToCookLogsIfOnFormat(text, args);
        }

        private static void LogToCookLogsIfOn(string text)
        {
            if (HEU_PluginSettings.WriteCookLogs)
            {
                HEU_CookLogs.Instance.WriteToLogFile(text);
            }
        }

        private static void LogToCookLogsIfOnFormat(string text, params object[] args)
        {
            if (HEU_PluginSettings.WriteCookLogs)
            {
                text = string.Format(text, args);
                HEU_CookLogs.Instance.WriteToLogFile(text);
            }
        }
    }
}