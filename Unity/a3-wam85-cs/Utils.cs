using System;
using System.Diagnostics;

namespace Splendor
{
    public static class Utils
    {
        public static bool IsTestMode { get; set; } = false;

        public static void RaiseNotDefined()
        {
            var stackFrame = new StackFrame(1);
            var method = stackFrame.GetMethod();
            var fileName = stackFrame.GetFileName();
            var lineNumber = stackFrame.GetFileLineNumber();

            Console.WriteLine($"*** Method not implemented: {method?.Name} at line {lineNumber} of {fileName}");
            
            if (!IsTestMode)
            {
                Environment.Exit(1);
            }
            else
            {
                throw new NotImplementedException($"Method {method?.Name} not implemented");
            }
        }
    }
} 