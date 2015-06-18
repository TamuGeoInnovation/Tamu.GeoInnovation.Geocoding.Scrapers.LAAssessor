using System;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Databases;
using USC.GISResearchLab.Common.Exceptions.Geocoding.Parameters;
using USC.GISResearchLab.Common.Geocoders.InterpolationAlgorithms.ParameterProviders;
using USC.GISResearchLab.Common.GeographicFeatures.Streets;
using USC.GISResearchLab.Common.GeographicObjects.Coordinates;
using USC.GISResearchLab.Common.Geographics.CoordinateSystems.Conversions;
using USC.GISResearchLab.Common.Geographics.CoordinateSystems.Geographic;
using USC.GISResearchLab.Common.Geographics.CoordinateSystems.StatePlanes;
using USC.GISResearchLab.Common.Geographics.Units;
using USC.GISResearchLab.Common.Geographics.Units.Linears;
using USC.GISResearchLab.Common.Geometries.Points;
using USC.GISResearchLab.Common.Geometries.Polygons;
using USC.GISResearchLab.Common.GeoreferencedImages;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Geocoding.Algorithms.FeatureInterpolationMethods.Interfaces;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Workers;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor
{
    public class LAAssessor : SQLServerDatabaseSource,
        IFeatureIdProvider, INumberOfLotsProvider, ILotNumberProvider, IAddressRangeProvider,
        IImageProvider, IAddressValidationProvider
    {

        #region Properties

        private bool _disposed;

        public bool IsRelaxable { get { return false; } }
        public bool IsFuzzyable { get { return false; } }
        public bool IsCacheable { get { return false; } }
        public bool IsSoundexable { get { return true; } }

        private Unit _CoordinateUnits;
        public Unit CoordinateUnits
        {
            get { return _CoordinateUnits; }
            set { _CoordinateUnits = value; }
        }

        private bool _ShouldQueryAgent;
        public bool ShouldQueryAgent
        {
            get { return _ShouldQueryAgent; }
            set { _ShouldQueryAgent = value; }
        }
	
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private LAAssessorAgent _LAAssessorAgent;
        public LAAssessorAgent LAAssessorAgent
        {
            get { return _LAAssessorAgent; }
            set { _LAAssessorAgent = value; }
        }
	
        private NumberOfLotsWorker _NumberOfLotsWorker;
        public NumberOfLotsWorker NumberOfLotsWorker
        {
            get { return _NumberOfLotsWorker; }
            set { _NumberOfLotsWorker = value; }
        }
	
        private ImageWorker _ImageWorker;
        public ImageWorker ImageWorker
        {
            get { return _ImageWorker; }
            set { _ImageWorker = value; }
        }
	
        private AINWorker _AINWorker;
        public AINWorker AINWorker
        {
            get { return _AINWorker; }
            set { _AINWorker = value; }
        }
	
        private AddressValidatorWorker _AddressValidatorWorker;
        public AddressValidatorWorker AddressValidatorWorker
        {
            get { return _AddressValidatorWorker; }
            set { _AddressValidatorWorker = value; }
        }

        private string _AgentURL;
        public string AgentURL
        {
            get { return _AgentURL; }
            set { _AgentURL = value; }
        }

        private LAAssessorCache _LAAssessorCache;
        public LAAssessorCache LAAssessorCache
        {
            get { return _LAAssessorCache; }
            set { _LAAssessorCache = value; }
        }
        #endregion


        public LAAssessor(string connectionString)
            : base(connectionString)
        {
            LAAssessorCache = new LAAssessorCache(connectionString);
            Name = "LAASSESSOR";
            CoordinateUnits = new DecimalDegrees();
        }

        public LAAssessor(string connectionString, string agentURL, bool shouldQueryAgent)
            : base(connectionString)
        {
            LAAssessorCache = new LAAssessorCache(connectionString);

            ShouldQueryAgent = shouldQueryAgent;
            AgentURL = agentURL;
            if (!StringUtils.IsEmpty(AgentURL))
            {
                LAAssessorAgent = new LAAssessorAgent(AgentURL);
            }
            else
            {
                ShouldQueryAgent = false;
            }
            AddressValidatorWorker = new AddressValidatorWorker(LAAssessorCache, LAAssessorAgent);
            ImageWorker = new ImageWorker(LAAssessorCache, LAAssessorAgent);
            AINWorker = new AINWorker(LAAssessorCache, LAAssessorAgent, AddressValidatorWorker);
            NumberOfLotsWorker = new NumberOfLotsWorker(LAAssessorCache, LAAssessorAgent, AddressValidatorWorker);
            Name = "LAASSESSOR";
            CoordinateUnits = new DecimalDegrees();
        }

        public LAAssessor(string dataSource, string catalog, string userName, string password, string agentURL, bool shouldQueryAgent)
            : base(dataSource, catalog, userName, password)
        {
            LAAssessorCache = new LAAssessorCache(dataSource, catalog, userName, password);

            ShouldQueryAgent = shouldQueryAgent;
            AgentURL = agentURL;
            if (!StringUtils.IsEmpty(AgentURL))
            {
                LAAssessorAgent = new LAAssessorAgent(AgentURL);
            }
            else
            {
                ShouldQueryAgent = false;
            }
            AddressValidatorWorker = new AddressValidatorWorker(LAAssessorCache, LAAssessorAgent);
            ImageWorker = new ImageWorker(LAAssessorCache, LAAssessorAgent);
            AINWorker = new AINWorker(LAAssessorCache, LAAssessorAgent, AddressValidatorWorker);
            NumberOfLotsWorker = new NumberOfLotsWorker(LAAssessorCache, LAAssessorAgent, AddressValidatorWorker);
            Name = "LAASSESSOR";
            CoordinateUnits = new DecimalDegrees();
        }

        public bool ValidateParameters(StreetAddress streetAddress, bool shouldThrowException)
        {
            bool ret = true;
            ValidateableStreetAddress address = ValidateableStreetAddress.FromStreetAddress(streetAddress);


            ret = address.HasValidNumber && address.HasName && (address.HasCity || address.HasZIP) && address.HasState;

            if (! ret)
            {
                if (shouldThrowException)
                {
                    throw new RequiredParameterException(Name + ": " + "address.HasValidNumber && address.HasName && (address.HasCity || address.HasZIP) && address.HasState are required");
                }
            }

            return ret;
        }

        #region IProvider Members

        public string GetFeatureId(string numberStr, string preDirectional, string name, string suffix, string postDirectional, string city, string state, string zipStr)
        {
            StreetAddress address = new StreetAddress(numberStr, preDirectional, name, suffix, postDirectional, city, state, zipStr);
            return GetFeatureId(address);
        }

        public string GetFeatureId(StreetAddress address)
        {
            string ret = null;
            ret = LAAssessorCache.AINCache.GetAIN(address);
            if (ret == null || ret == "")
            {
                if (ShouldQueryAgent)
                {
                    string[] ains = AINWorker.GetAIN(address);
                    if (ains != null)
                    {
                        if (ains.Length == 1)
                        {
                            ret = ains[0];
                        }
                        else
                        {
                            throw new Exception("Ambigous AIN match for address: " + address);
                        }
                    }
                }
            }
            return ret;
        }

       
        public bool ValidateAddress(StreetAddress address)
        {
            return LAAssessorCache.AddressCache.AddressExists(address);
        }

        //public MatchedFeature GetFeature(StreetAddress address, bool shouldUseRelaxation, bool shouldUseFuzzy)
        //{
        //    MatchedFeature ret = null;
        //    return ret;
        //}

        public Polygon GetGeometry(StreetAddress address, bool shouldUseRelaxation, bool shouldUseFuzzy)
        {
            Polygon ret = null;
            string AIN = GetFeatureId(address);
            if (AIN != null && AIN != "")
            {
                ret = LAAssessorCache.GeometryCache.GetGeometry(AIN);
                if (ret == null)
                {
                    if (ShouldQueryAgent)
                    {
                        AssessorImage image = (AssessorImage)ImageWorker.GetImage(AIN, 600, 600, .25);
                        Polygon polygon = Vectorizing.Vectorizer.Vectorize(image.URL);
                        if (polygon != null)
                        {
                            StatePlane0405 inputCS = new StatePlane0405();
                            GeographicCoordinateSystem outputCS = new GeographicCoordinateSystem();

                            ret = (Polygon)GeometryConverter.ConvertGeometry(polygon, inputCS, new Feet(), outputCS, new DecimalDegrees());
                            ret.Centroid = ((Point)GeometryConverter.ConvertGeometry(new Point(polygon.Centroid), inputCS, new Feet(), outputCS, new DecimalDegrees())).Coordinates;

                            if (ret.Centroid != null && ret.Centroid[0] != 0 && ret.Centroid[1] != 0)
                            {
                                LAAssessorCache.GeometryCache.CacheGeometry(
                                AIN, ret.Centroid[0], ret.Centroid[1],
                                ret.CoordinateString, ret.GeometryType);
                            }
                        }
                    }

                }
            }
            return ret;
        }


        public GeoreferencedImage GetImage(StreetAddress address, int width, int height, double resolution)
        {
            AssessorImage ret = null;
            string AIN = GetFeatureId(address);
            if (AIN != null && AIN != "")
            {
                ret = LAAssessorCache.ImageCache.GetImage(AIN);
                if (ret == null)
                {
                    if (ShouldQueryAgent)
                    {
                        ret = ImageWorker.GetImage(AIN, width, height, resolution);
                        LAAssessorCache.ImageCache.CacheImage(
                            AIN, ret.URL, ret.Image, ret.X, ret.Y, ret.XMax, ret.YMax,
                            ret.SRS, ret.Size, ret.Resolution
                            );
                    }
                }
            }
            return ret;
        }

        
        public int GetNumberOfLots(StreetAddress streetAddress, Street street)
        {
            int ret = -1;

            ret = LAAssessorCache.NumberOfLotsCache.GetNumberOfLots(streetAddress, street.Id, street.Source);
            ValidateableStreetAddress address = ValidateableStreetAddress.FromStreetAddress(streetAddress);
            if (ret <= 0)
            {
                ret = NumberOfLotsWorker.GetNumberOfLots(address , street);
            }
            //if (ret <= 0)
            //{
            //    AddressRange range = street.GetAddressRangeForAddress(address);
            //    ret = range.Addresses.Length;
            //}
            return ret;
        }

       
        public int GetLotNumber(StreetAddress address, int from, int to)
        {
            int ret = -1;

            AddressRange addresses = GetAddresses(address, from, to);
            if (addresses != null)
            {
                ret = addresses.getLotNumber(Convert.ToInt32(address.Number));
            }

            if (ret < 0)
            {
                addresses = new AddressRange(from, to);
                ret = addresses.getLotNumber(Convert.ToInt32(address.Number));
            }
            return ret;
        }

        public AddressRange GetAddresses(StreetAddress address, int from, int to)
        {
            return LAAssessorCache.AddressCache.GetAddressesOnSegment(address, from, to);
        }
        #endregion

    }
}
