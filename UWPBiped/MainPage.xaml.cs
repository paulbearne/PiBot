using Windows.UI.Xaml.Controls;

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Command;
using System;
using Windows.Storage;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.IO;

namespace UWPBiped
{
    public sealed partial class MainPage : Page
    {
        private bool firstRun = true;
        StorageFolder localFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
        ChakraHostAsync host = new ChakraHostAsync();
        const string mostRecentScriptCacheBlocks = "last-script-blocks.xml";
        const string mostRecentScriptCacheJSCode = "last-script-jscode.js";
        string mostRecentBlocks = "";
        public MainPage()
        {
            this.InitializeComponent();
            firstRun = true;
            slVolume.Value = Variables.config.volume;
            // init the blockly web server
            startServer();   
            Variables.voice = new Speech(audio);
            Variables.voice.OnComplete += Voice_OnComplete;
            slVolume.Value = Variables.config.volume;
        }

        private async void startServer()
        {

            var publicFolder = await localFolder.GetFolderAsync("public");

            await StartMostRecentScript();

            var server = new WebServer();

            // include last saved script
            server.Get(
                "/",
                async (req, res) => { await WebServer.WriteStaticResponseFilter(req, res, publicFolder, IncludeLastScript); });
            server.Get(
                "/index.html",
                async (req, res) => { await WebServer.WriteStaticResponseFilter(req, res, publicFolder, IncludeLastScript); });

            server.UseStatic(publicFolder);

            server.Post("/runcode", async (req, res) =>
            {
                var code = req.GetValue("code");
                var blocks = req.GetValue("blocks");
                if (!String.IsNullOrEmpty(code))
                {
                    await SaveMostRecentScript(code, blocks);
                    host.runScriptAsync(code);
                }
                await res.RedirectAsync("..");
            });

            server.Post("/stopcode", async (req, res) =>
            {
                host.haltScript();
                await res.RedirectAsync("..");
            });

            server.Listen(10000);
        }

        private async Task StartMostRecentScript()
        {
            var storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var lastBlocks = await storageFolder.GetFileAsync(mostRecentScriptCacheBlocks);
                mostRecentBlocks = await FileIO.ReadTextAsync(lastBlocks);
            }
            catch (Exception)
            {
                // do nothing if we cannot open or read the file
            }

            try
            {
                var lastScript = await storageFolder.GetFileAsync(mostRecentScriptCacheJSCode);
                string jscode = await FileIO.ReadTextAsync(lastScript);
                host.runScriptAsync(jscode);
            }
            catch (Exception)
            {
                // do nothing if we cannot open or read the file
            }
        }

        private async Task SaveMostRecentScript(string jscode, string blocks)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            try
            {
                var lastScript = await storageFolder.CreateFileAsync(
                    mostRecentScriptCacheJSCode,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(lastScript, jscode);
            }
            catch (Exception)
            {
            }
            try
            {
                var lastScript = await storageFolder.CreateFileAsync(
                    mostRecentScriptCacheBlocks,
                    CreationCollisionOption.ReplaceExisting);
                // TODO: we should not hardcode this so much, but for now it's ok...
                var newBlocks = blocks.Replace(@"<xml xmlns=""http://www.w3.org/1999/xhtml"">", @"<xml id=""last-script"" style=""display: none"">");
                await FileIO.WriteTextAsync(lastScript, newBlocks);
            }
            catch (Exception)
            {
            }
        }

        private async Task<string> IncludeLastScript(Stream input)
        {
            var inputStream = input.AsInputStream();
            string result;
            using (var dataReader = new Windows.Storage.Streams.DataReader(input.AsInputStream()))
            {
                uint numBytesLoaded = await dataReader.LoadAsync((uint)input.Length);
                string text = dataReader.ReadString(numBytesLoaded);
                result = text.Replace(@"<!-- INCLUDE xml for last-script -->", mostRecentBlocks);
            }
            return result;
        }



        private void Voice_OnComplete()
        {
            
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            audio.Volume = slVolume.Value / 100;
           // if (firstRun == false)
            //{
            Variables.config.volume = (UInt16)(slVolume.Value);
           // }
           // firstRun = false;
        }

        private void btnExit_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Variables.config.Save();
            App.Current.Exit();
        }
    }
}
