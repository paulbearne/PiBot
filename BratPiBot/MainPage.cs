using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace BratPiBot
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public partial class MainPage : Page
    {
        // This is used on the main page as the title of the sample.
        public const string FEATURE_NAME = "Brat Biped Robot";

        // Change the array below to reflect the name of your scenarios.
        // This will be used to populate the list of scenarios on the main page with
        // which the user will choose the specific scenario that they are interested in.
        // These should be in the form: "Navigating to a web page".
        // The code in MainPage will take care of turning this into: "1) Navigating to a web page"
        List<Scenario> scenarios = new List<Scenario>
        {
          //  new Scenario() { Title = "Bot Central", ClassType = typeof(BratPiBot.MasterPage) },
            new Scenario() { Title = "Servo Controls", ClassType = typeof(BratPiBot.MaestroSerialPage) },
           // new Scenario() { Title = "Sensor Controls", ClassType = typeof(BratPiBot.SensorsPage) },
           // new Scenario() { Title = "Video Controls", ClassType = typeof(BratPiBot.VideoPage) },
            new Scenario() { Title = "Audio Controls", ClassType = typeof(BratPiBot.AudioPage) }
            //new Scenario() { Title = "Servers", ClassType = typeof(BratPiBot.ClientServerPage) }
        };
    }

    public class Scenario
    {
        public string Title { get; set; }

        public Type ClassType { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
