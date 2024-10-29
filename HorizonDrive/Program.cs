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
        public static char division = '#';
        private static string text = "";
        static int inverted_size = 20;

        public void ConsoleAudioVisualizer(char division = '#')
        {
            Thread thr2 = new Thread(AudioVisualizer.ConsoleInput);
            thr2.Start();
            AudioVisualizer.division = division;
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
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(values.Select(x => (double)x).ToArray());
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
            Console.WriteLine(AudioVisualizer.text);
            Thread.Sleep(1);
            Console.Clear();
        }
        static void ConsoleInput()
        {
            while(true)
            {
                char keyPressed = Console.ReadKey().KeyChar;
                AudioVisualizer.text += keyPressed;
                CheckCommand();
            }      
        }

        static void CheckCommand()
        {
            Random rnd = new Random();
            if (text.Contains(";"))
            {
                var words = text.ToLower().TrimEnd(';').Split(' ');
                for (int i = 0; i < words.Length-1; i++)
                {
                    switch (words[i])
                    {
                        case "color":
                            switch (words[i+1])
                            {
                                case "red":
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    break;
                                case "green":
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    break;
                                case "white":
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                                case "yellow":
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    break;
                                case "blue":
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    break;
                                case "magenta":
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    break;
                                case "cyan":
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    break;
                                case "gray":
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                    break;
                                case "random":
                                    Console.ForegroundColor = (ConsoleColor)rnd.Next(1, 15);
                                    break;
                            }
                            break;
                        case "size":
                            try
                            {
                                string size = words[i+1];
                                inverted_size = Double.TryParse(size, out double test) ? Convert.ToInt32(size) : 20;
                            }
                            catch (Exception)
                            {
                            }
                            break;

                        case "bar":
                            AudioVisualizer.division = words[i + 1][0];
                            break;

                        default:
                            break;
                    }
                }
                text = "";
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
