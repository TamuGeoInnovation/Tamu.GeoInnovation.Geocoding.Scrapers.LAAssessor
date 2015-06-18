using System;
using USC.GISResearchLab.Common.GeoreferencedImages;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor
{
	/// <summary>
    /// Summary description for AssessorImageQueryResult.
	/// </summary>
	public class AssessorImage : GeoreferencedWebImage
    {

        #region Properties
        private TimeSpan _TimeTaken;
        private string _AssessorId;

        public string AssessorId
        {
            get { return _AssessorId; }
            set { _AssessorId = value; }
        }
		
        public TimeSpan TimeTaken
        {
            get { return _TimeTaken; }
            set{_TimeTaken = value;}
        }
        #endregion

        public AssessorImage()
		{
			SRS = "CASTATEPLANE0405";
		}
	}
}
