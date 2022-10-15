using MbientLab.MetaWear.Core.SensorFusionBosch;
using MbientLab.MetaWear.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmrl_1_net_framework
{
    class FusionData
    {
        public Acceleration Acceleration { get; set; }
        public ImuCalibrationState ImuCalibrationState { get; set; }
        public Quaternion Quaternion { get; set; }

        public FusionData()
        {
            Acceleration = new Acceleration(0, 0, 0);
            ImuCalibrationState = new ImuCalibrationState();
            Quaternion = new Quaternion(0, 0, 0, 0);
        }
        public FusionData(Acceleration acc, Quaternion q)
        {
            Acceleration = acc;
            ImuCalibrationState = new ImuCalibrationState();
            Quaternion = q;
        }
    }
}
