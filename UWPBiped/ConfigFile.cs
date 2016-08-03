using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWPBiped
{
    public class servo
    {
        private UInt16 Speed;
        private UInt16 Acceleration;
        private UInt16 Target;

        public servo()
        {
            Speed = new UInt16();
            Acceleration = new UInt16();
            Target = new UInt16();
        }

        public UInt16 speed {
            get
            {
                return Speed;
            }
            set
            {
                Speed = value;
            }
        }

        public UInt16 acceleration
        {
            get
            {
                return Acceleration;
            }
            set
            {
                Acceleration = value;
            }
        }

        public UInt16 target
        {
            get
            {
                return Target;
            }
            set
            {
                Target = value;
            }
        }

       
    }

    public class servos
    {
        private servo LeftHip;
        private servo LeftLeg;
        private servo LeftAnkle;
        private servo RightHip;
        private servo RightLeg;
        private servo RightAnkle;

        public servos(){
            LeftHip = new servo();
            LeftLeg = new servo();
            LeftAnkle = new servo();
            RightHip = new servo();
            RightLeg = new servo();
            RightAnkle = new servo();
        }

        public servo lefthip
        {
            get
            {
                return LeftHip;
            }
            set
            {
                LeftHip = value;
            }
        }

        public servo leftleg
        {
            get
            {
                return LeftLeg;
            }
            set
            {
                LeftLeg = value;
            }
        }

        public servo leftankle
        {
            get
            {
                return LeftAnkle;
            }
            set
            {
                LeftAnkle = value;
            }
        }

        public servo righthip
        {
            get
            {
                return RightHip;
            }
            set
            {
                RightHip = value;
            }
        }

        public servo rightleg
        {
            get
            {
                return RightLeg;
            }
            set
            {
                RightLeg = value;
            }
        }

        public servo rightankle
        {
            get
            {
                return RightAnkle;
            }
            set
            {
                RightAnkle = value;
            }
        }

    }



    public sealed partial class ConfigData
    {

        private servos servoCalValues;
        private UInt16 Volume;

        public ConfigData(){
            servoCalValues = new servos();
            Volume = new UInt16();
        }

        public servos servocal
        {
            get
            {
                return servoCalValues;
            }
            set
            {
                servoCalValues = value;
            }
        }

        public UInt16 volume{
            get {
                return Volume;
             }
            set {
                Volume = value;
            }
        }

        // setup with default values
        public void initValues()
        {
            servoCalValues.leftankle.acceleration = 50;
            servoCalValues.leftankle.speed = 50;
            servoCalValues.leftankle.target = 1500;
            servoCalValues.leftleg.acceleration = 50;
            servoCalValues.leftleg.speed = 50;
            servoCalValues.leftleg.target = 1500;
            servoCalValues.lefthip.acceleration = 50;
            servoCalValues.lefthip.speed = 50;
            servoCalValues.lefthip.target = 1500;
            servoCalValues.rightankle.acceleration = 50;
            servoCalValues.rightankle.speed = 50;
            servoCalValues.rightankle.target = 1500;
            servoCalValues.rightleg.acceleration = 50;
            servoCalValues.rightleg.speed = 50;
            servoCalValues.rightleg.target = 1500;
            servoCalValues.righthip.acceleration = 50;
            servoCalValues.righthip.speed = 50;
            servoCalValues.righthip.target = 1500;
            volume = 20;

        }

        public bool configExists()
        {
           try
            {
                string appdir = ApplicationData.Current.LocalFolder.Path;
                using (StreamReader reader = File.OpenText(appdir + "\\config.dat"))
                {
                    if (reader != null)
                    {
                        // got a reader so get the data
                        Volume = UInt16.Parse(reader.ReadLine());
                        servoCalValues.leftankle.acceleration = UInt16.Parse(reader.ReadLine());
                        servoCalValues.leftankle.speed = UInt16.Parse(reader.ReadLine());
                        servoCalValues.leftankle.target = UInt16.Parse(reader.ReadLine());
                        servoCalValues.leftleg.acceleration = UInt16.Parse(reader.ReadLine());
                        servoCalValues.leftleg.speed = UInt16.Parse(reader.ReadLine());
                        servoCalValues.leftleg.target = UInt16.Parse(reader.ReadLine());
                        servoCalValues.lefthip.acceleration = UInt16.Parse(reader.ReadLine());
                        servoCalValues.lefthip.speed = UInt16.Parse(reader.ReadLine());
                        servoCalValues.lefthip.target = UInt16.Parse(reader.ReadLine());
                        servoCalValues.rightankle.acceleration = UInt16.Parse(reader.ReadLine());
                        servoCalValues.rightankle.speed = UInt16.Parse(reader.ReadLine());
                        servoCalValues.rightankle.target = UInt16.Parse(reader.ReadLine());
                        servoCalValues.rightleg.acceleration = UInt16.Parse(reader.ReadLine());
                        servoCalValues.rightleg.speed = UInt16.Parse(reader.ReadLine());
                        servoCalValues.rightleg.target = UInt16.Parse(reader.ReadLine());
                        servoCalValues.righthip.acceleration = UInt16.Parse(reader.ReadLine());
                        servoCalValues.righthip.speed = UInt16.Parse(reader.ReadLine());
                        servoCalValues.righthip.target = UInt16.Parse(reader.ReadLine());
                        reader.Dispose();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                return false;

            }
        }

        public void Save()
        {
            //configFile = await storageFolder.GetFileAsync("config.dat");
            try
            {
                string appdir = ApplicationData.Current.LocalFolder.Path;
                using (StreamWriter writer = File.CreateText(appdir + "\\config.dat"))
                {
                    writer.WriteLine(Volume);
                    writer.WriteLine(servoCalValues.leftankle.acceleration);
                    writer.WriteLine(servoCalValues.leftankle.speed);
                    writer.WriteLine(servoCalValues.leftankle.target);
                    writer.WriteLine(servoCalValues.leftleg.acceleration);
                    writer.WriteLine(servoCalValues.leftleg.speed);
                    writer.WriteLine(servoCalValues.leftleg.target);
                    writer.WriteLine(servoCalValues.lefthip.acceleration);
                    writer.WriteLine(servoCalValues.lefthip.speed);
                    writer.WriteLine(servoCalValues.lefthip.target);
                    writer.WriteLine(servoCalValues.rightankle.acceleration);
                    writer.WriteLine(servoCalValues.rightankle.speed);
                    writer.WriteLine(servoCalValues.rightankle.target);
                    writer.WriteLine(servoCalValues.rightleg.acceleration);
                    writer.WriteLine(servoCalValues.rightleg.speed);
                    writer.WriteLine(servoCalValues.rightleg.target);
                    writer.WriteLine(servoCalValues.righthip.acceleration);
                    writer.WriteLine(servoCalValues.righthip.speed);
                    writer.WriteLine(servoCalValues.righthip.target);
                    writer.Flush();
                    writer.Dispose();

                }
            } catch(Exception e)
            {

            }
            

        }

        public void Load()
        {
            try
            {
                using (StreamReader reader = File.OpenText(ApplicationData.Current.LocalFolder + "\\config.dat"))
                {
                    Volume = UInt16.Parse(reader.ReadLine());
                    servoCalValues.leftankle.acceleration = UInt16.Parse(reader.ReadLine());
                    servoCalValues.leftankle.speed = UInt16.Parse(reader.ReadLine());
                    servoCalValues.leftankle.target = UInt16.Parse(reader.ReadLine());
                    servoCalValues.leftleg.acceleration = UInt16.Parse(reader.ReadLine());
                    servoCalValues.leftleg.speed = UInt16.Parse(reader.ReadLine());
                    servoCalValues.leftleg.target = UInt16.Parse(reader.ReadLine());
                    servoCalValues.lefthip.acceleration = UInt16.Parse(reader.ReadLine());
                    servoCalValues.lefthip.speed = UInt16.Parse(reader.ReadLine());
                    servoCalValues.lefthip.target = UInt16.Parse(reader.ReadLine());
                    servoCalValues.rightankle.acceleration = UInt16.Parse(reader.ReadLine());
                    servoCalValues.rightankle.speed = UInt16.Parse(reader.ReadLine());
                    servoCalValues.rightankle.target = UInt16.Parse(reader.ReadLine());
                    servoCalValues.rightleg.acceleration = UInt16.Parse(reader.ReadLine());
                    servoCalValues.rightleg.speed = UInt16.Parse(reader.ReadLine());
                    servoCalValues.rightleg.target = UInt16.Parse(reader.ReadLine());
                    servoCalValues.righthip.acceleration = UInt16.Parse(reader.ReadLine());
                    servoCalValues.righthip.speed = UInt16.Parse(reader.ReadLine());
                    servoCalValues.righthip.target = UInt16.Parse(reader.ReadLine());
                }
            } catch(Exception e)
            {
                
            }
        }

    }
}
