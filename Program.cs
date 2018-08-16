using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;


using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace HEROPixyCam
{
    public class Program
    {
        
        public static void Main()
        {
            Pixy2UART pixy = new Pixy2UART(CTRE.HERO.IO.Port6);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            /* loop forever */
            while (true)
            {
                if(pixy.m_link.bufferOverflow)
                {
                    pixy.m_link.ClearBuffer();
                    Debug.Print("Buffer filled, clearing");
                }
                Pixy2CCC.Block b = pixy.ccc.GetBlock();

                if (b != null)
                {
                    bool tmp = false;
                    Debug.Print(pixy.ccc.OffsetBlock(ref tmp, 50, 10).ToString() + " " + tmp.ToString());
                }
                else
                    Debug.Print("Null'd block");

                if(stopwatch.DurationMs > 1000)
                {
                    //pixy.SetLamp(255, 255);
                }
                if(stopwatch.DurationMs > 2000)
                {
                    pixy.SetLamp(0, 0);
                    stopwatch.Start();
                }

                //System.Threading.Thread.Sleep(5);
            }
        }
    }
}