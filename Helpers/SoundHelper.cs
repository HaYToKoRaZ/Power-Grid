using System;
using System.IO;
using System.Media;

namespace PowerGridHaYTooL.Helpers
{
    /// <summary>
    /// Güç planı değişimlerinde (otomatik GPU geçişi veya kullanıcı tıklaması)
    /// çalınan modern, kısa ve yüksek teknolojili ses motoru.
    /// Standart Windows sistem seslerinden farklı, bağımsız ve ~135ms uzunluğundadır.
    /// </summary>
    public static class SoundHelper
    {
        private static SoundPlayer? _switchPlayer;
        private static bool _initialized = false;

        static SoundHelper()
        {
            InitPlayer();
        }

        private static void InitPlayer()
        {
            try
            {
                byte[] wavBytes = GenerateModernSwitchWav();
                var ms = new MemoryStream(wavBytes);
                _switchPlayer = new SoundPlayer(ms);
                _switchPlayer.Load();
                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }

        /// <summary>
        /// Güç profili değiştiğinde çalan fütüristik, yumuşak çift tonlu arpej bildirimi.
        /// Asenkron çalışır, arayüzü veya oyunu kesinlikle bekletmez.
        /// </summary>
        public static void PlayPlanSwitchSound()
        {
            try
            {
                if (_initialized && _switchPlayer != null)
                {
                    _switchPlayer.Play();
                }
            }
            catch { }
        }

        /// <summary>
        /// 44.1 kHz, 16-bit Mono PCM formatında yumuşak ve tatlı bir geçiş tonu üretir.
        /// 1. Faz (50ms): 523 Hz (C5) yumuşak yükseliş
        /// 2. Faz (85ms): 880 Hz (A5) + %25 harmonik parlaklık ve hızlı üstel sönümleme
        /// </summary>
        private static byte[] GenerateModernSwitchWav()
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * 0.135); // 135 milisaniye
            short[] samples = new short[totalSamples];

            int t1Samples = (int)(sampleRate * 0.050); // 50 ms
            int t2Samples = totalSamples - t1Samples;  // 85 ms

            double freq1 = 523.25; // C5
            double freq2 = 880.00; // A5

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double sampleVal;

                if (i < t1Samples)
                {
                    // Faz 1: Yumuşak yükseliş (Attack) ve 523Hz ton
                    double phaseProgress = (double)i / t1Samples;
                    double env = Math.Sin(phaseProgress * Math.PI * 0.5);
                    sampleVal = Math.Sin(2.0 * Math.PI * freq1 * t) * env;
                }
                else
                {
                    // Faz 2: 880Hz parlak ton + ikinci harmonik + temiz üstel sönümleme (Decay)
                    int i2 = i - t1Samples;
                    double progress = (double)i2 / t2Samples;
                    double env = Math.Exp(-progress * 4.2);
                    double wave = (0.75 * Math.Sin(2.0 * Math.PI * freq2 * t)) + 
                                  (0.25 * Math.Sin(2.0 * Math.PI * (freq2 * 2.0) * t));
                    sampleVal = wave * env;
                }

                // Kulak tırmalamayan tatlı ses seviyesi (~%50 tepe seviyesi)
                short pcm = (short)Math.Clamp(sampleVal * 16000.0, short.MinValue, short.MaxValue);
                samples[i] = pcm;
            }

            return BuildWavFile(samples, sampleRate);
        }

        private static byte[] BuildWavFile(short[] pcmSamples, int sampleRate)
        {
            int subChunk2Size = pcmSamples.Length * 2; // 16-bit = 2 bytes per sample
            int chunkSize = 36 + subChunk2Size;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // RIFF chunk descriptor
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(chunkSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E' });

            // "fmt " sub-chunk
            bw.Write(new char[] { 'f', 'm', 't', ' ' });
            bw.Write(16); // Subchunk1Size (16 for PCM)
            bw.Write((short)1); // AudioFormat (1 = PCM)
            bw.Write((short)1); // NumChannels (1 = Mono)
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2); // ByteRate
            bw.Write((short)2); // BlockAlign
            bw.Write((short)16); // BitsPerSample

            // "data" sub-chunk
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(subChunk2Size);

            for (int i = 0; i < pcmSamples.Length; i++)
            {
                bw.Write(pcmSamples[i]);
            }

            bw.Flush();
            return ms.ToArray();
        }
    }
}
