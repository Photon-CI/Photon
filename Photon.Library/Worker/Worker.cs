using Photon.Communication;
using Photon.Framework;
using Photon.Library.TcpMessages.Worker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security;
using System.Threading;

namespace Photon.Library.Worker
{
    public class Worker : IDisposable
    {
        private Process _process;
        private AnonymousPipeServerStream _pipeIn;
        private AnonymousPipeServerStream _pipeOut;
        private bool _isStarted;

        public string Id {get;}
        public string Filename {get; set;}
        public MessageProcessorRegistry MessageRegistry {get; set;}
        public MessageTransceiver Transceiver {get; private set;}
        public bool ShowConsole {get; set;}

        public string Username {get; set;}
        public string Password {get; set;}
        public bool LoadUserProfile {get; set;}


        public Worker()
        {
            Id = Guid.NewGuid().ToString("D");
        }

        public void Dispose()
        {
            try {Stop();}
            catch {}

            _pipeIn?.Dispose();
            _pipeOut?.Dispose();
            Transceiver?.Dispose();
        }

        public void Connect()
        {
            if (_isStarted) throw new InvalidOperationException("Worker has already been started!");
            _isStarted = true;

            StartProcess();
        }

        public void Disconnect(int milliseconds = 6_000)
        {
            var disconnectMessage = new WorkerDisconnectRequestMessage();

            var _ = Transceiver.Send(disconnectMessage)
                .GetResponseAsync<WorkerDisconnectResponseMessage>();

            if (!_process.WaitForExit(milliseconds))
                throw new TimeoutException("Timeout waiting for worker to disconnect!");

            Transceiver.Stop();
        }

        private void Stop()
        {
            if (!_isStarted) return;
            _isStarted = false;

            if (Transceiver != null) {
                try {
                    using (var tokenSource = new CancellationTokenSource(6_000)) {
                        Transceiver.Flush(tokenSource.Token);
                    }
                }
                catch (OperationCanceledException) {}

                Transceiver.Stop();
            }

            if (_process != null)
                StopProcess();
        }

        private void StartProcess()
        {
            SecureString securePass = null;
            if (!string.IsNullOrEmpty(Password)) {
                securePass = new SecureString();
                foreach (var c in Password)
                    securePass.AppendChar(c);
            }

            _process = null;
            Transceiver = null;
            _pipeIn = null;
            _pipeOut = null;
            try {
                Transceiver = new MessageTransceiver(MessageRegistry);
                _pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
                _pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

                var argStr = string.Join(" ", GetArguments());

                var _exe = ShowConsole ? "cmd.exe" : Filename;
                var _args = ShowConsole ? $"/C \"\"{Filename}\" {argStr}\"" : argStr;

                _process = new Process {
                    StartInfo = {
                        FileName = _exe,
                        Arguments = _args,
                        UseShellExecute = false,
                        UserName = Username,
                        Password = securePass,
                        LoadUserProfile = LoadUserProfile,
                        CreateNoWindow = !ShowConsole,
                    },
                };

                _process.Start();

                var _pipeInOut = new CombinedStream(_pipeIn, _pipeOut);
                Transceiver.Start(_pipeInOut);
            }
            catch {
                _process?.Dispose();
                _process = null;

                _pipeIn?.Dispose();
                _pipeOut?.Dispose();
                Transceiver?.Dispose();

                throw;
            }
            finally {
                _pipeIn?.DisposeLocalCopyOfClientHandle();
                _pipeOut?.DisposeLocalCopyOfClientHandle();
            }
        }

        private bool StopProcess(int milliseconds = 0)
        {
            try {
                if (_process.HasExited) return true;

                if (!_process.CloseMainWindow())
                    _process.Kill();

                if (milliseconds > 0)
                    _process.WaitForExit(milliseconds);

                return true;
            }
            catch (InvalidOperationException) {
                return true;
            }
            catch (Exception) {
                //...
                return false;
            }
            finally {
                _process.Dispose();
                _process = null;
            }
        }

        private IEnumerable<string> GetArguments()
        {
            var pipeInHandle = _pipeIn?.GetClientHandleAsString();
            var pipeOutHandle = _pipeOut?.GetClientHandleAsString();

            if (!string.IsNullOrEmpty(pipeInHandle))
                yield return $"-in-handle=\"{pipeInHandle}\"";

            if (!string.IsNullOrEmpty(pipeOutHandle))
                yield return $"-out-handle=\"{pipeOutHandle}\"";
        }
    }
}
