using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using NAudio;

using System.Linq;
using System.Threading;
using AForge.Math;
using System.Threading.Tasks;

namespace HorizonDrive
{ 
    class AudioVisualizer
    {
        public char division = '#';
        private static string tmp = "";
        static int inverted_size = 20;
        public void ConsoleAudioVisualizer(char division = '#')
        {
            Thread thr2 = new Thread(AudioVisualizer.ConsoleInput);
            thr2.Start();
            this.division = division;
            WaveInEvent waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = 1, // indicates which microphone to use
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = 22
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();

        }

        void  WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
        {
            Int16[] values = new Int16[1024];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
            int sampleRate = 44100;
            List<double> complices = new List<double>();
            foreach (var item in values)
            {
                complices.Add(item);
            }
            var tmp = complices.ToArray();

            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(tmp);
            double[] psd = FftSharp.FFT.Power(spectrum);
            double[] freq = FftSharp.FFT.FrequencyScale(psd.Length, sampleRate);
            for (int i = 0; i < psd.Length - inverted_size; i += inverted_size)
            {
                string bar = division.ToString();
                var count = psd.ToList().GetRange(i, inverted_size).Average();
                if (count > 0)
                {
                    bar = new(division, (int)(count));
                }
                Console.WriteLine(bar);
            }
            Console.WriteLine(AudioVisualizer.tmp);
            Thread.Sleep(1);
            Console.Clear();
        }
        static void ConsoleInput()
        {
            while(true)
            {
                char keyPressed = Console.ReadKey().KeyChar;
                AudioVisualizer.tmp += keyPressed;
                CheckCommand();
            }
            
        }

        static void CheckCommand()
        {
            if (tmp.Contains("color"))
            {
                if (tmp.Contains("red"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    tmp = "";
                }
                if (tmp.Contains("green"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    tmp = "";
                }
                if (tmp.Contains("cyan"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    tmp = "";
                }
                if (tmp.Contains("magenta"))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    tmp = "";
                }

            }
            if (tmp.Contains("size"))
            {
                try
                {
                    var x = tmp.Remove(tmp.Length - 4, 4);
                    double test;
                    inverted_size = Double.TryParse(x,out test) ? Convert.ToInt32(x) : 20;
                }
                catch (Exception)
                {
                }
                tmp = "";
                

            }

        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            Console.CursorVisible = false;
            // Printing the current dimensions
            Console.ForegroundColor = ConsoleColor.Green;
            AudioVisualizer audioVisualizer = new AudioVisualizer();
            audioVisualizer.ConsoleAudioVisualizer();

            Console.WriteLine("C# Audio Level Meter");
            Console.WriteLine("(press any key to exit)");
            Console.ReadLine();


        }
    }
}
