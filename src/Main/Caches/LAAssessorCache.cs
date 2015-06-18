using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.AINs;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.Images;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.NumberOfLots;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.Parcels;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches.Segments;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
    public class LAAssessorCache: SQLServerDatabaseSource
    {

        #region Properties
        private AddressCache _AddressCache;
        private AINCache _AINCache;
        private GeometryCache _GeometryCache;
        private ImageCache _ImageCache;
        private ParcelCache _ParcelCache;
        private QueryCache _QueryCache;
        private SegmentCache _SegmentCache;
        private NumberOfLotsCache _NumberOfLotsCache;

        public NumberOfLotsCache NumberOfLotsCache
        {
            get { return _NumberOfLotsCache; }
            set { _NumberOfLotsCache = value; }
        }

        public SegmentCache SegmentCache
        {
            get { return _SegmentCache; }
            set { _SegmentCache = value; }
        }
	
        public QueryCache QueryCache
        {
            get { return _QueryCache; }
            set { _QueryCache = value; }
        }
	
        public ParcelCache ParcelCache
        {
            get { return _ParcelCache; }
            set { _ParcelCache = value; }
        }
	


        public ImageCache ImageCache
        {
            get { return _ImageCache; }
            set { _ImageCache = value; }
        }

        public GeometryCache GeometryCache
        {
            get { return _GeometryCache; }
            set { _GeometryCache = value; }
        }
	
        public AINCache AINCache
        {
            get { return _AINCache; }
            set { _AINCache = value; }
        }

        public AddressCache AddressCache
        {
            get { return _AddressCache; }
            set { _AddressCache = value; }
        }
        #endregion

        public LAAssessorCache(string connectionString)
            : base(connectionString)
        {
            AddressCache = new AddressCache(connectionString, "la_assessor_cache_parcels");
            GeometryCache = new GeometryCache(connectionString, "la_assessor_cache_geometries");
            ImageCache = new ImageCache(connectionString, "la_assessor_cache_images");
            NumberOfLotsCache = new NumberOfLotsCache(connectionString, "la_assessor_cache_segments");
            ParcelCache = new ParcelCache(connectionString, "la_assessor_cache_parcels");
            QueryCache = new QueryCache(connectionString, "la_assessor_cache_queries");
            SegmentCache = new SegmentCache(connectionString, "la_assessor_cache_segments");
            AINCache = new AINCache(connectionString, "la_assessor_cache_parcels", AddressCache, ParcelCache);
        }

        public LAAssessorCache(string dataSource, string catalog, string userName, string password)
            : base(dataSource, catalog, userName, password)
        {
            AddressCache = new AddressCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_parcels");
            GeometryCache = new GeometryCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_geometries");
            ImageCache = new ImageCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_images");
            NumberOfLotsCache = new NumberOfLotsCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_segments");
            ParcelCache = new ParcelCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_parcels");
            QueryCache = new QueryCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_queries");
            SegmentCache = new SegmentCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_segments");
            AINCache = new AINCache(DataSource, Catalog, UserName, Password, "la_assessor_cache_parcels", AddressCache, ParcelCache);
        }
    }
}
