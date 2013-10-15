using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

using gpserv;

using vJoyWrapper;

using System.Runtime.InteropServices;

namespace gpserv
{
    class Program
    {

        static void createListener()
        {
            // Create an instance of the TcpListener class.
            TcpListener tcpListener = null;
            
            //IPAddress ipAddress = new IPAddress(0x6501A8C0);
            IPAddress ipAddress = new IPAddress(0x6401A8C0);
            try
            {
                // Set the listener on the local IP address 
                // and specify the port.
                tcpListener = new TcpListener(ipAddress, 1234);
                tcpListener.Start();
                Console.WriteLine( "Waiting for a connection...");
            }
            catch (Exception e)
            {
                var output = "Error: " + e.ToString();
                Console.WriteLine(output);
            }
            while (true)
            {
                // Always use a Sleep call in a while(true) loop 
                // to avoid locking up your CPU.
                Thread.Sleep(70);
                // Create a TCP socket. 
                // If you ran this server on the desktop, you could use 
                // Socket socket = tcpListener.AcceptSocket() 
                // for greater flexibility.
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                // Read the data stream from the client.
                while (true)
                {
                    Thread.Sleep(5);
                    byte[] bytes = new byte[50];
                    NetworkStream stream = tcpClient.GetStream();
                    //if (!stream.DataAvailable) break;
                    stream.Read(bytes, 0, bytes.Length);
                    SocketHelper helper = new SocketHelper();
                    helper.processMsg(tcpClient, stream, bytes);
                }
            }
        }


        static void Main(string[] args)
        {

            //while (true)
            //{
            //    Thread.Sleep(50);
            //    SendKeys.SendWait("{UP}");
            //}

            createListener();

            //VJoy gp = new VJoy();
            //gp.SetAxis(VJoy.JoystickAxis.AxisX, (ushort)VJoy.AxisMax);
        }
    }
}

class SocketHelper
{
    TcpClient mscClient;
    string mstrMessage;
    string mstrResponse;
    byte[] bytesSent;
    static bool adown = false;
    static bool zdown = false;
    static bool ldown = false;
    static bool rdown = false;

    static deque<char> buffer = new deque<char>(1000000);
    static deque<char> temp = new deque<char>(50);
    static deque<char> temp2 = new deque<char>(50);
    static int hashes = 0;

    static bool insidebraces = false;

    static PWMKeyPress ControllerA = new PWMKeyPress(Keys.A, 50);
    static PWMKeyPress ControllerZ = new PWMKeyPress(Keys.Z, 50);
    static PWMKeyPress ControllerL = new PWMKeyPress(Keys.Left, 20);
    static PWMKeyPress ControllerR = new PWMKeyPress(Keys.Right, 20);


