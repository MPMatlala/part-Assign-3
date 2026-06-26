using System;
using System.IO;
using System.Media;

namespace CyberBot3
{//start of namespace
    public class voice_greeting
    {//start of class
        public void greet()
        {  //start of greet
            try
            {// start of try
                // path-replacement trick as Part 2
                string auto_path = AppDomain.CurrentDomain.BaseDirectory
                    .Replace(@"\bin\Debug\", @"\greeting.wav")
                    .Replace(@"\bin\Release\", @"\greeting.wav");

                // Fallback: look next to the exe
                if (!File.Exists(auto_path))
                    auto_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (File.Exists(auto_path))
                {// start of if
                    SoundPlayer player = new SoundPlayer(auto_path);
                    player.Play();
                }// end of if
            }// end of try
            catch
            {// start of catch
                // If audio fails, app still runs normally
            }// end of catch
        }//end of greet
    }//end of class
}//end of namespace
