using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emminer.gpumap
{
    class NvidiaGPU
    {
        private NvPhysicalGpuHandle _handle;
        private NvGPUCoolerSettings _coolerSetting;
        public NvidiaGPU(NvPhysicalGpuHandle handle, NvGPUCoolerSettings coolerSetting)
        {
            _handle = handle;
            _coolerSetting = coolerSetting;
        }
        public int Index { get; set; }
        public uint BusId { get; set; }
        public string Name { get; set; }
        public uint SubDeviceId { get; set; }
        public string VendorName
        {
            get
            {
                if (SubDeviceId != 0)
                {
                    var vendorIdStr = SubDeviceId.ToString("X");
                    var vendorId = vendorIdStr.Substring(vendorIdStr.Length >= 4 ? vendorIdStr.Length - 4 : 0);
                    switch(vendorId)
                    {
                        case "3842":
                            return "EVGA";
                        case "1462":
                            return "MSI";
                        case "10DE":
                            return "NVIDIA";
                        case "19DA":
                            return "ZOTAC";
                        case "807D":
                        case "1043":
                            return "ASUS";
                        case "1458":
                            return "GYGABYTE";
                        case "196E":
                            return "PNY";
                        case "1569":
                            return "PALIT";
                    }
                    return vendorIdStr;
                }

                return "Vendor 0";
            }
        }
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", VendorName, Name);
            }
        }
        public void SetFanSpeed(int level)
        {
            if (level < 0 || level > 100)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            if (_coolerSetting.Coolers == null || _coolerSetting.Coolers.Length == 0)
            {
                throw new InvalidOperationException("cooler setting is null.");
            }

            var levels = new NvGPUCoolerLevels
            {
                Version = NVAPI.GPU_COOLER_LEVELS_VER,
                Levels = new NvLevel[NVAPI.MAX_COOLER_PER_GPU]
            };
            for (int i = 0; i < _coolerSetting.Count; i++)
            {
                levels.Levels[i].Level = level;
                levels.Levels[i].Policy = _coolerSetting.Coolers[i].CurrentPolicy;
                var ret = NVAPI.NvAPI_GPU_SetCoolerLevels(_handle, 0, ref levels);
                if (ret != NvStatus.OK)
                {
                    throw new InvalidOperationException(string.Format("setting fan speed failed, code: {0}", ret));
                }
            }
        }

        public static List<NvidiaGPU> EnumGPUs()
        {
            NvStatus status;
            var gpus = new List<NvidiaGPU>();
            if (!NVAPI.IsAvailable)
            {
                return gpus;
            }

            var gpuHandles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
            status = NVAPI.NvAPI_EnumPhysicalGPUs(gpuHandles, out int count);
            if (status != NvStatus.OK)
            {
                return gpus;
            }

            for (int i = 0; i < count; i++)
            {
                var handle = gpuHandles[i];
                var sb = new StringBuilder(NVAPI.SHORT_STRING_MAX);
                status = NVAPI.NvAPI_GPU_GetFullName(handle, sb);
                if (status != NvStatus.OK)
                {
                    continue;
                }
                NvGPUCoolerSettings settings = new NvGPUCoolerSettings
                {
                    Version = NVAPI.GPU_COOLER_SETTINGS_VER,
                    Count = 1,
                    Coolers = new NvCooler[NVAPI.MAX_COOLER_PER_GPU]
                };
                status = NVAPI.NvAPI_GPU_GetCoolerSettings(handle, 0, ref settings);
                if (status != NvStatus.OK)
                {
                    continue;
                }
                status = NVAPI.NvAPI_GPU_GetPCIIdentifiers(handle, out uint deviceId, out uint subSystemId, out uint revisionId, out uint extDeviceId);
                if (status != NvStatus.OK)
                {
                    continue;
                }
                status = NVAPI.NvAPI_GPU_GetBusId(handle, out uint busId);
                if (status != NvStatus.OK)
                {
                    continue;
                }
                var gpu = new NvidiaGPU(handle, settings) { BusId = busId, Name = sb.ToString(), Index = i, SubDeviceId = subSystemId };
                gpus.Add(gpu);
            }

            return gpus;
        }
    }
}
