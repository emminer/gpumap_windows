using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace emminer.gpumap
{
    internal enum NvStatus
    {
        OK = 0,
        ERROR = -1,
        LIBRARY_NOT_FOUND = -2,
        NO_IMPLEMENTATION = -3,
        API_NOT_INTIALIZED = -4,
        INVALID_ARGUMENT = -5,
        NVIDIA_DEVICE_NOT_FOUND = -6,
        END_ENUMERATION = -7,
        INVALID_HANDLE = -8,
        INCOMPATIBLE_STRUCT_VERSION = -9,
        HANDLE_INVALIDATED = -10,
        OPENGL_CONTEXT_NOT_CURRENT = -11,
        NO_GL_EXPERT = -12,
        INSTRUMENTATION_DISABLED = -13,
        EXPECTED_LOGICAL_GPU_HANDLE = -100,
        EXPECTED_PHYSICAL_GPU_HANDLE = -101,
        EXPECTED_DISPLAY_HANDLE = -102,
        INVALID_COMBINATION = -103,
        NOT_SUPPORTED = -104,
        PORTID_NOT_FOUND = -105,
        EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106,
        INVALID_PERF_LEVEL = -107,
        DEVICE_BUSY = -108,
        NV_PERSIST_FILE_NOT_FOUND = -109,
        PERSIST_DATA_NOT_FOUND = -110,
        EXPECTED_TV_DISPLAY = -111,
        EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112,
        NO_ACTIVE_SLI_TOPOLOGY = -113,
        SLI_RENDERING_MODE_NOTALLOWED = -114,
        EXPECTED_DIGITAL_FLAT_PANEL = -115,
        ARGUMENT_EXCEED_MAX_SIZE = -116,
        DEVICE_SWITCHING_NOT_ALLOWED = -117,
        TESTING_CLOCKS_NOT_SUPPORTED = -118,
        UNKNOWN_UNDERSCAN_CONFIG = -119,
        TIMEOUT_RECONFIGURING_GPU_TOPO = -120,
        DATA_NOT_FOUND = -121,
        EXPECTED_ANALOG_DISPLAY = -122,
        NO_VIDLINK = -123,
        REQUIRES_REBOOT = -124,
        INVALID_HYBRID_MODE = -125,
        MIXED_TARGET_TYPES = -126,
        SYSWOW64_NOT_SUPPORTED = -127,
        IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128,
        REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129,
        OUT_OF_MEMORY = -130,
        WAS_STILL_DRAWING = -131,
        FILE_NOT_FOUND = -132,
        TOO_MANY_UNIQUE_STATE_OBJECTS = -133,
        INVALID_CALL = -134,
        D3D10_1_LIBRARY_NOT_FOUND = -135,
        FUNCTION_NOT_FOUND = -136
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NvPhysicalGpuHandle
    {
        private readonly IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvCooler
    {
        public int Type;
        public int Controller;
        public int DefaultMin;
        public int DefaultMax;
        public int CurrentMin;
        public int CurrentMax;
        public int CurrentLevel;
        public int DefaultPolicy;
        public int CurrentPolicy;
        public int Target;
        public int ControlType;
        public int Active;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUCoolerSettings
    {
        public uint Version;
        public uint Count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_COOLER_PER_GPU)]
        public NvCooler[] Coolers;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvLevel
    {
        public int Level;
        public int Policy;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUCoolerLevels
    {
        public uint Version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_COOLER_PER_GPU)]
        public NvLevel[] Levels;
    }

    class NVAPI
    {
        const string NVAPI_FileName = "nvapi.dll";

        public const int MAX_PHYSICAL_GPUS = 64;
        public const int SHORT_STRING_MAX = 64;
        public const int MAX_COOLER_PER_GPU = 20;

        public static readonly uint GPU_COOLER_SETTINGS_VER = 132040;
        public static readonly uint GPU_COOLER_LEVELS_VER = 65700;

        private delegate NvStatus NvAPI_InitializeDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus NvAPI_GPU_GetFullNameDelegate(NvPhysicalGpuHandle gpuHandle, StringBuilder name);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus NvAPI_EnumPhysicalGPUsDelegate([Out] NvPhysicalGpuHandle[] gpuHandles, out int gpuCount);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus NvAPI_GPU_GetCoolerSettingsDelegate(NvPhysicalGpuHandle gpuHandle, int coolerIndex, ref NvGPUCoolerSettings nvGPUCoolerSettings);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus NvAPI_GPU_SetCoolerLevelsDelegate(NvPhysicalGpuHandle gpuHandle, int coolerIndex, ref NvGPUCoolerLevels NvGPUCoolerLevels);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus NvAPI_GPU_GetPCIIdentifiersDelegate(NvPhysicalGpuHandle gpuHandle, out uint deviceId, out uint subSystemId, out uint revisionId, out uint extDeviceId);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate NvStatus NvAPI_GPU_GetBusIdDelegage(NvPhysicalGpuHandle gpuHandle, out uint busId);

        private static readonly bool available;
        private static readonly NvAPI_InitializeDelegate NvAPI_Initialize;
        public static readonly NvAPI_EnumPhysicalGPUsDelegate NvAPI_EnumPhysicalGPUs;
        public static readonly NvAPI_GPU_GetFullNameDelegate NvAPI_GPU_GetFullName;
        public static readonly NvAPI_GPU_GetCoolerSettingsDelegate NvAPI_GPU_GetCoolerSettings;
        public static readonly NvAPI_GPU_SetCoolerLevelsDelegate NvAPI_GPU_SetCoolerLevels;
        public static readonly NvAPI_GPU_GetPCIIdentifiersDelegate NvAPI_GPU_GetPCIIdentifiers;
        public static readonly NvAPI_GPU_GetBusIdDelegage NvAPI_GPU_GetBusId;

        [DllImport(NVAPI_FileName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr nvapi_QueryInterface(uint id);

        private static void GetDelegate<T>(uint id, out T newDelegate) where T : class
        {
            IntPtr ptr = nvapi_QueryInterface(id);
            if (ptr != IntPtr.Zero)
            {
                newDelegate =
                  Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            }
            else
            {
                newDelegate = null;
            }
        }

        static NVAPI()
        {
            GetDelegate(0x0150E828, out NvAPI_Initialize);
            if (NvAPI_Initialize != null && NvAPI_Initialize() == NvStatus.OK)
            {
                GetDelegate(0xE5AC921F, out NvAPI_EnumPhysicalGPUs);
                GetDelegate(0xCEEE8E9F, out NvAPI_GPU_GetFullName);
                GetDelegate(0xDA141340, out NvAPI_GPU_GetCoolerSettings);
                GetDelegate(0x891FA0AE, out NvAPI_GPU_SetCoolerLevels);
                GetDelegate(0x2DDFB66E, out NvAPI_GPU_GetPCIIdentifiers);
                GetDelegate(0x1BE0B8E5, out NvAPI_GPU_GetBusId);
            }
            if (NvAPI_EnumPhysicalGPUs != null
                && NvAPI_GPU_GetFullName != null
                && NvAPI_GPU_GetCoolerSettings != null
                && NvAPI_GPU_SetCoolerLevels != null
                && NvAPI_GPU_GetPCIIdentifiers != null
                && NvAPI_GPU_GetBusId != null)
            {
                available = true;
            }
        }

        public static bool IsAvailable
        {
            get { return available; }
        }
    }
}
