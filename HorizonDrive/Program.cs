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
using System.Diagnostics;

namespace HorizonDrive
{ 
    class AudioVisualizer
    {
        public static char division = '#';
        private static string text = "";
        static int inverted_size = 20;
        static int buffer = 22;
        static int speed = 17;
        static WaveInEvent waveIn;
        static bool inverted = false;
        static int buffer_size = (int)Math.Pow(2, (int)(Math.Log(buffer * 89) / Math.Log(2)) + 1);


        public void ConsoleAudioVisualizer(char division = '#')
        {
            Thread thr2 = new Thread(AudioVisualizer.ConsoleInput);
            thr2.Start();
            AudioVisualizer.division = division;
            waveIn = new NAudio.Wave.WaveInEvent

            {
                DeviceNumber = 1, // indicates which microphone to use
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = buffer
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();
           

        }

        void  WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
        {
            Int16[] values = new Int16[buffer_size];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
            int sampleRate = 44100;
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(values.Select(x => (double)x).ToArray());
            double[] psd = FftSharp.FFT.Power(spectrum);
            ConsoleOutput(psd);
            Console.WriteLine(AudioVisualizer.text);
            var durationTicks = Math.Round(0.001 * speed * Stopwatch.Frequency);
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedTicks < durationTicks)
            {

            }
            Console.Clear();
        }

        static void ConsoleOutput(double[] psd)
        {
            if (inverted)
            {
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
            }
            else
            {
                List<char[]> bars = new List<char[]>();
                for (int i = 0; i < psd.Length - inverted_size; i += inverted_size)
                {
                    string bar;

                    var count = psd.ToList().GetRange(i, inverted_size).Average();
                    if (count > 0)
                    {
                        bar = new(division, (int)(count));
                        if (count < 20)
                        {
                            string test = new(' ', (int)(20 - count));
                            bar += test;
                        }
                    }
                    else
                    {
                        bar = new(' ', (int)(20));
                    }
                    bar = bar.Insert(1, "0");
                    bars.Add(bar.ToCharArray());
                }
                char[][] tmp = bars.ToArray();
                if (tmp.Length > 1)
                {
                    for (int i = 19; i > 0; i--)
                    {
                        char[] bar;
                        bar = tmp.Select(v => v[i]).ToArray();
                        Console.WriteLine(bar);
                    }
                }
            }
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
                for (int i = 0; i < words.Length; i++)
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
                                inverted_size = Int32.TryParse(size, out int test) ? Convert.ToInt32(size) : 20;
                            }
                            catch (Exception)
                            {
                            }
                            break;

                        case "bar":
                            AudioVisualizer.division = words[i + 1][0];
                            break;
                        case "speed":
                            try
                            {
                                string tmp_speed = words[i + 1];
                                speed = Int32.TryParse(tmp_speed, out int test) ? Convert.ToInt32(tmp_speed) : 17;
                            }
                            catch (Exception)
                            {
                            }
                            break;

                        case "invert":
                            {
                                inverted = !inverted;
                                i++;
                            }
                            break;

                        case "buffer":
                            try
                            {
                                string tmp_buffer = words[i + 1];
                                buffer = Int32.TryParse(tmp_buffer, out int test) ? Convert.ToInt32(tmp_buffer) : buffer;
                                buffer_size = (int)Math.Pow(2, (int)(Math.Log(buffer * 89) / Math.Log(2)) + 1);
                                waveIn.BufferMilliseconds = buffer;

                            }
                            catch (Exception)
                            {
                            }
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
            Console.ForegroundColor = ConsoleColor.Green;
            AudioVisualizer audioVisualizer = new AudioVisualizer();
            audioVisualizer.ConsoleAudioVisualizer();
            Console.WriteLine("C# AudioVisualizer");
            //Console.ReadLine();
        }
    }
}
