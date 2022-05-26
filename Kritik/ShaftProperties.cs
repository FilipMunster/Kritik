using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kritik
{
    public class ShaftProperties : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BoundaryCondition bCLeft;
        public BoundaryCondition BCLeft
        {
            get => bCLeft;
            set
            {
                bCLeft = value;
                NotifyPropertyChanged();
            }
        }
        private BoundaryCondition bCRight;
        public BoundaryCondition BCRight
        {
            get => bCRight;
            set
            {
                bCRight = value;
                NotifyPropertyChanged();
            }
        }
        private GyroscopicEffect gyros;
        public GyroscopicEffect Gyros
        {
            get => gyros;
            set
            {
                gyros = value;
                NotifyPropertyChanged();
                if (value == GyroscopicEffect.none)
                    ShaftRotationInfluence = false;
            }
        }
        private bool shaftRotationInfluence;
        public bool ShaftRotationInfluence
        {
            get => shaftRotationInfluence;
            set
            {
                shaftRotationInfluence = value;
                NotifyPropertyChanged();
            }
        }
        private double shaftRPM;
        public double ShaftRPM
        {
            get => shaftRPM;
            set
            {
                shaftRPM = value;
                NotifyPropertyChanged();
            }
        }
        private double youngModulus;
        public double YoungModulus
        {
            get => youngModulus;
            set
            {
                youngModulus = value;
                NotifyPropertyChanged();
            }
        }
        private double materialDensity;
        public double MaterialDensity
        {
            get => materialDensity;
            set
            {
                materialDensity = value;
                NotifyPropertyChanged();
            }
        }
        private double operatingSpeed;
        public double OperatingSpeed
        {
            get => operatingSpeed;
            set
            {
                operatingSpeed = value;
                NotifyPropertyChanged();
            }
        }
        private double runawaySpeed;
        public double RunawaySpeed
        {
            get => runawaySpeed;
            set
            {
                runawaySpeed = value;
                NotifyPropertyChanged();
            }
        }
        private double maxCriticalSpeed;
        public double MaxCriticalSpeed
        {
            get => maxCriticalSpeed;
            set
            {
                maxCriticalSpeed = value;
                NotifyPropertyChanged();
            }
        }

        public ShaftProperties()
        {
            BCLeft = BoundaryCondition.free;
            BCRight = BoundaryCondition.free;
            Gyros = GyroscopicEffect.none;
            ShaftRotationInfluence = false;
            ShaftRPM = 0;
            YoungModulus = 210e9;
            MaterialDensity = 7850;
            OperatingSpeed = 0;
            RunawaySpeed = 0;
            MaxCriticalSpeed = 5000;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
