using System;
using System.Collections.Generic;
using NAudio.Wave;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace HorizonDrive
{
    public abstract class AudioVisualizer
    {
        public abstract void Start();
        public abstract void Output(double[] psd);
        protected abstract void WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e);
    }

    class ConsoleAudioVisualizer : AudioVisualizer
    {
        public char division = '#';
        string text = "";
        int inverted_size = 20;
        int buffer = 22;
        int speed = 17;
        WaveInEvent waveIn;
        bool inverted = false;
        int buffer_size;
        public ConsoleAudioVisualizer(char new_division = '#')
        {
            buffer_size = (int)Math.Pow(2, (int)(Math.Log(buffer * 89) / Math.Log(2)) + 1);
            this.division = new_division;
        }

        public override void Start()
        {
            Thread thr2 = new Thread(this.Input);
            thr2.Start();
            waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = 0, // indicates which microphone to use
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = buffer
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();
        }

        protected override void WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
        {
            Int16[] values = new Int16[buffer_size];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(values.Select(x => (double)x).ToArray());
            double[] psd = FftSharp.FFT.Power(spectrum);
            Output(psd);
        }
         public override void Output(double[] psd)
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
                    bar = bar.Insert(1, division.ToString());
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
            Console.WriteLine(text);
            var durationTicks = Math.Round(0.001 * speed * Stopwatch.Frequency);
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedTicks < durationTicks)
            {

            }
            Console.Clear();
        }

        void Input()
        {
            while (true)
            {
                char keyPressed = Console.ReadKey().KeyChar;
                this.text += keyPressed;
                if (text.Contains(";"))
                {
                    text = Backspace_Support(text);
                    var words = text.ToLower().TrimEnd(';').Split(' ');
                    CheckCommand(words);
                }
            }
        }



        void CheckCommand(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                switch (words[i])
                {
                    case "color":
                        ChangeColor(words[i + 1]);
                        break;
                    case "size":
                        try
                        {
                            string size = words[i + 1];
                            this.inverted_size = Int32.TryParse(size, out int test) ? Convert.ToInt32(size) : 20;
                        }
                        catch (Exception)
                        {
                        }
                        break;

                    case "bar":
                        division = words[i + 1][0];
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
                        ChangeBuffer(words[i + 1]);
                        break;

                    default:
                        break;
                }
            }
            text = "";
        }
        static string Backspace_Support(string text_to_edit)
        {
            var index = text_to_edit.IndexOf('\b');
            while (index != -1)// loop for backspace support
            {
                if (index == 0)
                {
                    text_to_edit = text_to_edit.Remove(index, 1);
                }
                else
                {
                    text_to_edit = text_to_edit.Remove(index - 1, 2);
                }
                index = text_to_edit.IndexOf('\b');
            }
            return text_to_edit;
        }
        void ChangeBuffer(string buffer_new_value)
        {
            try
            {
                buffer = Int32.TryParse(buffer_new_value, out int test) ? Convert.ToInt32(buffer_new_value) : buffer;
                buffer_size = (int)Math.Pow(2, (int)(Math.Log(buffer * 89) / Math.Log(2)) + 1);
                waveIn.BufferMilliseconds = buffer;

            }
            catch (Exception)
            {
            }
        }
        void ChangeColor(string color)
        {
            Random rnd = new Random();
            switch (color)
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
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            AudioVisualizer audioVisualizer = new ConsoleAudioVisualizer();
            audioVisualizer.Start();
            Console.WriteLine("C# AudioVisualizer");
            //Console.ReadLine();
        }
    }
}
