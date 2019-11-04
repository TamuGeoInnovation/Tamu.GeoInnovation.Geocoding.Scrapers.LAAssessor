using USC.GISResearchLab.Common.Geometries;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
    /// <summary>
    /// Summary description for CachedParcel.
    /// </summary>
    public class CachedGeometry
    {

        #region Properties
        private int _id;
        private double _centerLat;
        private double _centerLon;
        private string _coordinates;
        private GeometryType _GeometryType;

        public GeometryType GeometryType
        {
            get { return _GeometryType; }
            set { _GeometryType = value; }
        }

        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        public double centerLat
        {
            get { return _centerLat; }
            set { _centerLat = value; }
        }
        public double centerLon
        {
            get { return _centerLon; }
            set { _centerLon = value; }
        }
        public string coordinates
        {
            get { return _coordinates; }
            set { _coordinates = value; }
        }
        #endregion

    }
}
