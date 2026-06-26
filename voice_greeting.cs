using System;
using System.IO;
using System.Media;

namespace CyberBot3
{
    public class voice_greeting
    {
        public void greet()
        {
            try
            {
                // Try same path-replacement trick as Part 2
                string auto_path = AppDomain.CurrentDomain.BaseDirectory
                    .Replace(@"\bin\Debug\", @"\greeting.wav")
                    .Replace(@"\bin\Release\", @"\greeting.wav");

                // Fallback: look next to the exe
                if (!File.Exists(auto_path))
                    auto_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (File.Exists(auto_path))
                {
                    SoundPlayer player = new SoundPlayer(auto_path);
                    player.Play();
                }
            }
            catch
            {
                // If audio fails, app still runs normally
            }
        }
    }
}
