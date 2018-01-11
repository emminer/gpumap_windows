using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emminer.gpumap
{
    class Program
    {
        static void Main(string[] args)
        {
            var gpus = NvidiaGPU.EnumGPUs();
            if (gpus == null || gpus.Count == 0)
            {
                Console.WriteLine("Nvidia gpu is not found.");
                return;
            }
            Console.WriteLine("gpumap v{0} by emminer, inspired by leenoox's gpumap in nvOC.", GetVersion());
            Console.WriteLine("gpumap will help you physically map your GPU's by spinning single GPU fan(s) only, one at a time.");
            Console.WriteLine("It will also show some info about the GPU. Please follow prompts.");
            Console.WriteLine("gpumap has detected {0} GPU's.", gpus.Count);

            Console.WriteLine("To continue make sure you've stoped all mining processes.");
            Console.WriteLine("Continue(y / n) ? ");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.KeyChar == 'y')
            {
                var cuda_drvice_order_env = Environment.GetEnvironmentVariable("CUDA_DEVICE_ORDER");
                if (!"PCI_BUS_ID".Equals(cuda_drvice_order_env, StringComparison.Ordinal))
                {
                    Console.WriteLine("Fixing the CUDA_DEVICE_ORDER=PCI_BUS_ID in order to properly map GPU's.");
                    Console.WriteLine("Please reboot for the changes to take effect.");
                    Environment.SetEnvironmentVariable("CUDA_DEVICE_ORDER", "PCI_BUS_ID", EnvironmentVariableTarget.User);
                }
                else
                {
                    Run(gpus);
                }
            }
        }

        static void Run(List<NvidiaGPU> gpus)
        {
            Console.WriteLine();
            Console.WriteLine("Turning all fans OFF...");
            gpus.ForEach(gpu =>
            {
                gpu.SetFanSpeed(0);
            });
            Console.WriteLine("... done!");
            Console.WriteLine();
            Console.WriteLine();

            var count = 0;
            while (count < gpus.Count)
            {
                var gpu = gpus[count];
                gpu.SetFanSpeed(75);
                Console.WriteLine("Take a look at your rig for spinning fan now");
                Console.WriteLine("GPU{0} fan is ON. This is your GPU{0}.", count);
                Console.WriteLine();

                Console.WriteLine("GPU{0} info:", count);
                Console.WriteLine("VENDOR_ID: {0}", gpu.VendorName);
                Console.WriteLine("Full Name: {0}", gpu.Name);
                Console.WriteLine("Bus ID: {0:X}", gpu.BusId);
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Continue(y / n) ? ");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    gpu.SetFanSpeed(0);
                }
                else if (key.KeyChar == 'n' || key.KeyChar == 'N')
                {
                    gpu.SetFanSpeed(0);
                    Console.WriteLine("Acknowledged. Thank you for using gpumap.");
                    Console.WriteLine("Reboot is required.");
                    return;
                }
                else
                {
                    Console.WriteLine("Neither y or n were pressed!");
                    Console.WriteLine("Lets try this again...");

                    key = Console.ReadKey();
                    Console.WriteLine();

                    if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                    {
                        gpu.SetFanSpeed(0);
                    }
                    else if (key.KeyChar == 'n')
                    {
                        gpu.SetFanSpeed(0);
                        Console.WriteLine("Acknowledged. Thank you for using gpumap");
                        Console.WriteLine("Reboot is required.");
                        return;
                    }
                    else
                    {
                        gpu.SetFanSpeed(0);
                        Console.WriteLine("Neither y or n were pressed again!");
                        Console.WriteLine("Thank you for using gpumap.");
                        Console.WriteLine("Reboot is required.");
                    }
                }
                count++;
            }

            Console.WriteLine("All done, that was the last detected GPU.");
            Console.WriteLine("Thank you for using gpumap.");
            Console.WriteLine("Reboot is required.");
        }
        static string GetVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("{0}.{1}", version.Major, version.Minor);
        }
    }
}
