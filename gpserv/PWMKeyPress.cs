using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Threading;

namespace gpserv
{
    public class PWMKeyPress
    {
        Keys key;
        public float intensity = 0;
        public float max_intensity = 0;

        public int multiplier_on = 8000;
        public int multiplier_off = 10000;
        public int sleep_time = 5;

        Thread PWMThread;

        public PWMKeyPress(Keys _key, int _sleep_time)
        {
            this.key = _key;
            this.sleep_time = _sleep_time;
        }

        private void modulateKeyPress()
        {
            int dummy = 1;
            while (true)
            {
                VirtualKeyboard.KeyDown(key);
                //Thread.Sleep(1);
                //for (int i = 0; i < (int)(multiplier_on * (intensity)); i++)
                //{
                //    //dummy step
                //    dummy = -dummy;
                //}
                Thread.Sleep(Math.Max(0, (int)(sleep_time*(intensity/max_intensity))));
                VirtualKeyboard.KeyUp(key);
                //for (int i = 0; i < (int)(multiplier_off * (max_intensity - intensity)); i++)
                //{
                //    //dummy step
                //    dummy = -dummy;
                //}
                //Thread.Sleep(1);
                Thread.Sleep(Math.Max(0, (int)(sleep_time*0.7*(1-(intensity / max_intensity)))));
            }
        }

        public void pressKey()
        {
            PWMThread = new Thread(modulateKeyPress);
            PWMThread.Start();
            Console.WriteLine("PWM started for " + key.ToString());
        }

        public void releaseKey()
        {
            PWMThread.Suspend();
            VirtualKeyboard.KeyUp(key);
            Console.WriteLine("PWM stopped for " + key.ToString());
        }

    }
}
