using System;
using System.Collections.Generic;
using System.Linq;
using loreal_print.Models;
using loreal_print.Models.CirculationOutput;
using NLog;
using System.Web;
using loreal_print.MEC_Common.Security;

namespace loreal_print.Repository
{
    public class Circulation
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CirculationModel GetCirculation()
        {
            var lResult = new CirculationModel();
            var lLogMsg = string.Empty;
            var lCirculationOutput = new List<CirculationModel>();
            var newCirculationOutput = new CirculationModel();
            var newCirculationGroup = new CirculationGroup();
            var newCirculationContent = new CirculationContent();
            var lCircGroupTable = string.Empty;
            var pContext = System.Web.HttpContext.Current;
            var isCircGroupOpen = false;
            var isCircContentOpen = false;
            var recordCount = 0;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    var lCirculationRecords = db.GetCirculation(lCookieData.Year, lCookieData.BookID, lCookieData.VersionID)
                    .ToList();

                    var recordTotal = lCirculationRecords.Count;

                    newCirculationOutput.BookID = Convert.ToInt32(lCookieData.BookID);
                    newCirculationOutput.VersionID = lCookieData.VersionID;
                    newCirculationOutput.Year = lCookieData.Year;

                    foreach (var circItem in lCirculationRecords)
                    {
                        recordCount++;

                        if (lCircGroupTable != circItem.CirculationGroupType + " " + circItem.CirculationType)
                        {
                            lCircGroupTable = circItem.CirculationGroupType + " " + circItem.CirculationType;

                            if (isCircContentOpen)
                            {
                                newCirculationGroup.CirculationContents.Add(newCirculationContent);
                                isCircContentOpen = false;
                            }

                            if (isCircGroupOpen)
                            {
                                newCirculationOutput.CirculationGroups.Add(newCirculationGroup);
                                isCircGroupOpen = false;
                            }

                            newCirculationGroup = new CirculationGroup
                            {
                                CirculationGroupType = circItem.CirculationGroupType,
                                CirculationType = circItem.CirculationType,
                                CirculationTable = lCircGroupTable
                            };
                            isCircGroupOpen = true;
                        }

                        // Content
                        if (isCircContentOpen)
                        {
                            newCirculationGroup.CirculationContents.Add(newCirculationContent);
                            isCircContentOpen = false;
                        }

                        newCirculationContent = new CirculationContent
                        {
                            CirculationSubType = circItem.CirculationSubType,
                            CirculationSubTypeID = circItem.CirculationSubTypeID,
                        };

                        if (circItem.PrintCirculation != null)
                        {
                            newCirculationContent.PrintCirculation = Convert.ToInt32(circItem.PrintCirculation);
                        }
                        if (circItem.DigitalReplicaCirculation != null)
                        {
                            newCirculationContent.DigitalReplicaCirculation = Convert.ToInt32(circItem.DigitalReplicaCirculation);
                        }
                        if (circItem.DigitalNonReplicaCirculation != null)
                        {
                            newCirculationContent.DigitalNonReplicaCirculation = Convert.ToInt32(circItem.DigitalNonReplicaCirculation);
                        }
                        isCircContentOpen = true;

                        if (recordCount == recordTotal)
                        {
                            newCirculationGroup.CirculationContents.Add(newCirculationContent);
                            newCirculationOutput.CirculationGroups.Add(newCirculationGroup);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in Repository GetCirculation.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                throw new Exception(lReturnMessage);
            }

            if (newCirculationOutput.CirculationGroups.Any())
            {
                lResult = newCirculationOutput;
            }

            return lResult;
        }
    }
}