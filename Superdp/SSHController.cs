﻿using System.Dynamic;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32.SafeHandles;
using static Superdp.Native;

namespace Superdp
{

    unsafe internal class SshController : IDisposable
    {
        //const int writeBufferSize = 10 * 1024 * 1024; // 10 MiB
        const int writeBufferSize = 1 * 1 * 1024; //  1 kiB

        // An Int32 that store how many bytes have been written
        const int sharedBufferSize = writeBufferSize + 4;

        readonly CoreWebView2SharedBuffer sharedBuffer;

        public HeroForm OwningForm { get; private set; }

        readonly PseudoConsolePipe inputPipe, outputPipe;

        readonly PseudoConsole pseudoCons;
        Process? sshProc;
        private bool disposedValue;

        readonly byte* bufptr;

        public string TabId { get; private set; }


        public SshController(HeroForm owningForm, string tabId)
        {
            OwningForm = owningForm;
            TabId = tabId;

            inputPipe = new PseudoConsolePipe();
            outputPipe = new PseudoConsolePipe();

            sharedBuffer = OwningForm.webView.CoreWebView2.Environment.CreateSharedBuffer(sharedBufferSize);
            bufptr = (byte*)(sharedBuffer.Buffer.ToPointer());

            dynamic additionalData = new ExpandoObject();
            additionalData.tabId = TabId;
            additionalData.displayBufferSize = writeBufferSize;
            OwningForm.webView.CoreWebView2.PostSharedBufferToScript(sharedBuffer, CoreWebView2SharedBufferAccess.ReadOnly, JsonSerializer.Serialize(additionalData));

            pseudoCons = PseudoConsole.Create(inputPipe.ReadSide, outputPipe.WriteSide, 40, 80);
            Task.Run(() => CopyPipeToOutput());
        }

        public void Connect(string hostname, string username)
        {
            string hostString = string.IsNullOrEmpty(username) ? hostname : $"{username}@{hostname}";
            sshProc = ProcessFactory.Start($"ssh -A {hostString}", PseudoConsole.PseudoConsoleThreadAttribute, pseudoCons.Handle);
        }

        public void Input(string text)
        {
            using var writer = new StreamWriter(new FileStream(inputPipe.WriteSide, FileAccess.Write), default, -1, true);
            writer.Write(text);
        }

        public void Resize(int rows, int cols) => pseudoCons?.Resize(rows, cols);

