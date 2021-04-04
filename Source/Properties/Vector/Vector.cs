using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BlazorWebLib.Properties;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        public T Add<T>(T target) where T : Vector
        {
            var hold = Copy() as Vector;
            hold.values[0] += target.values[0];
            hold.values[1] += target.values[1];
            return hold as T;
        }
        
        public T Subtract<T>(Vector target) where T : Vector
        {
            var hold = Copy() as Vector;
            hold.values[0] -= target.values[0];
            hold.values[1] -= target.values[1];
            return hold as T;
        }
        
        public object Copy()
        {
            return Activator.CreateInstance(this.GetType(), values[0], values[1]);
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