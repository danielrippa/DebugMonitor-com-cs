using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Collections.Generic;

using static Win32.Kernel32;

namespace Debug {

  [ComVisible(true)]
  [Guid("A6992D2F-5309-4B7E-9721-3F7784A88E1E")]
  [InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IDebugMonitor {
    void Start();
    void Stop();
    int GetStringCount();
    string GetNextString();
  }

  [ComVisible(true)]
  [Guid("46A5B401-9C45-4779-8095-D4E0BD6078D5")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("Debug.Monitor")]
  public class Monitor : IDebugMonitor, IDisposable {

    private const int DBWIN_BUFFER_SIZE = 4096;
    private const uint WAIT_OBJECT_0 = 0;

    private volatile bool isListening;
    private Thread? listeningThread;

    public void Start() {
      if (isListening) return; isListening = true;

      listeningThread = new Thread(ListenLoop) { IsBackground = true };
      listeningThread.Start();
    }

    public void Stop() {
      if (!isListening) return; isListening = false;
      listeningThread?.Join();
    }

    private readonly object listLock = new object();

    public int GetStringCount() {
      lock (listLock) {
          return capturedStrings.Count;
      }
    }

    public string GetNextString() {

      lock (listLock) {
        if (capturedStrings.Count == 0) {
            return "";
        }

        string nextMessage = capturedStrings[0];
        capturedStrings.RemoveAt(0);
        return nextMessage;
      }

    }

    private IntPtr hDBWinMutex, hDBWinBuffer, hEventBufferReady, hEventDataReady, pBuffer;
    private readonly List<string> capturedStrings = new List<string>();

    private void ListenLoop() {

      hDBWinMutex = CreateMutex(IntPtr.Zero, false, "DBWinMutex");

      hEventBufferReady = CreateEvent(IntPtr.Zero, false, false, "DBWIN_BUFFER_READY");
      hEventDataReady = CreateEvent(IntPtr.Zero, false, false, "DBWIN_DATA_READY");

      hDBWinBuffer = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, DBWIN_BUFFER_SIZE, "DBWIN_BUFFER");
      pBuffer = MapViewOfFile(hDBWinBuffer, 0x04, 0, 0, 0);

      if (pBuffer == IntPtr.Zero) return;

      SetEvent(hEventBufferReady);

      while (isListening) {

        if (WaitForSingleObject(hEventDataReady, 250) != WAIT_OBJECT_0) continue;

        int processId = Marshal.ReadInt32(pBuffer);
        var pString = new IntPtr(pBuffer.ToInt64() + 4);

        int len = 0;
        while (len < DBWIN_BUFFER_SIZE - 4 && Marshal.ReadByte(pString, len) != 0) { len++; }

        if (len > 0) {
          var buffer = new byte[len];
          Marshal.Copy(pString, buffer, 0, len);

          string message = DecodeString(buffer);

          lock (listLock) {
            capturedStrings.Add(processId + " " + message);
          }
        }

        SetEvent(hEventBufferReady);

      }

      if (pBuffer != IntPtr.Zero) UnmapViewOfFile(pBuffer);
      if (hDBWinBuffer != IntPtr.Zero) CloseHandle(hDBWinBuffer);
      if (hEventDataReady != IntPtr.Zero) CloseHandle(hEventDataReady);
      if (hEventBufferReady != IntPtr.Zero) CloseHandle(hEventBufferReady);
      if (hDBWinMutex != IntPtr.Zero) CloseHandle(hDBWinMutex);

    }

    // Un algoritmo falopa para adivinar si es ANSI o Unicode basandose en la cantidad de bytes
    private static string DecodeString(byte[] rawBuffer) {
      if ((rawBuffer.Length % 2) != 0) {
        // Si es impar es ANSI si o si.
        return Encoding.Default.GetString(rawBuffer);
      } else {
        // Si es par todavia podria ser ANSI.
        if (rawBuffer.Length > 1 && rawBuffer[1] == 0) {
          // Pero si el segundo byte es 0, seguro es Unicode.
          return Encoding.Unicode.GetString(rawBuffer);
        } else {
          return Encoding.Default.GetString(rawBuffer);
        }
      }
    }

    public void Dispose() {
      Stop();
      GC.SuppressFinalize(this);
    }

  }

}
