using System.Xml.XPath;
using USC.GISResearchLab.Common.Utils.Agents;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents
{
    /// <summary>
    /// Summary description for LAAssessorUtils.
    /// </summary>
    public class LAAssessorAgent
    {

        #region Properties
        private string _Layers;
        private string _AgentServerURL;


        public string AgentServerUrl
        {
            get { return _AgentServerURL; }
            set { _AgentServerURL = value; }
        }

        public string Layers
        {
            get { return _Layers; }
            set { _Layers = value; }
        }

        #endregion

        public LAAssessorAgent(string agentUrl)
        {
            Layers = "0,1,0,1,1,1,1,0,1,0,0,0,0,0,0,0";
            AgentServerUrl = agentUrl;
        }

        public XPathDocument RunGetAINAgent(string number, string preDirectional, string name, string suffix, string postDirectional, string city, string state, string zip)
        {

            string planPath = "/agent/runner?plan=laassessorassessorid%2Fplans%2Fproduction";
            string parameters = "";
            parameters += "&number=" + number;
            parameters += "&pre=" + preDirectional;
            parameters += "&name=" + name;
            parameters += "&suffix=" + suffix;
            parameters += "&post=" + postDirectional;
            parameters += "&city=" + city;
            parameters += "&state=" + state;
            parameters += "&zip=" + zip;
            parameters += "&combinedaddress=" + number + " " + name;

            string url = AgentServerUrl + planPath + parameters;
            return AgentUtils.query(url);
        }

        public XPathDocument RunValidateAINAgent(string ain)
        {

            string planPath = "/agent/runner?plan=laassessorainvalidator%2Fplans%2Fproduction";
            string parameters = "";
            parameters += "&ain=" + ain;
            string url = AgentServerUrl + planPath + parameters;
            return AgentUtils.query(url);
        }

        public XPathDocument RunGetImageAgent(string assessorID, int width, int height)
        {
            string planPath = "/agent/runner?plan=laassessorimages%2Fplans%2Fproduction";
            string parameters = "";
            parameters += "&width=" + width;
            parameters += "&height=" + height;
            parameters += "&layers=" + Layers;
            parameters += "&assessorid=" + assessorID;

            string url = AgentServerUrl + planPath + parameters;
            return AgentUtils.query(url);
        }

        public XPathDocument RunGetImageCorrectedAgent()
        {
            string planPath = "/agent/runner?plan=laassessorimages2%2Fplans%2Fproduction";
            string parameters = "";

            string url = AgentServerUrl + planPath + parameters;
            return AgentUtils.query(url);
        }

        public XPathDocument RunGetImageByExtentAgent(string assessorID, double x, double y, double xMax, double yMax, int width, int height)
        {
            string planPath = "/agent/runner?plan=laassessorimages3%2Fplans%2Fproduction";
            string parameters = "";
            parameters += "&mapX=" + x;
            parameters += "&mapY=" + y;
            parameters += "&mapXMax=" + xMax;
            parameters += "&mapYMax=" + yMax;
            parameters += "&width=" + width;
            parameters += "&height=" + height;
            parameters += "&layers=" + Layers;
            parameters += "&assessorId=" + assessorID;

            string url = AgentServerUrl + planPath + parameters;
            return AgentUtils.query(url);
        }

        public XPathDocument RunTokenBasedParserAgent(string streetAddress)
        {

            streetAddress = streetAddress.Replace("&", " and ");
            string planPath = "/agent/runner?plan=tokenbasedaddressparser%2Fplans%2Fproduction";
            string parameters = "&StreetAddress=" + streetAddress;
            string url = AgentServerUrl + planPath + parameters;

            return AgentUtils.query(url);
        }

    }
}
