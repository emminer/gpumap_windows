# gpumap_windows
Ever wondered which physical video card is assigned as GPU2 or GPU6?
Troubleshooting nightmares? Your miner reported that GPU7 failed of the bus. How do you know which physical card is assigned as GPU7?

Here is an ingenious way to determine physical location of the NVIDIA GPU's on windows.

When executed, first, it will turn all GPU fans OFF.
Then it will turn only GPU0 fan ON allowing you to physically map which one is GPU0 by simply looking at which fan is spinning. By pressing "y" button, it will turn GPU0 OFF and move on to the next GPU.
It will turn GPU fans ON then OFF, one by one until the last GPU is reached.

It's highly inspired by @leenoox's gpumap for nvOC v0019-2.0.