        unsafe private void CopyPipeToOutput()
        {
            using var pseudoConsoleOutput = new FileStream(outputPipe.ReadSide, FileAccess.Read);

            var bufferSpan = new Span<byte>(bufptr, writeBufferSize);

            int* at = (int*)&bufptr[writeBufferSize];
            *at = 0;

            while (true)
            {
                if (*at == writeBufferSize) *at = 0;

                var bytesWritten = pseudoConsoleOutput.Read(bufferSpan[*at..writeBufferSize]);
                *at += bytesWritten;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    inputPipe.Dispose();
                    outputPipe.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SshController()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    #region MiniTerm
    // From https://github.dev/microsoft/terminal/blob/main/samples/ConPTY/MiniTerm/

    /// <summary>
    /// Represents an instance of a process.
    /// </summary>
    internal sealed class Process : IDisposable
    {
        public Process(STARTUPINFOEX startupInfo, PROCESS_INFORMATION processInfo)
        {
            StartupInfo = startupInfo;
            ProcessInfo = processInfo;
        }

        public STARTUPINFOEX StartupInfo { get; }
        public PROCESS_INFORMATION ProcessInfo { get; }

        public bool Terminate() => TerminateProcess(ProcessInfo.hProcess, 0);

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // dispose unmanaged state

                // Free the attribute list
                if (StartupInfo.lpAttributeList != IntPtr.Zero)
                {
                    DeleteProcThreadAttributeList(StartupInfo.lpAttributeList);
                    Marshal.FreeHGlobal(StartupInfo.lpAttributeList);
                }

                // Close process and thread handles
                if (ProcessInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(ProcessInfo.hProcess);
                }
                if (ProcessInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(ProcessInfo.hThread);
                }

                disposedValue = true;
            }
        }

        ~Process()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // use the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Support for starting and configuring processes.
    /// </summary>
    /// <remarks>
    /// Possible to replace with managed code? The key is being able to provide the PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE attribute
    /// </remarks>
    static class ProcessFactory
    {
        /// <summary>
        /// Start and configure a process. The return value represents the process and should be disposed.
        /// </summary>
        internal static Process Start(string command, IntPtr attributes, IntPtr hPC)
        {
            var startupInfo = ConfigureProcessThread(hPC, attributes);
            var processInfo = RunProcess(ref startupInfo, command);
            return new Process(startupInfo, processInfo);
        }

        private static STARTUPINFOEX ConfigureProcessThread(IntPtr hPC, IntPtr attributes)
        {
            // this method implements the behavior described in https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#preparing-for-creation-of-the-child-process

            var lpSize = IntPtr.Zero;
            var success = InitializeProcThreadAttributeList(
                lpAttributeList: IntPtr.Zero,
                dwAttributeCount: 1,
                dwFlags: 0,
                lpSize: ref lpSize
            );
            if (success || lpSize == IntPtr.Zero) // we're not expecting `success` here, we just want to get the calculated lpSize
            {
                throw new InvalidOperationException("Could not calculate the number of bytes for the attribute list. " + Marshal.GetLastWin32Error());
            }

            var startupInfo = new STARTUPINFOEX();
            startupInfo.StartupInfo.cb = Marshal.SizeOf<STARTUPINFOEX>();
            startupInfo.lpAttributeList = Marshal.AllocHGlobal(lpSize);

            success = InitializeProcThreadAttributeList(
                lpAttributeList: startupInfo.lpAttributeList,
                dwAttributeCount: 1,
                dwFlags: 0,
                lpSize: ref lpSize
            );
            if (!success)
            {
                throw new InvalidOperationException("Could not set up attribute list. " + Marshal.GetLastWin32Error());
            }

            success = UpdateProcThreadAttribute(
                lpAttributeList: startupInfo.lpAttributeList,
                dwFlags: 0,
                attribute: attributes,
                lpValue: hPC,
                cbSize: (IntPtr)IntPtr.Size,
                lpPreviousValue: IntPtr.Zero,
                lpReturnSize: IntPtr.Zero
            );
            if (!success)
            {
                throw new InvalidOperationException("Could not set pseudoconsole thread attribute. " + Marshal.GetLastWin32Error());
            }

            return startupInfo;
        }

        private static PROCESS_INFORMATION RunProcess(ref STARTUPINFOEX sInfoEx, string commandLine)
        {
            int securityAttributeSize = Marshal.SizeOf<SECURITY_ATTRIBUTES>();
            var pSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
            var tSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
            var success = CreateProcess(
                lpApplicationName: null,
                lpCommandLine: commandLine,
                lpProcessAttributes: ref pSec,
                lpThreadAttributes: ref tSec,
                bInheritHandles: false,
                dwCreationFlags: EXTENDED_STARTUPINFO_PRESENT,
                lpEnvironment: IntPtr.Zero,
                lpCurrentDirectory: null,
                lpStartupInfo: ref sInfoEx,
                lpProcessInformation: out PROCESS_INFORMATION pInfo
            );
            if (!success)
            {
                throw new InvalidOperationException("Could not create process. " + Marshal.GetLastWin32Error());
            }

            return pInfo;
        }
    }

    /// <summary>
    /// Utility functions around the new Pseudo Console APIs
    /// </summary>
    internal sealed class PseudoConsole : IDisposable
    {
        public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;

        public IntPtr Handle { get; }

        private PseudoConsole(IntPtr handle)
        {
            Handle = handle;
        }

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int rows, int cols)
        {
            var createResult = CreatePseudoConsole(new COORD { X = (short)cols, Y = (short)rows }, inputReadSide, outputWriteSide, 0, out IntPtr hPC);
            if (createResult != 0)
                throw new InvalidOperationException("Could not create pseudo console. Error Code " + createResult);
            return new PseudoConsole(hPC);
        }

        internal void Resize(int rows, int cols) =>
            ResizePseudoConsole(Handle, new COORD { X = (short)cols, Y = (short)rows });

        public void Dispose()
        {
            ClosePseudoConsole(Handle);
        }
    }

    /// <summary>
    /// A pipe used to talk to the pseudoconsole, as described in:
    /// https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session
    /// </summary>
    /// <remarks>
    /// We'll have two instances of this class, one for input and one for output.
    /// </remarks>
    internal sealed class PseudoConsolePipe : IDisposable
    {
        public readonly SafeFileHandle ReadSide;
        public readonly SafeFileHandle WriteSide;

        public PseudoConsolePipe()
        {
            if (!CreatePipe(out ReadSide, out WriteSide, IntPtr.Zero, 0))
            {
                throw new InvalidOperationException("failed to create pipe");
            }
        }

        #region IDisposable

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReadSide?.Dispose();
                WriteSide?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    #endregion
}
