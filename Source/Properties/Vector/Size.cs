namespace Blazoop.Source.Properties.Vector
{
    public class Size : Vector
    {
        public Size() : this(0, 0)
        {
        }

        public Size(int width, int height) : base(width, height)
        {
        }

        public int Width
        {
            get => values[0];
            set
            {
                if (values[0] == value) return;
                values[0] = value;
                OnPropertyChanged();
            }
        }

        public int Height
        {
            get => values[1];
            set
            {
                if (values[1] == value) return;
                values[1] = value;
                OnPropertyChanged();
            }
        }
    }
}