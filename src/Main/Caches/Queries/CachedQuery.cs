namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
    /// <summary>
    /// Summary description for CachedParcel.
    /// </summary>
    public class CachedQuery
    {
        public int id;
        public int resultCount;

        public string number;
        //public string pre;  // pre and post have not effect in the assessor queries
        public string name;
        public string suffix;
        //public string post;


        public CachedQuery()
        {
        }
    }
}
