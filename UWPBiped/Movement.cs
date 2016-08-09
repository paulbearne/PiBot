using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPBiped
{
    public class BratMovement: PololuMaestro
    {

        private UInt16 last_angle;
        private UInt16 Speed;
        private UInt16 angle;
        private bool stepN;
        public BratMovement()
        {

        }


        private void GroupMove(UInt16 LH, UInt16 LK, UInt16 LA , UInt16 RH , UInt16 RK , UInt16 RA,UInt16 speed)
        {
            UInt16[] groupValues = { LH, LK, LA, RH, RK, RA };

            // maestro speed is 0.25us per 10ms tick so speed of 4 1us per tick to match original we need to 
            // multiply by speed by 4 then divide by 10 to match then wait until finished 
            speed *= 4;

            for (byte i= 0;i < 6; i++)
            {
                setSpeed(i, speed);
            }

            setTargets(0, groupValues);

            // were multitasking sort of so we need to run each command and wait until next 
            
            
        }

        public void walk()
        {

            if (stepN)
            {
                angle =(UInt16)( 90 + angle);
                GroupMove(last_angle, last_angle, 55, last_angle, last_angle, 75, Speed);
                GroupMove(angle, angle, 75, angle, angle, 75, Speed);
                GroupMove(angle, angle, 90, angle, angle, 90, Speed);
                stepN = !stepN;
            }
            else
            {
                angle = (UInt16)(90 - angle);
                GroupMove(last_angle, last_angle, 105, last_angle, last_angle, 125, Speed);
                GroupMove(angle, angle, 105, angle, angle, 105, Speed);
                GroupMove(angle, angle, 90, angle, angle, 90, Speed);
                stepN = !stepN;
            }
            last_angle = angle;
        }

        void Walk(byte angle)
        {
            if (stepN)
            {
                angle = 90 + angle;
                GroupMove(last_angle, last_angle, 55, last_angle, last_angle, 75, Speed);
                GroupMove(angle, angle, 75, angle, angle, 75, Speed);
                GroupMove(angle, angle, 90, angle, angle, 90, Speed);
                stepN = !stepN;
            }
            else
            {
                angle = 90 - angle;
                GroupMove(last_angle, last_angle, 105, last_angle, last_angle, 125, Speed);
                GroupMove(angle, angle, 105, angle, angle, 105, Speed);
                GroupMove(angle, angle, 90, angle, angle, 90, Speed);
                stepN = !stepN;
            }
            last_angle = angle;
        }

        void Turn_Left()
        {
            GroupMove(90, 90, 90, 90, 90, 90, 500);
            GroupMove(90, 90, 75, 90, 90, 55, 450);
            GroupMove(90, 90, 75, 90, 90, 75, 50);
            GroupMove(55, 55, 75, 55, 55, 75, 500);
            GroupMove(55, 55, 90, 55, 55, 90, 250);
            GroupMove(90, 90, 90, 90, 90, 90, 500);
            GroupMove(90, 90, 75, 90, 90, 55, 450);
            GroupMove(90, 90, 75, 90, 90, 75, 50);
            GroupMove(55, 55, 75, 55, 55, 75, 500);
            GroupMove(55, 55, 90, 55, 55, 90, 250);
            GroupMove(90, 90, 90, 90, 90, 90, 500);
        }

        void Get_Up_From_Front()
        {
            GroupMove(90, 90, 90, 90, 90, 90, 500);
            GroupMove(60, 0, 90, 120, 170, 90, 500);
            GroupMove(120, 0, 60, 120, 170, 90, 500);
            GroupMove(170, 0, 90, 10, 180, 90, 500);
            GroupMove(90, 90, 90, 90, 90, 90, 1000);
        }

        void Get_Up_From_Back()
        {
            GroupMove(90, 90, 90, 90, 90, 90, 500);
            GroupMove(90, 180, 90, 90, 0, 90, 500);
            GroupMove(90, 90, 90, 90, 90, 90, 500);
        }

        void Roll_Left()
        {
            GroupMove(90, 90, 90, 90, 90, 90, 500);
            GroupMove(90, 90, 90, 40, 90, 90, 100);
            GroupMove(90, 90, 90, 90, 90, 90, 500);
        }

        void Roll_Right()
        {
            GroupMove(90, 90, 90, 90, 90, 90, 500);
            GroupMove(140, 90, 90, 90, 90, 90, 100);
            GroupMove(90, 90, 90, 90, 90, 90, 500);
        }
    }
}
