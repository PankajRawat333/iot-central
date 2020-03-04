using System;

namespace IoTCentralSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            IoTCentralDemo demo = new IoTCentralDemo();
            demo.Run(2);
            Console.Read();
        }
    }
}
