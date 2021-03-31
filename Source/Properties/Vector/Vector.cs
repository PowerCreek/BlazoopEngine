using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BlazorWebLib.Properties;

namespace Blazoop.Source.Properties.Vector
{
    public class Vector : INotifyPropertyChanged, IEquatable<int[]>
    {
        protected int[] values = {0, 0};

        public Vector()
        {
        }

        public Vector(int a, int b)
        {
            values[0] = a;
            values[1] = b;
        }

        public bool Equals(int[] other)
        {
            return other != null && values.SequenceEqual(other);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}