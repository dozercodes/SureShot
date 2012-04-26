/************************************************************************************ 
 *
 * Microsoft XNA Community Game Platform
 * Copyright (C) Microsoft Corporation. All rights reserved.
 * 
 * ===================================================================================
 * Modified by: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Threading;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// Log will create automatically a log file and write log/warning/error info for simple
    /// runtime error checking, which is very useful for minor errors. The application can still 
    /// continue working, but this log provides an easy way to find errors. Also, you can
    /// enable WriteToNotifier to make the log message appear on the screen if State.ShowNotification
    /// is enabled.
    /// </summary>
    public sealed class Log
    {
        /// <summary>
        /// An enum that defines the severity of the log message.
        /// </summary>
        public enum LogLevel 
        { 
            /// <summary>
            /// Low level of severity. For minor debugging purposes.
            /// </summary>
            Log, 
            /// <summary>
            /// Middle level of severity. Not critical for executing the application, but
            /// the user should pay attention.
            /// </summary>
            Warning, 
            /// <summary>
            /// High level of severtiy. Critical for executing the application.
            /// </summary>
            Error 
        };

        #region Variables
        /// <summary>
        /// Writer
        /// </summary>
        private static StreamWriter writer = null;

        /// <summary>
        /// Log filename
        /// </summary>
        private static string LogFilename = "Log.txt";

        private static bool writeToNotifier = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Private constructor to prevent instantiation.
        /// </summary>
        private Log()
        {
        } // Log()
        #endregion

        #region Static constructor to create log file
        /// <summary>
        /// Static constructor
        /// </summary>
        static Log()
        {
            try
            {
                // Open file
                String filename = State.GetSettingVariable("LogFileName");
                if (filename != null && !filename.Equals(""))
                    LogFilename = filename;
                FileStream file = FileHelper.CreateGameContentFile(LogFilename, false);
                //old: new FileStream(
                //	LogFilename, FileMode.OpenOrCreate,
                //	FileAccess.Write, FileShare.ReadWrite);

                // Check if file is too big (more than 2 MB),
                // in this case we just kill it and create a new one :)
                if (file.Length > 2 * 1024 * 1024)
                {
                    file.Close();
                    file = FileHelper.CreateGameContentFile(LogFilename, false);
                    //old: file = new FileStream(
                    //	LogFilename, FileMode.Create,
                    //	FileAccess.Write, FileShare.ReadWrite );
                } // if (file.Length)
                // Associate writer with that, when writing to a new file,
                // make sure UTF-8 sign is written, else don't write it again!
                if (file.Length == 0)
                    writer = new StreamWriter(file,
                        System.Text.Encoding.UTF8);
                else
                    writer = new StreamWriter(file);

                // Go to end of file
                writer.BaseStream.Seek(0, SeekOrigin.End);

                // Enable auto flush (always be up to date when reading!)
                writer.AutoFlush = true;

                // Add some info about this session
                writer.WriteLine("");
#if WINDOWS_PHONE
                writer.WriteLine("/// Session started at: " + 
                    String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now));
#else
                writer.WriteLine("/// Session started at: " +
                    StringHelper.WriteIsoDateAndTime(DateTime.Now));
#endif
                writer.WriteLine("/// GoblinXNA");
                writer.WriteLine("");
            } // try
            catch (IOException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g., on a CD-Rom) it doesn't matter.
            } // catch
            catch (UnauthorizedAccessException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g., on a CD-Rom) it doesn't matter.
            } // catch
        } // Log()
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether to write to the screen notifier, as well. If this is set to true
        /// and State.ShowNotification is set to true, anything written to Log will be automatically
        /// shown in the screen as well.
        /// </summary>
        public static bool WriteToNotifier
        {
            get { return writeToNotifier; }
            set { writeToNotifier = true; }
        }
        #endregion

        #region Write log entry
        /// <summary>
        /// Writes a LogLevel.Warning text message to the Log file, as well as the screen, if enabled
        /// </summary>
        /// <param name="message">The log message</param>
        static public void Write(String message)
        {
            Write(message, LogLevel.Error);
        }

        /// <summary>
        /// Writes a log/warning/error text message to the Log file, as well as the screen, if enabled
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The log level</param>
        static public void Write(string message, LogLevel level)
        {
            if ((int)level < (int)State.LogPrintLevel)
                return;

            if(writeToNotifier)
                GoblinXNA.UI.Notifier.AddMessage(level.ToString() + ": " + message);

            // Can't continue without valid writer
            if (writer == null)
                return;

            try
            {
                DateTime ct = DateTime.Now;
                string s = "[" + ct.Hour.ToString("00") + ":" +
                    ct.Minute.ToString("00") + ":" +
                    ct.Second.ToString("00") + "] " + level.ToString() + ": " +
                    message;
                writer.WriteLine(s);
            } // try
            catch (IOException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g., on a CD-Rom) it doesn't matter.
            } // catch
            catch (UnauthorizedAccessException)
            {
                // Ignore any file exceptions, if file is not
                // createable (e.g., on a CD-Rom) it doesn't matter.
            } // catch
        } // Write(message)
        #endregion
    } // class Log
} 
