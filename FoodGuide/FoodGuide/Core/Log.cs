using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.SharePoint;

namespace FoodGuide.Core
{
    public class DebugTextWriter : System.IO.TextWriter
    {
        public override void Write(char[] buffer, int index, int count)
        {
            Log.Info(new String(buffer, index, count));
        }

        public override void Write(string value)
        {
            Log.Info(value);
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }

    public class Log
    {
        public static DebugTextWriter TextWriter = new DebugTextWriter();

        public static readonly bool Enabled = true;

        public static readonly string LogFile = @"C:\temp\ContentStore.txt";

        public static readonly int LogFileThreshhold = 1024 * 1024 * 5;

        private static List<string> LogList;

        private static int LogListThreshhold = 10000;

        private static Thread LogThread;

        private const int DumpTimeoutMs = 10000;

        private const int RollLogfileCount = 10;

        private static bool LogVerbose;

        public static void SetLogVerbose(bool logVerbose)
        {
            LogVerbose = logVerbose;
        }

        static Log()
        {
            LogThread = new Thread(ThreadDumpLogList);
            LogThread.IsBackground = true; // background: thread end when process ends. Otherwise thread will keep the process up and running
            LogThread.Start();
            LogList = new List<string>();
        }


        public static void Verbose(string message)
        {
            if (LogVerbose)
            {
                Write("VERBOSE      ", message, null);
            }
        }

        public static void Debug(string message)
        {
            Write("DEBUG        ", message, null);
        }

        public static void Info(string message)
        {
            Write("INFO         ", message, null);
        }

        public static void Warn(string message)
        {
            Write("WARN         ", message, null);
        }

        public static void Maintenance(string message)
        {
            Write("MAINTENANCE       ", message, null);
        }

        public static void Scope(string message)
        {
            Write("SCOPE             ", message, null);
        }

        public static void MaintenanceError(string message)
        {
            Write("MAINTENANCE ERROR ", message, null);
        }

        public static void MaintenanceError(string message, Exception ex)
        {
            Write("MAINTENANCE ERROR ", message, ex);
        }

        public static void Error(string message)
        {
            Error(message, null);
        }

        public static void Error(string message, Exception ex)
        {
            Write("ERROR        ", message, ex);
        }

        public static void ErrorVerbose(string message)
        {
            ErrorVerbose(message, null);
        }
        public static void ErrorVerbose(string message, Exception ex)
        {
            if (LogVerbose)
            {
                Write("VERBOSE ERROR", message, ex);
            }
        }


        public static void Stopwatch(string message, Stopwatch sw)
        {
            Stopwatch(message, sw, true);
        }

        public static void Stopwatch(string message, Stopwatch sw, bool stopStopwatch)
        {
            if (stopStopwatch) sw.Stop();

            TimeSpan ts = sw.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            Write("DEBUG STOPWATCH ", "Elapsed Time: " + elapsedTime + ", " + message, null);
        }



        protected static void Write(string type, string message, Exception ex)
        {
            if (Enabled)
            {
                try
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    string loginName = (SPContext.Current != null) ? SPContext.Current.Web.CurrentUser.LoginName : "n/a";
                    string logString = DateTime.Now + " (" + threadId + (")").PadRight(3 - threadId.ToString().Length, ' ') + " (" + loginName + ") " + type + "=> " + message + " " +
                        (ex != null ? ex.Message + ": " + ex.StackTrace : "") + Environment.NewLine;

                    lock (LogList)
                    {
                        if (LogList.Count > LogListThreshhold)
                        {
                            // to many items in loglist. Clear current log and add message
                            LogList.Clear();
                            LogList.Add("WARNING: LogList threshhold exceeded. clearing current loglist.");
                        }
                        LogList.Add(logString);
                    }

                }
                catch (Exception)
                {

                }
            }
        }

        protected static void ThreadDumpLogList()
        {
            while (true)
            {
                try
                {
                    lock (LogFile)
                    {
                        DumpLogList();
                    }
                }
                catch (ThreadAbortException tae)
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    File.AppendAllText(LogFile,
                                       DateTime.Now + " (" + threadId + (")").PadRight(3 - threadId.ToString().Length, ' ') + " " + "WARN" + "=> " +
                                       "ThreadDumpLogList(): Logging thread has been aborted. Writing remaining logs..." + " " + Environment.NewLine);
                    DumpLogList();
                    File.AppendAllText(LogFile,
                                       DateTime.Now + " (" + threadId + (")").PadRight(3 - threadId.ToString().Length, ' ') + " " + "WARN" + "=> " +
                                       "ThreadDumpLogList(): Done writing remaining logs." + " " + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.AppendAllText(LogFile, DateTime.Now + " (" + Thread.CurrentThread.ManagedThreadId + ") " + "WARN" + "=> " + "General error in log file handler thread" + ex.Message +
                                           Environment.NewLine);
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    try
                    {
                        Thread.Sleep(DumpTimeoutMs);
                    }
                    catch (ThreadAbortException tae)
                    {
                        File.AppendAllText(LogFile,
                                           DateTime.Now + " (" + Thread.CurrentThread.ManagedThreadId + ") " + "WARN" + "=> " +
                                           "ThreadDumpLogList(): Logging thread has been aborted. Writing remaining logs..." + " " +
                                           Environment.NewLine);
                        DumpLogList();
                        File.AppendAllText(LogFile,
                                           DateTime.Now + " (" + Thread.CurrentThread.ManagedThreadId + ") " + "WARN" + "=> " +
                                           "ThreadDumpLogList(): Done writing remaining logs." + " " + Environment.NewLine);
                    }
                }
            }
        }

        private static void DumpLogList()
        {
            FileInfo fi = new FileInfo(LogFile);
            int logFileThreshhold = LogFileThreshhold;
            if (fi.Exists && fi.Length > logFileThreshhold)
            {
                fi.MoveTo(fi.FullName + DateTime.Now.ToFileTime() + fi.Extension);
            }

            try
            {
                // additional validation checks for empty name etc. 
                if (fi.Directory != null && fi.Name != null && fi.Name.Length > 5 && !string.IsNullOrEmpty(fi.Extension))
                {
                    FileSystemInfo[] files = fi.Directory.GetFileSystemInfos(fi.Name + "*" + fi.Extension);
                    IEnumerable<FileSystemInfo> orderedFiles = files.OrderByDescending(f => f.LastWriteTime).Skip(RollLogfileCount);
                    foreach (FileSystemInfo fileSystemInfo in orderedFiles)
                    {
                        fileSystemInfo.Delete();
                        Debug("Deleted Logfile " + fileSystemInfo.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Error("Could not Roll Logfiles", ex);
            }

            lock (LogList)
            {
                if (LogList.Count != 0)
                {
                    File.AppendAllText(LogFile, string.Join("", LogList.ToArray()));
                    LogList.Clear();
                }
            }
        }
    }
}
