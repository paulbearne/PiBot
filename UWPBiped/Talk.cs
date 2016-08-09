using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPBiped
{
    // wrapper for tts
    public sealed class Talk
    {
        public Talk()
        {

        }

        public static void say(string msg)
        {
            Variables.voice.say(msg);
        }
    }
}
