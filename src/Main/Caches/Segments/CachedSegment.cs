namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches
{
	/// <summary>
	/// Summary description for CachedSegment.
	/// </summary>
	public class CachedSegment
	{

		public double parcelsonblock;
		public int id;
		public int segmentid;
		public double parcelsOnEvenSide;
		public double parcelsOnOddSide;


		public CachedSegment()
		{
			parcelsonblock = -1.0;
			parcelsOnEvenSide = -1.0;
			parcelsOnOddSide = -1.0;
		}


	}
}
