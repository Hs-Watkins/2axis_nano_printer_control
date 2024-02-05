using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.KCube.DCServoCLI;

namespace KDC101Console
{
    class Program
    {
        static bool[] bC = { false, false };

        static void Main(string[] args)
        {
            // Uncomment this line (and SimulationManager.Instance.UninitializeSimulations() at the end on Main)
            // If you are using a simulated device
            //SimulationManager.Instance.InitializeSimulations();

            // Find the Devices and Begin Communicating with them

            // Enter the serial number for your device
            string serialNo1 = "27000001";
            string serialNo2 = "27000002";

            DeviceManagerCLI.BuildDeviceList();

            // This creates an instance of KCubeDCServo class, passing in the Serial 
            //Number parameter.  
            KCubeDCServo device1 = KCubeDCServo.CreateKCubeDCServo(serialNo1);
            KCubeDCServo device2 = KCubeDCServo.CreateKCubeDCServo(serialNo2);

            // We tell the user that we are opening connection to the device. 
            Console.WriteLine("Opening device {0}", serialNo1);
            Console.WriteLine("Opening device {0}", serialNo2);

            // This connects to the device. 
            device1.Connect(serialNo1);

            // Wait for the device settings to initialize. We ask the device to 
            // throw an exception if this takes more than 5000ms (5s) to complete. 
            device1.WaitForSettingsInitialized(5000);

            // Same for Device 2.
            device2.Connect(serialNo2);
            device2.WaitForSettingsInitialized(5000);

            // This calls LoadMotorConfiguration on the device to initialize the 
            // DeviceUnitConverter object required for real world unit parameters.
            MotorConfiguration motorSettings1 = device1.LoadMotorConfiguration(device1.DeviceID,
            DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);

            MotorConfiguration motorSettings2 = device2.LoadMotorConfiguration(device2.DeviceID,
            DeviceConfiguration.DeviceSettingsUseOptionType.UseFileSettings);

            // This starts polling the device at intervals of 250ms (0.25s). 

            device1.StartPolling(250);
            device2.StartPolling(250);

            // We are now able to Enable the device otherwise any move is ignored. 
            // You should see a physical response from your controller. 
            device1.EnableDevice();
            device2.EnableDevice();
            Console.WriteLine("Devices Enabled");


            // Needs a delay to give time for the device to be enabled. 
            Thread.Sleep(500);

            // Home both actuators at once  
            Thread Home1Thread = new Thread(() => Home1(device1));
            Thread Home2Thread = new Thread(() => Home2(device2));
            Console.WriteLine("Actuators are Homing");
            Home1Thread.Start();
            Home2Thread.Start();

            // Wait for the threads to complete
            Home1Thread.Join();
            Home2Thread.Join();


            // Move the stage/actuator to 5mm (or degrees depending on the device 
            // connected).
            //device1.SetJogVelocityParams();
            decimal[] Xpositions = { 20m, 20.766m, 20.583m, 19.461m, 17.468m, 14.724m, 11.394m, 7.68m, 3.804m, 0 };
            decimal[] Ypositions = { 0, 3.804m, 7.68m, 11.394m, 14.724m, 17.468m, 19.461m, 20.583m, 20.766m, 20 };
            Console.WriteLine("Actuator is Moving");

            Thread mainThread = Thread.CurrentThread;
            mainThread.Name = "Main Thread";

            // Iterate through XPositions and YPositions Simultaneously but synchronized
            for (int i = 0; i < Xpositions.Length; i++)
            {
                Thread MoveXThread = new Thread(() => MoveX(device1, Xpositions[i]));
                Thread MoveYThread = new Thread(() => MoveY(device2, Ypositions[i]));

                // Move the Actuators
                MoveXThread.Start();
                MoveYThread.Start();

                // Wait for Move to Finish
                MoveXThread.Join();
                MoveYThread.Join();
            }



            //Stop polling devices
            device1.StopPolling();
            device2.StopPolling();

            // Shut down controller using Disconnect() to close comms
            // Then the used library
            device1.ShutDown();
            device2.ShutDown();

            Console.WriteLine("Complete. Press any key to exit");
            Console.ReadKey();

            // Uncomment this line if you are using Simulations
            //SimulationManager.Instance.UninitializeSimulations();



        }
        static void MoveX(KCubeDCServo device1, decimal Xposition)
        {
            device1.MoveTo(Xposition, 20000);
            Console.WriteLine("Current X position: {0}", device1.Position);
        }
        static void MoveY(KCubeDCServo device2, decimal Yposition)
        {
            device2.MoveTo(Yposition, 20000);
            Console.WriteLine("Current Y position: {0}", device2.Position);
        }

        static void Home1(KCubeDCServo device1)
        {
            device1.Home(60000);
        }

        static void Home2(KCubeDCServo device2)
        {
            device2.Home(60000);
        }
    }


}