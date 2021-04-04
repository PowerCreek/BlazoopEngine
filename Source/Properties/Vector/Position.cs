namespace Blazoop.Source.Properties.Vector
{
    public class Position : Vector
    {
        public Position() : this(0, 0)
        {
        }

        public Position(int x, int y) : base(x,y)
        {
            
        }

        public int X
        {
            get => values[0];
            set
            {
                if (values[0] != value)
                {
                    values[0] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Y
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