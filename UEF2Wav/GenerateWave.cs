using System;
using System.Collections.Generic;
using System.Text;

namespace UEF2Wav
{
   public class GenerateWave
    {
        //baud 1200
        //phase 180
        //sample rate 44100
        //stopBit 4
        public static Int16[] generateTone(int frequency, int cycles, double phase, int sampleRate)
        {
         
            var samples =  (int)Math.Floor((double)(sampleRate / frequency) * cycles);
            var array = new Int16[samples];
            for (var i = 0; i < samples; i++)
            {

                array[i] = (Int16)Math.Floor(Math.Sin(phase + (frequency * (2 * Math.PI) * i / sampleRate)) * 0x7fff);
            }
            return array;
        }

        public static Int16[] bit0(int sampleRate, double phase = 180)
        {
            var NewPhase = phase * (Math.PI / 180);
            var baud = 1200;
            return generateTone(baud, 1, NewPhase, sampleRate);
        }

        public static Int16[] bit1(int sampleRate, double phase = 180)
        {
            var NewPhase = phase * (Math.PI / 180);
            var baud = 1200;
            return generateTone(baud * 2, 2, NewPhase, sampleRate);
        }

        public static Int16[] stopBit(int sampleRate, double phase = 180)
        {
            var NewPhase = phase * (Math.PI / 180);
            var baud = 1200;
            return generateTone(baud * 2, 2, NewPhase, sampleRate);
        }

        public static Int16[] highWave(int sampleRate, double phase = 180 )
        {
            var NewPhase = phase * (Math.PI / 180);
            var baud = 1200;
            return generateTone(baud * 2, 1, NewPhase, sampleRate);
        }


    }
}
