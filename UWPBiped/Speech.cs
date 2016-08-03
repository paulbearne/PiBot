using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.SpeechSynthesis;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Media.Playback;

namespace UWPBiped
{
    public sealed partial class Speech
    {
        private SpeechSynthesizer synthesizer;
        private MediaElement media;

        public delegate void speechComplete();
        public event speechComplete OnComplete;


        public Speech(MediaElement parentmedia)
        {
            media = parentmedia;
            media.MediaEnded += Media_MediaEnded;
            synthesizer = new SpeechSynthesizer();
        }

        

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (OnComplete != null)
            {
                OnComplete();
            }
        }

        public void say(string text)
        {
            speak(text);
        }
        private async void speak(string textToSynthesize)
        {
            // If the media is playing, the user has pressed the button to stop the playback.
            if (media.CurrentState.Equals(Windows.UI.Xaml.Media.MediaElementState.Playing))
            {
                media.Stop();

            }
            else
            {
                string text = textToSynthesize;
                if (!String.IsNullOrEmpty(text))
                {
                    try
                    {
                        // Create a stream from the text. This will be played using a media element.
                        SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

                        // Set the source and start playing the synthesized audio stream.
                        media.AutoPlay = true;
                        media.SetSource(synthesisStream, synthesisStream.ContentType);
                        media.Play();
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // If media player components are unavailable, (eg, using a N SKU of windows), we won't
                        // be able to start media playback. Handle this gracefully
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                        await messageDialog.ShowAsync();
                    }
                    catch (Exception)
                    {
                        // If the text is unable to be synthesized, throw an error message to the user.

                        media.AutoPlay = false;
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }


    }
}
