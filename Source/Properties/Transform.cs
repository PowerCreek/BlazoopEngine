using System;
using System.Reflection;
using Blazoop.ExternalDeps.Classes.ElementProps;
using Blazoop.Source.Properties.Vector;

namespace Blazoop.Source.Properties
{
        public class Transform : ElementProperty<Transform>
        {
            private Position _position = new();

            private Size _size = new();

            
            public Transform()
            {
                Position = new();
                Size = new();
            }

            public Size Size
            {
                get => _size;
                set
                {
                    if (_size.Equals(value)) return;
                    _size = new Size(value.Width, value.Height);
                    _size.PropertyChanged += (a, b) => { OnResize?.Invoke(this, _size); };
                    OnResize?.Invoke(this, _size);
                }
            }

            public Action<Transform, Size> OnResize
            {
                get => GetPropertyActionCall<Size>().Invoke;
                set => GetPropertyActionCall<Size>().Action += value;
            }

            public Position Position
            {
                get => _position;
                set
                {
                    if (_position.Equals(value)) return;
                    _position = new Position(value.X, value.Y);
                    _position.PropertyChanged += (a, b) =>
                    {
                        OnMove?.Invoke(this, _position);
                    };
                    OnMove?.Invoke(this, _position);
                }
            }

            public Action<Transform, Position> OnMove
            {
                get => GetPropertyActionCall<Position>().Invoke;
                set => GetPropertyActionCall<Position>().Action = value;
            }

            public void SetPositionSize(int x, int y, int w, int h)
            {
                SetPositionSize(new Position(x, y), new Size(w, h));
            }

            public void SetPositionSize(Position position, Size size)
            {
                _position = position;
                Size = size;
            }
    }
}