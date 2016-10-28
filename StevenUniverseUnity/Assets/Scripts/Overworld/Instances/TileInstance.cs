
using StevenUniverse.FanGame.StrategyMap;
using StevenUniverse.FanGame.Overworld.Templates;

namespace StevenUniverse.FanGame.Overworld.Instances
{
    [System.Serializable]
    public class TileInstance : Instance, ITile, System.IEquatable<TileInstance>, System.IComparable<TileInstance>
    {

        public TileInstance( Template template, IntVector2 position, int elevation ) : this( template, position.x, position.y, elevation )
        {

        }

        public TileInstance(Template template, int x, int y, int elevation) : base(template, x, y, elevation)
        {
        }

        public TileInstance(string templateAppDataPath, int x, int y, int elevation)
            : base(templateAppDataPath, x, y, elevation)
        {
        }

        public TileTemplate TileTemplate
        {
            get { return GetTemplate<TileTemplate>(); }
            set { SetTemplate(value); }
        }

        public int TemplateSortingOrder
        {
            get { return this.TileTemplate.TileLayer.GetSortingValue(); }
        }

        public int SortingOrder
        {
            get { return Elevation*100 + TileTemplate.TileLayer.GetSortingValue(); }
        }

        public TileTemplate.Mode TileMode
        {
            get { return TileTemplate.TileMode; }
        }

        public bool IsGrounded
        {
            get { return TileTemplate.IsGrounded; }
        }

        public bool Equals(TileInstance other)
        {
            //Two tiles are equal if they both have the same position and InstanceSortingOrder
            return (this.Position == other.Position && this.SortingOrder == other.SortingOrder);
        }

        public int CompareTo(TileInstance other)
        {
            //Return the comparison of the SortingOrder values
            return SortingOrder.CompareTo(other.SortingOrder);
        }

        public bool IsAbove(TileInstance other)
        {
            //Return the comparison of the SortingOrder values
            return CompareTo(other) > 0;
        }
    }
}