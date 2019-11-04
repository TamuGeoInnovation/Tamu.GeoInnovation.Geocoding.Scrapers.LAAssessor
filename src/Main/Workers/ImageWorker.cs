using System;
using System.Xml.XPath;
using USC.GISResearchLab.Common.Geographics.CoordinateSystems.Conversions;
using USC.GISResearchLab.Common.Geographics.CoordinateSystems.StatePlanes;
using USC.GISResearchLab.Common.Geographics.Units;
using USC.GISResearchLab.Common.Geographics.Units.Linears;
using USC.GISResearchLab.Common.Utils.Web.Images;
using USC.GISResearchLab.Common.Utils.XML;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Workers
{
    public class ImageWorker : LAAssessorCacheWorker
    {

        public ImageWorker(LAAssessorCache laAssessorCache, LAAssessorAgent laAssessorAgent)
            : base(laAssessorCache, laAssessorAgent) { }

        public AssessorImage GetImage(string assessorId, int width, int height, double resolution)
        {

            double resolutionFeetPerPixel = resolution * LengthConversionConstants.FEET_PER_METER;

            if (assessorId != null && !assessorId.Equals(""))
            {
                assessorId = assessorId.Trim();
                assessorId = assessorId.Replace("-", "");
            }
            else
            {
                assessorId = "";
            }

            /* Read the initial time. */
            DateTime startTime = DateTime.Now;

            AssessorImage assessorImageQueryResult = new AssessorImage();
            assessorImageQueryResult.AssessorId = assessorId;

            // query the LA Assessor with the address and street number
            XPathDocument xpathDocument = LAAssessorAgent.RunGetImageAgent(assessorId, width, height);

            // get the rows
            XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
            XPathExpression xpathExpression = xpathNavigator.Compile("//Row");
            XPathNodeIterator rows = xpathNavigator.Select(xpathExpression);

            // loop through each of the addresses returned by the LA Assessor page
            while (rows.MoveNext())
            {


                XPathNavigator node = rows.Current;

                assessorImageQueryResult.URL = XMLUtils.getGrandchildValue(node, "mapURL", "Value");
                assessorImageQueryResult.Image = ImagesUtils.GetImage(assessorImageQueryResult.URL);
                assessorImageQueryResult.X = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapX", "Value"));
                assessorImageQueryResult.XMax = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapXMax", "Value"));
                assessorImageQueryResult.Y = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapY", "Value"));
                assessorImageQueryResult.YMax = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapYMax", "Value"));
            }

            /* Read the end time. */
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;

            assessorImageQueryResult.TimeTaken = duration;

            return assessorImageQueryResult;
        }

        public AssessorImage GetImageForAddress(string assessorId, int size, double resolution)
        {
            return GetImageForAssessorId(assessorId, size, resolution);
        }

        public AssessorImage GetImageForAssessorId(string assessorId, int size, double resolution)
        {


            // get the first assessor image (scale dependent on the size of the parcel)
            AssessorImage assessorImageQueryResult1 = GetImage(assessorId, size, size, resolution);

            assessorImageQueryResult1.AssessorId = assessorId;
            //			string mapUrl = assessorImageQueryResult1.mapURL;
            double mapX = Convert.ToDouble(assessorImageQueryResult1.X);
            double mapXMax = Convert.ToDouble(assessorImageQueryResult1.XMax);
            double mapY = Convert.ToDouble(assessorImageQueryResult1.Y);
            double mapYMax = Convert.ToDouble(assessorImageQueryResult1.YMax);
            //			double mapXCenter = (mapXMax + mapX) / 2;
            //			double mapYCenter = (mapYMax + mapY) / 2;

            //			// get the center lat and lon of the image by calling the sp2dd conversion
            //			GeographyTools.GeographyToolsService geographyToolsService = new GeographyTools.GeographyToolsService();
            //			GeographyTools.GeoPointDecimalDegrees geoPointDecimalDegreesCenter = new GeographyTools.GeoPointDecimalDegrees();
            //			
            //
            //			geoPointDecimalDegreesCenter = geographyToolsService.ConvertLAAssessor2DD(mapYCenter, mapXCenter, agentServerUrl);
            //			double centerLon = geoPointDecimalDegreesCenter.lon.degrees * -1.0;
            //			double centerLat = geoPointDecimalDegreesCenter.lat.degrees;
            //
            //
            //			// get the bounding box for the image based on resolution
            //			mapXMax = (resolutionFeetPerPixel * (size / 2)) + mapXCenter;
            //			mapX = mapXCenter - (resolutionFeetPerPixel * (size / 2));
            //			mapY = (resolutionFeetPerPixel * (size / 2)) + mapYCenter;
            //			mapYMax = mapYCenter - (resolutionFeetPerPixel * (size / 2));
            //
            //			// get the 2nd assessor image centered around the parcel in the desired the resolution 
            //			AssessorImageQueryResult assessorImageQueryResult2 = GetAssessorImageByExtent(assessorId, mapX.ToString(), mapY.ToString(), mapXMax.ToString(), mapYMax.ToString(), size.ToString(), size.ToString(), LAYERS, agentServerUrl);
            return assessorImageQueryResult1;
        }

        public AssessorImage GetImageCorrected()
        {


            /* Read the initial time. */
            DateTime startTime = DateTime.Now;

            AssessorImage assessorImageQueryResult = new AssessorImage();

            // query the LA Assessor with the address and street number
            XPathDocument xpathDocument = LAAssessorAgent.RunGetImageCorrectedAgent();

            // get the rows
            XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
            XPathExpression xpathExpression = xpathNavigator.Compile("//Row");
            XPathNodeIterator rows = xpathNavigator.Select(xpathExpression);

            // loop through each of the addresses returned by the LA Assessor page
            while (rows.MoveNext())
            {


                XPathNavigator node = rows.Current;

                assessorImageQueryResult.URL = XMLUtils.getGrandchildValue(node, "mapURL", "Value");
                assessorImageQueryResult.X = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapX", "Value"));
                assessorImageQueryResult.XMax = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapXMax", "Value"));
                assessorImageQueryResult.Y = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapY", "Value"));
                assessorImageQueryResult.YMax = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapYMax", "Value"));

            }

            /* Read the end time. */
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;

            assessorImageQueryResult.TimeTaken = duration;

            return assessorImageQueryResult;
        }

        public AssessorImage GetImageTile(string assessorId, double bottomLeftLon, double bottomLeftLat, double topRightLon, double topRightLat, int width, int height)
        {
            double[] bottomLeftSP = CoordinateSystemConverter.DDToStatePlane(bottomLeftLon, bottomLeftLat, new StatePlane0405(), new Feet());
            double[] topRightSP = CoordinateSystemConverter.DDToStatePlane(topRightLon, topRightLat, new StatePlane0405(), new Feet());
            return GetImageByExtent(assessorId, bottomLeftSP[0], bottomLeftSP[1], topRightSP[0], topRightSP[1], width, height);
        }

        public AssessorImage GetImageByExtent(string assessorId, double x, double y, double xMax, double yMax, int width, int height)
        {


            /* Read the initial time. */
            DateTime startTime = DateTime.Now;

            AssessorImage assessorImageQueryResult = new AssessorImage();
            assessorImageQueryResult.AssessorId = assessorId;

            // query the LA Assessor with the address and street number
            XPathDocument xpathDocument = LAAssessorAgent.RunGetImageByExtentAgent(assessorId, x, y, xMax, yMax, width, height);

            // get the rows
            XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
            XPathExpression xpathExpression = xpathNavigator.Compile("//Row");
            XPathNodeIterator rows = xpathNavigator.Select(xpathExpression);

            // loop through each of the addresses returned by the LA Assessor page
            while (rows.MoveNext())
            {


                XPathNavigator node = rows.Current;

                assessorImageQueryResult.URL = XMLUtils.getGrandchildValue(node, "mapURL", "Value");
                assessorImageQueryResult.X = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapX", "Value"));
                assessorImageQueryResult.XMax = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapXMax", "Value"));
                assessorImageQueryResult.Y = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapY", "Value"));
                assessorImageQueryResult.YMax = Convert.ToDouble(XMLUtils.getGrandchildValue(node, "mapYMax", "Value"));

            }

            /* Read the end time. */
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;

            assessorImageQueryResult.TimeTaken = duration;

            return assessorImageQueryResult;
        }
    }
}