    public void processMsg(TcpClient client, NetworkStream stream, byte[] bytesReceived)
    {
        // Handle the message received and  
        // send a response back to the client.
        mstrMessage = Encoding.ASCII.GetString(bytesReceived, 0, bytesReceived.Length);
        mscClient = client;


        for (int i = 0; i < mstrMessage.Length && mstrMessage[i] != '\0'; i++)
        {
            buffer.push_back(mstrMessage[i]);
        }

        while (!buffer.empty())
        {
            if (insidebraces)
            {
                if (buffer.front() == '}')
                {
                    insidebraces = false;
                    //handle l, r presses
                    if (temp2.v[0] == 'p' && temp2.v[1] == 'l')
                    {
                        Console.WriteLine("left pressed");
                        VirtualKeyboard.KeyDown(Keys.Left);
                    }
                    else if (temp2.v[0] == 'r' && temp2.v[1] == 'l')
                    {
                        Console.WriteLine("left released");
                        VirtualKeyboard.KeyUp(Keys.Left);
                    }
                    else if (temp2.v[0] == 'p' && temp2.v[1] == 'r')
                    {
                        Console.WriteLine("right pressed");
                        VirtualKeyboard.KeyDown(Keys.Right);
                    }
                    else if (temp2.v[0] == 'r' && temp2.v[1] == 'r')
                    {
                        Console.WriteLine("right released");
                        VirtualKeyboard.KeyUp(Keys.Right);
                    }
                    temp2.clear();
                }
                else
                {
                    temp2.push_back(buffer.front());
                }
                buffer.pop_front();
            }
            else
            {
                if (buffer.front() == '{')
                {
                    insidebraces = true;
                }
                else
                {
                    temp.push_back(buffer.front());
                    if (temp.back() == '#') hashes++;
                }
                buffer.pop_front();
                if (hashes == 3) break;
            }

        }

        if (hashes < 3)
        {
            return;
        }

        mstrMessage = new String(temp.v, 0, temp.length);
        hashes = 0;
        temp.clear();

        //mstrMessage = mstrMessage.Substring(0, 5);
        //return;
        //float x;
        char[] sep = new char[1];
        sep[0] = '#';
        string[] scoms = mstrMessage.Split(sep);
        float[] fcoms = new float[3];
        
        bool[] pos = new bool[3];
        pos[0] = float.TryParse(scoms[0], out fcoms[0]);
        pos[1] = float.TryParse(scoms[1], out fcoms[1]);
        pos[2] = float.TryParse(scoms[2], out fcoms[2]);
        //Console.WriteLine(x);

        float sensitivityV = 0.08f;
        float sensitivityH = 0.05f;

        ControllerA.intensity = (Math.Abs(fcoms[2]) - sensitivityV) * 0.5f;
        ControllerA.max_intensity = (Math.Abs(0.6f) - sensitivityV) * 0.5f;

        ControllerZ.intensity = (Math.Abs(fcoms[2]) - sensitivityV) * 0.5f;
        ControllerZ.max_intensity = (Math.Abs(0.6f) - sensitivityV) * 0.5f;

        ControllerL.intensity = (Math.Abs(fcoms[1]) - sensitivityH) * 1f;
        ControllerL.max_intensity = (Math.Abs(0.6f) - sensitivityH) * 0.5f;

        ControllerR.intensity = (Math.Abs(fcoms[1]) - sensitivityH) * 1f;
        ControllerR.max_intensity = (Math.Abs(0.6f) - sensitivityH) * 0.5f;

        //if (pos[1] && fcoms[1] > sensitivityH && ldown == false)
        //{
        //    ldown = true;
        //    ControllerL.pressKey();
        //}
        //else if (pos[1] && fcoms[1] < sensitivityH && fcoms[1] > -sensitivityH && ldown == true)
        //{
        //    ldown = false;
        //    ControllerL.releaseKey();
        //}

        //if (pos[1] && fcoms[1] < -sensitivityH && rdown == false)
        //{
        //    rdown = true;
        //    ControllerR.pressKey();
        //}
        //else if (pos[1] && fcoms[1] < sensitivityH && fcoms[1] > -sensitivityH && rdown == true)
        //{
        //    rdown = false;
        //    ControllerR.releaseKey();
        //}

        //intensity = (Math.Abs(fcoms[2]) - sensitivity) * 0.5f;
        //bool firsttime = true;
        if (pos[2] && fcoms[2] > sensitivityV && adown == false)
        {
            adown = true;
            ControllerA.pressKey();

        }
        else if (pos[2] && fcoms[2] < sensitivityV && fcoms[2] > -sensitivityV && adown == true)
        {
            adown = false;
            ControllerA.releaseKey();
        }

        if (pos[2] && fcoms[2] < -sensitivityV && zdown == false)
        {
            zdown = true;
            ControllerZ.pressKey();

        }
        else if (pos[2] && fcoms[2] < sensitivityV && fcoms[2] > -sensitivityV && zdown == true)
        {
            zdown = false;
            ControllerZ.releaseKey();
        }

        //if (pos[2] && fcoms[2] < -sensitivity && zdown == false)
        //{
        //    zdown = true;
        //    VirtualKeyboard.KeyDown(Keys.Z);
        //    Console.WriteLine("Z pressed");
        //}
        //else if (pos[2] && fcoms[2] > -sensitivity && fcoms[2] < sensitivity && zdown == true)
        //{
        //    zdown = false;
        //    VirtualKeyboard.KeyUp(Keys.Z);
        //    Console.WriteLine("Z released");
        //}

        //if (pos[1] && fcoms[1] > sensitivity && ldown == false)
        //{
        //    ldown = true;
        //    VirtualKeyboard.KeyDown(Keys.Left);
        //    Console.WriteLine("<- pressed");
        //}
        //else if (pos[1] && fcoms[1] < sensitivity && fcoms[1]>-sensitivity && ldown == true)
        //{
        //    ldown = false;
        //    VirtualKeyboard.KeyUp(Keys.Left);
        //    Console.WriteLine("<- released");
        //}

        //if (pos[1] && fcoms[1] < -sensitivity && rdown == false)
        //{
        //    rdown = true;
        //    VirtualKeyboard.KeyDown(Keys.Right);
        //    Console.WriteLine("-> pressed");
        //}
        //else if (pos[1] && fcoms[1] > -sensitivity && fcoms[1]<sensitivity && rdown == true)
        //{
        //    rdown = false;
        //    VirtualKeyboard.KeyUp(Keys.Right);
        //    Console.WriteLine("-> released");
        //}

        if (mstrMessage.Equals("Hello"))
        {
            mstrResponse = "Goodbye";
        }
        else
        {
            mstrResponse = "What?";
        }
        
        bytesSent = Encoding.ASCII.GetBytes(mstrResponse);
        stream.Write(bytesSent, 0, bytesSent.Length);
    }
}
