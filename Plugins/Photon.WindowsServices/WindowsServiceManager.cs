using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.WindowsServices
{
    public class WindowsServiceDescription
    {
        public string Filename {get; set;}
        public string Name {get; set;}
        public string DisplayName {get; set;}
        public ServiceStartMode StartMode {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}
    }

    public static class WindowsServiceManager
    {
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenSCManager(string machineName, string databaseName, ScmAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceAccessRights dwDesiredAccess, int dwServiceType, ServiceStartMode dwStartType, ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lp, string lpPassword);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll")]
        private static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll")]
        private static extern int ControlService(IntPtr hService, ServiceControl dwControl, SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ChangeServiceConfig(IntPtr hService, uint nServiceType, uint nStartType, uint nErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, [In] char[] lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);


        public static void Install(WindowsServiceDescription description, CancellationToken token = default(CancellationToken))
        {
            var scm = OpenSCManager(ScmAccessRights.AllAccess);

            try {
                var service = OpenService(scm, description.Name, ServiceAccessRights.AllAccess);

                if (service != IntPtr.Zero)
                    throw new ApplicationException($"Service '{description.Name}' already exists!");

                service = CreateService(scm,
                    description.Name,
                    description.DisplayName,
                    ServiceAccessRights.AllAccess,
                    SERVICE_WIN32_OWN_PROCESS,
                    description.StartMode,
                    ServiceError.Normal,
                    description.Filename,
                    null,
                    IntPtr.Zero,
                    null,
                    description.Username,
                    description.Password);

                if (service == IntPtr.Zero)
                    throw new ApplicationException("Failed to install service.");

                CloseServiceHandle(service);
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        public static void Uninstall(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.AllAccess);

            try {
                var service = OpenService(scm, serviceName, ServiceAccessRights.AllAccess);

                if (service == IntPtr.Zero)
                    throw new ApplicationException("Service not installed.");

                try {
                    if (!DeleteService(service))
                        throw new ApplicationException("Could not delete service " + Marshal.GetLastWin32Error());
                }
                finally {
                    CloseServiceHandle(service);
                }
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        public static bool ServiceIsInstalled(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try {
                var service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus);

                if (service == IntPtr.Zero)
                    return false;

                CloseServiceHandle(service);
                return true;
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        public static async Task StartServiceAsync(string serviceName, CancellationToken token = default(CancellationToken))
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try {
                var service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);

                if (service == IntPtr.Zero)
                    throw new ApplicationException("Could not open service.");

                try {
                    await StartServiceAsync(service, token);
                }
                finally {
                    CloseServiceHandle(service);
                }
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        public static async Task StopServiceAsync(string serviceName, CancellationToken token = default(CancellationToken))
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try {
                var service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);

                if (service == IntPtr.Zero)
                    throw new ApplicationException("Could not open service.");

                try {
                    await StopServiceAsync(service, token);
                }
                finally {
                    CloseServiceHandle(service);
                }
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        public static ServiceState GetServiceStatus(string serviceName)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try {
                var service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus);

                if (service == IntPtr.Zero)
                    return ServiceState.NotFound;

                try {
                    return GetServiceStatus(service);
                }
                finally {
                    CloseServiceHandle(service);
                }
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        public static void Configure(WindowsServiceDescription description)
        {
            var scm = OpenSCManager(ScmAccessRights.Connect);

            try {
                var service = OpenService(scm, description.Name, ServiceAccessRights.QueryConfig | ServiceAccessRights.ChangeConfig);

                if (service == IntPtr.Zero)
                    throw new ApplicationException("Could not open service.");

                try {
                    var result = ChangeServiceConfig(
                        service,
                        SERVICE_NO_CHANGE,
                        (uint)description.StartMode,
                        SERVICE_NO_CHANGE,
                        description.Filename,
                        null,
                        IntPtr.Zero,
                        null,
                        description.Username,
                        description.Password,
                        description.DisplayName);

                    if (!result) {
                        var nError = Marshal.GetLastWin32Error();
                        var win32Exception = new Win32Exception(nError);

                        throw new ExternalException($"Could not change service start type: {win32Exception.Message}");
                    }
                }
                finally {
                    CloseServiceHandle(service);
                }
            }
            finally {
                CloseServiceHandle(scm);
            }
        }

        private static async Task StartServiceAsync(IntPtr service, CancellationToken token = default(CancellationToken))
        {
            StartService(service, 0, 0);

            var changedStatus = await WaitForServiceStatusAsync(service, ServiceState.StartPending, ServiceState.Running, token);

            if (!changedStatus)
                throw new ApplicationException("Unable to start service");
        }

        private static async Task StopServiceAsync(IntPtr service, CancellationToken token = default(CancellationToken))
        {
            var status = new SERVICE_STATUS();
            ControlService(service, ServiceControl.Stop, status);

            var changedStatus = await WaitForServiceStatusAsync(service, ServiceState.StopPending, ServiceState.Stopped, token);

            if (!changedStatus)
                throw new ApplicationException("Unable to stop service");
        }

        private static ServiceState GetServiceStatus(IntPtr service)
        {
            var status = new SERVICE_STATUS();

            if (QueryServiceStatus(service, status) == 0)
                throw new ApplicationException("Failed to query service status.");

            return status.dwCurrentState;
        }

        private static async Task<bool> WaitForServiceStatusAsync(IntPtr service, ServiceState waitStatus, ServiceState desiredStatus, CancellationToken token = default(CancellationToken))
        {
            var status = new SERVICE_STATUS();

            QueryServiceStatus(service, status);
            if (status.dwCurrentState == desiredStatus) return true;

            while (status.dwCurrentState == waitStatus) {
                token.ThrowIfCancellationRequested();

                // Do not wait longer than the wait hint. A good interval is
                // one tenth the wait hint, but no less than 1 second and no
                // more than 10 seconds.
                var dwWaitTime = status.dwWaitHint / 10;

                if (dwWaitTime < 1_000) dwWaitTime = 1_000;
                else if (dwWaitTime > 10_000) dwWaitTime = 10_000;

                await Task.Delay(dwWaitTime, token);

                if (QueryServiceStatus(service, status) == 0) break;
            }

            return status.dwCurrentState == desiredStatus;
        }

        private static IntPtr OpenSCManager(ScmAccessRights rights)
        {
            var scm = OpenSCManager(null, null, rights);

            if (scm == IntPtr.Zero)
                throw new ApplicationException("Could not connect to service control manager.");

            return scm;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class SERVICE_STATUS
    {
        public int dwServiceType = 0;
        public ServiceState dwCurrentState = 0;
        public int dwControlsAccepted = 0;
        public int dwWin32ExitCode = 0;
        public int dwServiceSpecificExitCode = 0;
        public int dwCheckPoint = 0;
        public int dwWaitHint = 0;
    }

    public enum ServiceState
    {
        Unknown = -1, // The state cannot be (has not been) retrieved.
        NotFound = 0, // The service is not known on the host server.
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7
    }

    [Flags]
    internal enum ScmAccessRights
    {
        Connect = 0x0001,
        CreateService = 0x0002,
        EnumerateService = 0x0004,
        Lock = 0x0008,
        QueryLockStatus = 0x0010,
        ModifyBootConfig = 0x0020,
        StandardRightsRequired = 0xF0000,
        AllAccess = (StandardRightsRequired | Connect | CreateService |
                     EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
    }

    [Flags]
    internal enum ServiceAccessRights
    {
        QueryConfig = 0x1,
        ChangeConfig = 0x2,
        QueryStatus = 0x4,
        EnumerateDependants = 0x8,
        Start = 0x10,
        Stop = 0x20,
        PauseContinue = 0x40,
        Interrogate = 0x80,
        UserDefinedControl = 0x100,
        Delete = 0x00010000,
        StandardRightsRequired = 0xF0000,
        AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig |
                     QueryStatus | EnumerateDependants | Start | Stop | PauseContinue |
                     Interrogate | UserDefinedControl)
    }

    public enum ServiceStartMode
    {
        Start = 0x00000000,
        SystemStart = 0x00000001,
        AutoStart = 0x00000002,
        Manual = 0x00000003,
        Disabled = 0x00000004,
    }

    internal enum ServiceControl
    {
        Stop = 0x00000001,
        Pause = 0x00000002,
        Continue = 0x00000003,
        Interrogate = 0x00000004,
        Shutdown = 0x00000005,
        ParamChange = 0x00000006,
        NetBindAdd = 0x00000007,
        NetBindRemove = 0x00000008,
        NetBindEnable = 0x00000009,
        NetBindDisable = 0x0000000A
    }

    internal enum ServiceError
    {
        Ignore = 0x00000000,
        Normal = 0x00000001,
        Severe = 0x00000002,
        Critical = 0x00000003
    }
}
