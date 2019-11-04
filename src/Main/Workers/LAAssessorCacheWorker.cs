using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Workers
{
    public class LAAssessorCacheWorker
    {
        #region Properties
        private LAAssessorCache _LAAssessorCache;
        private LAAssessorAgent _LAAssessorAgent;

        public LAAssessorAgent LAAssessorAgent
        {
            get { return _LAAssessorAgent; }
            set { _LAAssessorAgent = value; }
        }

        public LAAssessorCache LAAssessorCache
        {
            get { return _LAAssessorCache; }
            set { _LAAssessorCache = value; }
        }

        #endregion

        public LAAssessorCacheWorker(LAAssessorCache laAssessorCache, LAAssessorAgent laAssessorAgent)
        {
            LAAssessorCache = laAssessorCache;
            LAAssessorAgent = laAssessorAgent;
        }
    }
}
