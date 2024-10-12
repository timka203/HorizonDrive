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

namespace HorizonDrive
{
	class Program
	{
        static void Main(string[] args)
        {
 
        // Printing the current dimensions
            Console.WriteLine(Console.WindowWidth);
            Console.WriteLine(Console.WindowHeight);
            double[] signal = FftSharp.SampleData.SampleAudio1();
            int sampleRate = 48_000;
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            // calculate the power spectral density using FFT
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(signal);
            double[] psd = FftSharp.FFT.Power(spectrum);
            double[] freq = FftSharp.FFT.FrequencyScale(psd.Length, sampleRate);

            // plot the sample audio
            ScottPlot.Plot plt = new ScottPlot.Plot();
            ScottPlot.Plot myPlot = new();
            plt.Add.Scatter(freq, psd);
            plt.YLabel("Power (dB)");
            plt.XLabel("Frequency (Hz)");
            plt.SavePng("quickstart.png", 1200, 1800);

            var waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = 1, // indicates which microphone to use
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = 20
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.StartRecording();

            Console.WriteLine("C# Audio Level Meter");
            Console.WriteLine("(press any key to exit)");
            Console.ReadLine();


        }

        static void WaveIn_DataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
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
            int inverted_size = 10;
            for (int i = 0; i < psd.Length- inverted_size; i+= inverted_size)
            {
                string bar2 = "#";
                var count = psd.ToList().GetRange(i, inverted_size).Average();
                if ( count > 0)
                {
                    bar2 = new('#', (int)(count));
                }
                Console.WriteLine(bar2);

            }
            Thread.Sleep(1);
            Console.Clear();
        }
        }
}
