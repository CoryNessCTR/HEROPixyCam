using System;
using Microsoft.SPOT;

namespace HEROPixyCam
{
    class Link2UART : LinkType
    {
        public const int PIXY_UART_BAUDRATE = 115200;


        private System.IO.Ports.SerialPort m_uartPort;

        private byte[] m_localBuf;
        private byte start;
        private byte end;

#if true //Proper Implementation, use this from now on
        private void GetDataHandler(
                                object sender,
                                System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int tmp = ((System.IO.Ports.SerialPort)sender).BytesToRead;
            if(end + tmp < 0x100)
                ((System.IO.Ports.SerialPort)sender).Read(m_localBuf, end, tmp);
            else
            {
                ((System.IO.Ports.SerialPort)sender).Read(m_localBuf, end, 255 - end);
                ((System.IO.Ports.SerialPort)sender).Read(m_localBuf, 0, tmp - (255 - end));
            }
            end += (byte)tmp;
        }
#else //Leaky? causes a SystemOutOfMemoryException if using this method
        private void GetDataHandler(
                        object sender,
                        System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while (((System.IO.Ports.SerialPort)sender).BytesToRead > 0)
            {
                if ((byte)(end + 1) == start)
                {
                    bufferOverflow = true;
                    break;
                }
                m_localBuf[end++] = (byte)((System.IO.Ports.SerialPort)sender).ReadByte();
            }
        }
#endif
        public override void ClearBuffer()
        {
            start = end = 0;
            bufferOverflow = false;
        }
        public override byte bufferSize { get { return (byte)(end - start); } }

        public override byte Open(CTRE.Gadgeteer.IPortUart portDef, int arg)
        {

            m_uartPort = new System.IO.Ports.SerialPort(portDef.UART, arg, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            m_uartPort.DataReceived += GetDataHandler;
            m_uartPort.Open();

            m_localBuf = new byte[256];
            start = end = 0;
            return 0;
        }

        public override void Close()
        {
        }

        public override int Recv(byte[] buf, byte len)
        {
            UInt16 tmp = 0;
            return Recv(buf, len, ref tmp);
        }

        public override int Recv(byte[] buf, byte len, ref UInt16 cs)
        {
            cs = 0;
            if (bufferSize >= len)
            {
                for(int i = 0; i < len; i++)
                {
                    cs += m_localBuf[(byte)(i + start)];
                }
                if(start + len < 0x100)
                    Array.Copy(m_localBuf, start, buf, 0, len);
                else
                {
                    Array.Copy(m_localBuf, start, buf, 0, 255 - start);
                    Array.Copy(m_localBuf, 0, buf, 255 - start, len - (255 - start));
                }
                start += len;
                return len;
            }
            else
                return -1;
        }

        public override int Send(byte[] buf, byte len)
        {
            m_uartPort.Write(buf, 0, len);
            return len;
        }
    }

    class Pixy2UART : TPixy2
    {
        public Pixy2UART(CTRE.Gadgeteer.IPortUart portDef, int baudrate = Link2UART.PIXY_UART_BAUDRATE) : base()
        {
            m_link = new Link2UART();
            m_link.Open(portDef, baudrate);
        }
    }
}
