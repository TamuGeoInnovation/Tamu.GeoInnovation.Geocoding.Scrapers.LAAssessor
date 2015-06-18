namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor
{
	/// <summary>
	/// Summary description for AssessorAddressInfo.
	/// </summary>
	public class AssessorAddressInfo
    {
        #region Properties

        private string _Valid;
        private string _AssessorId;
        private string _AddressUsed;

        public string Valid
        {
            get{return _Valid;}
            set { _Valid = value; }
        }
		public string AssessorId
        {
            get { return _AssessorId; }
            set { _AssessorId = value; }
        }
		public string AddressUsed
        {
            get { return _AddressUsed; }
            set { _AddressUsed = value; }
        }
        #endregion

        public AssessorAddressInfo()
		{
			Valid = "false";
			AssessorId = "0";
			AddressUsed = "0";
		}
	}
}
