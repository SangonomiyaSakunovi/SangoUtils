using System;
using System.Threading;

//Developer: SangonomiyaSakunovi

namespace SangoKCPNet
{
    public static class KCPLog
    {
        public static Action<string> LogInfoCallBack;
        public static Action<string> LogErrorCallBack;
        public static Action<string> LogWarningCallBack;

        public static void Info(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogInfoCallBack != null)
            {
                LogInfoCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.None);
            }
        }

        public static void Start(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogInfoCallBack != null)
            {
                LogInfoCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.Blue);
            }
        }

        public static void Special(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogInfoCallBack != null)
            {
                LogInfoCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.Magenta);
            }
        }

        public static void Done(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogInfoCallBack != null)
            {
                LogInfoCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.Green);
            }
        }

        public static void Processing(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogInfoCallBack != null)
            {
                LogInfoCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.Cyan);
            }
        }

        public static void Error(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogErrorCallBack != null)
            {
                LogErrorCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.Red);
            }
        }

        public static void Warning(string message, params object[] arguments)
        {
            message = string.Format(message, arguments);
            if (LogWarningCallBack != null)
            {
                LogWarningCallBack(message);
            }
            else
            {
                ConsoleLog(message, KCPLogColor.Yellow);
            }
        }

        private static void ConsoleLog(string message, KCPLogColor color)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            message = string.Format("Thread:{0} {1}", threadId, message);
            switch (color)
            {
                case KCPLogColor.None:
                    Console.WriteLine(message);
                    break;
                case KCPLogColor.Yellow:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Red:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Magenta:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KCPLogColor.Cyan:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    Console.WriteLine(message);
                    break;
            }
        }
    }
}
