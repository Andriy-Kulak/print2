using HtmlTables;
using loreal_print.Models;
using loreal_print.Models.CirculationOutput;
using loreal_print.Models.EditorialCalendarOutput;
using loreal_print.Models.Rates;
using loreal_print.Models.RatesReport;
using loreal_print.Models.Tablet;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using WebSupergoo.ABCpdf10;
//using static System.Net.WebRequestMethods;

namespace loreal_print.MEC_Common.PDFOutput
{
    public class PDFService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string FilePath { get; set; }
        //public string FileName { get; set; }
        //public bool BuildPDF(PDFDocumentModel pPDFModel, HttpContext pContext)
        public byte[] BuildPDF(PDFDocumentModel pPDFModel, HttpContext pContext)
        {
            var lLogMsg = string.Empty;
            //var lResult = new byte[];
            var lFileUnique = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            var FileName = pPDFModel.Publisher + "-" + pPDFModel.Book + "-" + lFileUnique + ".pdf";
            var lFileLocation = "~/App_Data/" + pPDFModel.PublisherID + "/";
            var lLicense = "X/VKS0cNn5FhpydaGfTQKt+0efQWCtVwkfTQwuG8Xh9lkQnFfCW8KpFWQ0lkwg8KCtU34j9GuSIQr6MqQbd/xFItfG2eAn893TFMO/XgBjbi1y7S5MlUFrjUWBKMcmImUL1oUMFb8wtwCFVMoSiSIEERXiebQ2W5r8l4z1spFMbi699HgT3qvmOXlbkOuy9Mcx0q3kxq8Ve7AwR1JKYW/g==";
            // we are using license id for version 10
            var isInstalled = XSettings.InstallLicense(lLicense);

            // license id for version 9 (just in case:
            // X/VKS08wmMhAtn4jNv3DOcikae8bCdcYlqznwb23Xx9nkAjFfyeiNq1RDWxxjAcIC9Is/j5bpiRwyqooSLR1z1o6a2eYAng8tGZLYdf9U03h1h3G5o9adrPFLGKub2slRr1yVsBU/kd9BSZ2piGZKQR9ey2dSHivx84+6lFwY87kpdgeuzO38E7w+uBxoRlfSEArpH5Pk1SqRRRzOOwW
            // trial license below
            // XeJREBodo/8A5yFQb/yDbIv1eORKPdtyprjVztatBxc9lG+ed2DBUtgoEGtH/nQFBw== 

            Doc printPDF = new Doc();
            var lOutput = string.Empty;

            try
            {
                printPDF.HtmlOptions.UseScript = true;
                printPDF.Rendering.DotsPerInch = 300;
                printPDF.Rect.Inset(5, 5);
                //printPDF.Rect.Inset(72, 144);

                printPDF.FontSize = 6;
                printPDF.Font = printPDF.AddFont("Verdana");
                printPDF.TextStyle.CharSpacing = 0.2;
                printPDF.TextStyle.LineSpacing = 1.5;

                // Create the PDF Content
                lOutput = RenderTandC(printPDF, pPDFModel);
                lOutput += RenderRates(printPDF, pPDFModel);
                lOutput += RenderRatesReport(printPDF, pPDFModel);
                lOutput += RenderTablet(printPDF, pPDFModel);
                lOutput += RenderEditorialCalendar(printPDF, pPDFModel);
                lOutput += RenderCirculation(printPDF, pPDFModel);
                                                           
                var lResult = SaveStringToHTMLFile(lOutput, pPDFModel, "PDFOutput");
                HtmlToPdf(printPDF, lResult);

                // We're done with the html file - we can now delete it.
                RemoveHtmlFile(lResult);

                //var lResult = SaveStringToHTMLFile(theText, pPDFModel, "Circulation");
                //printPDF.AddPage();
                //HtmlToPdf(printPDF, lResult);


                var folderPath = HttpContext.Current.Server.MapPath(lFileLocation);
                FilePath = folderPath + FileName;

                //printPDF.Save(FilePath);
                //printPDF.Clear();
                //printPDF.Dispose();

                //lResult = true;
                //lResult = printPDF.GetData();
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in BuildPDF.  Publisher: {0}, Book: {1}, BookID: {2}.", pPDFModel.Publisher, pPDFModel.Book, pPDFModel.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);

            }
            return printPDF.GetData();
        }

        //private void RenderTable(Doc printPDF, PDFDocumentModel pPDFModel)
        //{
        //    WebSupergoo.ABCpdf10.
        //    PDFTable theTable = new PDFTable(printPDF, 5);
        //    theTable.CellPadding = 5;
        //    theTable.HorizontalAlignment = 1;
        //}

        private bool RenderURL(Doc printPDF, PDFDocumentModel pPDFModel, HttpContext pContext)
        {
            int lID = 0;
            var lResult = true;
            var lUrl = "http://loreal-print20160802110146.azurewebsites.net/print/circulation";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lUrl);
            request.CookieContainer = new CookieContainer();
            request.Credentials = CredentialCache.DefaultNetworkCredentials;

            using (WebResponse resp = request.GetResponse())
            {
                var authCookie = pContext.Request.Cookies["lorealPrint"];

                // cookieless Forms Authentication adds authentication ticket to the URL
                lUrl = resp.ResponseUri.AbsoluteUri;
                HttpWebResponse response = (HttpWebResponse)resp;
                if (response.Cookies.Count > 0)
                { // includes ASP.NET_SessionId
                    bool needsCookie2 = false;
                    StringBuilder builder = new StringBuilder("Cookie: ");

                    //for (int i = 0; i < response.Cookies.Count; ++i)
                    for (int i = 0; i < pContext.Request.Cookies.AllKeys.Length; ++i)
                    {
                        HttpCookie cookie = pContext.Request.Cookies[i];

                        //if (!needsCookie2 && cookie.Version != 1)
                        //    needsCookie2 = true;
                        if (i > 0)
                            builder.Append("; ");

                        builder.Append(cookie.Name + "=" + cookie.Value);
                    }
                    //{
                    //    Cookie cookie = response.Cookies[i];
                    //    if (!needsCookie2 && cookie.Version != 1)
                    //        needsCookie2 = true;
                    //    if (i > 0)
                    //        builder.Append("; ");
                    //    builder.Append(cookie.ToString());
                    //}
                    builder.Append(!needsCookie2 ? "\r\n" : "\r\nCookie2: $Version=1\r\n");
                    printPDF.HtmlOptions.HttpAdditionalHeaders = builder.ToString();
                }
            }
            printPDF.HtmlOptions.NoCookie = true;
            printPDF.HtmlOptions.PageLoadMethod = PageLoadMethodType.MonikerForHtml;

            lID = printPDF.AddImageUrl("http://loreal-print20160802110146.azurewebsites.net/print/circulation");

            while (true)
            {
                printPDF.FrameRect();
                if (!printPDF.Chainable(lID))
                    break;
                printPDF.Page = printPDF.AddPage();
                lID = printPDF.AddImageToChain(lID);
            }
            return lResult;
        }

        private void CreatePDFHeader(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            // Create the Top Header
            var lTitle = string.Format("Publisher: {0}\t Book: {1}\t Year: {2}", pPDFModel.Publisher, pPDFModel.Book, pPDFModel.Year);
            //printPDF.Rect.String = "0 732 612 792";
            printPDF.Rect.String = "0 732 612 792";
            printPDF.Color.String = "0 0 0";
            printPDF.FillRect();
            printPDF.Rect.String = "329 747 597 777";

            printPDF.Font = printPDF.EmbedFont("HelveticaNeueLT Std-Cn");
            printPDF.FontSize = 20;
            printPDF.Color.String = "255 255 255";
            printPDF.TextStyle.Indent = 25;
            printPDF.TextStyle.Bold = false;
            printPDF.TextStyle.VPos = 0.5;
            //printPDF.Rect.String = "15 747 306 777";
            printPDF.Rect.String = "15 747 500 777";
            printPDF.AddText(lTitle);
            printPDF.TextStyle.VPos = 0;
        }

        private string RenderTandC(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            // Get the Questions from the database.
            int lID = 0;
            var lLogMsg = string.Empty;

            var theText = "<table border='0'>" +
                            "<tbody>" +
                                "<tr><td><font size='6'><u>Publisher:</u> " + pPDFModel.Publisher + "</font></td></tr>" +
                                "<tr><td><font size='4'><u>Book:</u> " + pPDFModel.Book + "</font></td></tr>" +
                                "<tr><td><font size='4'><u>Year:</u> " + pPDFModel.Year + "</font><br></td></tr>" +
                                "<tr>" +
                                    "<td><font size='1'><u>Printed on " + DateTime.Now.ToString("MM/dd/yyyy") + "</u></font></td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td><font size='5'><br><u>Terms & Conditions</u></font></td>" +
                                "</tr>" +                          
                            "</tbody>" +
                        "</table>";
            // ***********************************************************************************************************************

            QuestionModel vm = new QuestionModel();

            try
            {
                vm.Get(HttpContext.Current);
                if (vm.SectionsListModel.SectionList.Any())
                {
                    foreach (var section in vm.SectionsListModel.SectionList)
                    {
                        //theText = theText + (
                        //    "<br /><h2><u>" + section.Name + "</u></h2>");
                        //theText += ("<table><tbody><tr><th>" + section.Name + "</th></tr></tbody></table>");
                        theText += "<table border='0'><tbody><tr><td><br><font size='4'><u>" + section.Name + "</u></font></td></tr></tbody></table>";

                        foreach (var subSection in section.SubSectionList)
                        {
                            //    theText += ("<br /><h3><u>" + subSection.Name + "</u></h3>");
                            theText += "<table border='0'><tbody><tr><td><br><font size='3'><u>" + subSection.Name + "</u></font></td></tr></tbody></table>" +
                                            "<table><tbody><tbody>";
                            foreach (var question in subSection.QuestionList)
                            {
                                //        theText += ("<br /><p>" + question.Name + "</p>");
                                theText += @"<tr><td border='0'>" + question.Name + "</td></tr>";
                                if (question.AnswerYesNo != null)
                                {
                                    theText += @"<tr>" +
                                                    "<td><u>Ans:</u> " + question.AnswerYesNo + "</td>" +
                                                "</tr>";
                                }
                                if (question.AnswerFreeForm != null)
                                    //theText += ("<p><b>" + question.AnswerFreeForm + "</b></p>");
                                    theText += @"<tr><td><u>Ans:</u> " + question.AnswerFreeForm + "</td></tr>";

                                foreach (var child in question.QuestionChildrenList)
                                {
                                    if (child.AnswerFreeForm != null || child.AnswerYesNo != null)
                                    {
                                        //theText += ("<br /><p>" + child.Name + "</p>");
                                        theText += @"<tr border='0'><td>" + child.Name + "</td></tr>";

                                        if (child.AnswerYesNo != null)
                                        {
                                            //theText += ("<p><b>" + child.AnswerYesNo + "</b></p>");
                                            theText += @"<tr><td><u>Ans:</u> " + child.AnswerYesNo + "</td></tr>";
                                        }

                                        if (child.AnswerFreeForm != null)
                                        {
                                            //theText += ("<p><b>" + child.AnswerFreeForm + "</b></p>");
                                            theText += @"<tr><td><u>Ans:</u> " + child.AnswerFreeForm + "</td></tr>";
                                        }
                                    }
                                }
                            }
                            theText += "</tbody></table>";
                        }
                        //theText += "</tbody></table>";
                    }
                    //printPDF.AddPage();

                    // Adding text
                    //lID = printPDF.AddHtml(theText);

                    //while (printPDF.Chainable(lID))
                    //{
                    //    printPDF.Page = printPDF.AddPage();
                    //    lID = printPDF.AddHtml("", lID);
                    //}
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService RenderTandC (Terms & Conditions) method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return theText;
        }

        //private void RenderRates(Doc printPDF, PDFDocumentModel pPDFModel)
        //{
        //    throw new NotImplementedException();
        //}

        private string RenderCirculation(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            int lID = 0;
            var lLogMsg = string.Empty;
            var newCirculationOutput = new CirculationModel();
            //var theText = "<br /><h3><u>1st Half 2016 Circulation</u></h3>";
            var theText = "<table border='0'>" +
                                "<tbody>" +
                                    "<tr>" +
                                        "<td><br><font size='5'><u>Circulation</u></font><br></td>" +
                                    "</tr>" +
                                "</tbody>" +
                            "</table>";

            try
            {
                var ReposCirc = new Repository.Circulation();
                newCirculationOutput = ReposCirc.GetCirculation();

                if (newCirculationOutput.CirculationGroups.Any())
                {
                    foreach (var table in newCirculationOutput.CirculationGroups)
                    {
                        theText += "<table border='0'><tbody><tr><td><br><font size='3'><u>" + table.CirculationTable + "</u></font></td></tr></tbody></table>";
                        theText += (
                        "<br /><table class='table table-striped table-condensed table-bordered table-hover'>" +
                            "<thead>" +
                                "<tr>" +
                                    "<th>Type</th>" +
                                    "<th>Print</th>" +
                                    "<th>Digital Replica</th>" +
                                    "<th>Digital Non-Replica</th>" +
                                "</tr>" +
                            "</thead>" +
                            "<tbody>"
                        );

                        foreach (var record in table.CirculationContents)
                        {
                            theText += (
                                    "<tr>" +
                                        "<td>" + record.CirculationSubType + "</td>" +
                                        "<td>" + record.PrintCirculation + "</td>" +
                                        "<td>" + record.DigitalReplicaCirculation + "</td>" +
                                        "<td>" + record.DigitalNonReplicaCirculation + "</td>" +
                                    "</tr>"
                                    );
                        }

                        theText += ("</tbody>" +
                            "</table>");
                    }
                }

            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService RenderCirculation method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return theText;
        }

        private string RenderEditorialCalendar(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            int lID = 0;
            var lLogMsg = string.Empty;
            var lEditorialCalendarOutput = new List<EditorialCalendarModel>();

            var theText = "<table border='0'>" +
                                "<tbody>" +
                                    "<tr>" +
                                        "<td><br><font size='5'><u>Editorial Calendar</u></font><br></td>" +
                                    "</tr>" +
                                "</tbody>" +
                            "</table>";

            try
            {
                var ReposEditCal = new Repository.EditorialCalendar();
                lEditorialCalendarOutput = ReposEditCal.GetCalendar();

                if (lEditorialCalendarOutput.Any())
                {
                    theText = theText + @"<table><thead><tr><th>Issue Date</th>
                        <th>Editorial Theme</th><th>On Sale Date</th><th>Mail to Subscribers Date</th>
                        <th>Space Closing Date - Scent Strips</th><th>Space Closing Date - Covers</th><th>Space Closing Date - ROB</th>
                        <th>Materials Closing Date - Scent Strips</th><th>Materials Closing Date - Covers</th><th>Materials Closing Date - ROB</th>
                        </tr></thead><tbody>";


                    foreach (var record in lEditorialCalendarOutput)
                    {
                        theText = theText + @"<tr><td>" + record.IssueDate + "</td><td>" + record.EditorialTheme + "</td><td>" + record.OnSaleDate +
                            "</td><td>" + record.MailToSubscribersDate + "</td><td>" + record.SpaceClosingDateScentStrips + "</td><td>" + record.SpaceClosingDateCovers +
                            "</td><td>" + record.SpaceClosingDateROB + "</td><td>" + record.MaterialsClosingDateScentStrips + "</td><td>" + record.MaterialsClosingDateCovers +
                            "</td><td>" + record.MaterialsClosingDateROB + "</td></tr>";
                    }
                    theText = theText + "</tbody></table>";
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService Circulation RenderEditorialCalendar method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return theText;
        }

        private string RenderRates(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            int lID = 0;
            var lLogMsg = string.Empty;
            var lRatesOutput = new RatesModel();

            var theText = "<table border='0'>" +
                "<tbody>" +
                    "<tr>" +
                        "<td><br><font size='5'><u>Rates</u></font></td>" +
                    "</tr>" +    
                "</tbody>" +
            "</table>";

            try
            {
                var ReposRates = new Repository.Rates();
                lRatesOutput = ReposRates.GetRates();

                if (lRatesOutput.RateParentAdTypes.Any())
                {

                    foreach (var parentAdType in lRatesOutput.RateParentAdTypes)
                    {
                        if (parentAdType.ParentAdTypeID == 1)
                        {
                            theText += @"<table border='0'><tbody><tr><td><font size='4'><u>Page Rates General</u></font></td></tr>" +
                            "<tr><td>***Please note, all dollar references are for Net US dollars.***</td></tr></tbody></table><br />" +

                        "<table>" +
                                    "<tr>" +
                                        "<td>2017 Guaranteed Rate Base(000)</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[1].RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>2017 NET P4C Open Rate</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[1].RateTiers[0].RateEditionTypes[0].Rate + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>2017 NET P4CB Open Rate</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[2].RateTiers[0].RateEditionTypes[0].Rate + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>Open Bleed Premium (in %)</td>" +
                                        "<td>" + parentAdType.BleedOpenPercentPremium + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>L'Oréal USA Bleed Premium (in %)</td>" +
                                        "<td>" + parentAdType.BleedEarnedPercentPremium + "</td>" +
                                    "</tr>" +
                                "</table>";

                            // General Rates - USA Discount Structure By Tier
                            theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>(General) L'Oréal USA Discount Structure By Tier</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th> Tier</td>" +
                                            "<th> Tier Range </th>" +
                                            "<th> Earned Net P4C Rate </th>" +
                                            "<th> Earned Net P4CB Rate </th>" +
                                            "<th> Earned Net P4C CPM </th>" +
                                            "<th> Earned Net P4CB CPM </th>" +
                                        "</tr>";
                            foreach (var RateTier in parentAdType.RateRateTypes[0].RateAdTypes[1].RateTiers)
                            {
                                if (RateTier.TierID != 0)
                                {
                                    theText += (
                                            "<tr>" +
                                                "<td>" + RateTier.Tier + "</td>" + // Tier
                                                "<td>" + RateTier.TierRange + "</td>" + // Tier Range
                                                "<td>" + RateTier.RateEditionTypes[0].Rate + "</td>" + // Earned Net P4C Rate
                                                "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[2].RateTiers[RateTier.TierID].RateEditionTypes[0].Rate + "</td>" + // Earned Net P4CB Rate 
                                                "<td>" + RateTier.RateEditionTypes[0].CPM + "</td>" + // Earned Net P4C CPM
                                                "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[2].RateTiers[RateTier.TierID].RateEditionTypes[0].CPM + "</td>" + // Earned Net P4CB CPM
                                            "</tr>"
                                        );
                                }
                            }

                            theText += (
                                "</table>"

                            );

                            // General Rates - Cover Premium Table

                            theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>Cover Premium Table</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th> Cover Type </th>" +
                                            "<th> Open Cover Premium </th>" +
                                            "<th> L'Oréal USA Cover Premium </th>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>Cover 2</td>" +
                                            "<td>" + parentAdType.Cover2OpenPercentPremium + "</td>" +
                                            "<td>" + parentAdType.Cover2EarnedPercentPremium + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>Cover 3</td>" +
                                            "<td>" + parentAdType.Cover3OpenPercentPremium + "</td>" +
                                            "<td>" + parentAdType.Cover3EarnedPercentPremium + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>Cover 4</td>" +
                                            "<td>" + parentAdType.Cover4OpenPercentPremium + "</td>" +
                                            "<td>" + parentAdType.Cover4EarnedPercentPremium + "</td>" +
                                        "</tr>" +
                                    "</table>";

                            // General Rates - Spread Discounts
                            //theText += (
                            //    "<br /><h4>Spread Discounts</h4>" +
                            theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>Spread Discounts</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<td>L'Oréal USA Spread Discount: ROB</td>" +
                                            "<td>" + parentAdType.SpreadROBEarnedPercentDiscount + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>L'Oréal USA Spread Discount: Cover 2/Page 1</td>" +
                                            "<td>" + parentAdType.SpreadC2P1EarnedPercentDiscount + "</td>" +
                                        "</tr>" +
                                    "</table>";

                            // General Rates - Fraction Premiums Table
                            //theText += (
                            //    "<br /><h4><u>Fraction Premiums Table</u></h4>" +
                            theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>Fraction Premiums Table</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th> Fractional Premium Type</th>" +
                                            "<th> Open Fractional Premium</th>" +
                                            "<th> L'Oréal USA Cover Premium</th>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>1/3 Page Standalone</td>" +
                                            "<td>" + parentAdType.FracThirdPageOpenPercentPremium + "</td>" +
                                            "<td>" + parentAdType.FracThirdPageEarnedPercentPremium + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>1/2 Page Standalone</td>" +
                                            "<td>" + parentAdType.FracHalfPageOpenPercentPremium + "</td>" +
                                            "<td>" + parentAdType.FracHalfPageEarnedPercentPremium + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>1/3 Page Running Opposite Full Brand Page</td>" +
                                            "<td>" + parentAdType.FracThirdRunOppFBPOpenPercentPremium + "</td>" +
                                            "<td>" + parentAdType.FracThirdRunOppFBPEarnedPercentPremium + "</td>" +
                                        "</tr>" +
                                    "</table>";

                            // General Rates - Fifth Color Premiums Table
                            //theText += (
                            //    "<br /><h4><u>Fifth Color Premiums Table</u></h4>" +
                            theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>Fifth Color Premiums Table</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th> Type</td>" +
                                            "<th> Open Fifth Color Premium (Net $)</th>" +
                                            "<th> L'Oréal USA Fifth Color Premium (Net $)</th>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>Metallic</td>" +
                                            "<td>" + parentAdType.FifthColorMetallicOpenDollarPremium + "</td>" +
                                            "<td>" + parentAdType.FifthColorMetallicEarnedDollarPremium + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>Match</td>" +
                                            "<td>" + parentAdType.FifthColorMatchOpenDollarPremium + "</td>" +
                                            "<td>" + parentAdType.FifthColorMatchEarnedDollarPremium + "</td>" +
                                        "</tr>" +
                                        "<tr>" +
                                            "<td>PMS</td>" +
                                            "<td>" + parentAdType.FifthColorPMSOpenDollarPremium + "</td>" +
                                            "<td>" + parentAdType.FifthColorPMSEarnedDollarPremium + "</td>" +
                                        "</tr>" +
                                    "</table>";

                            // Page Rates - Retail
                            // Only attempt to render this if Retail exists!
                            if (parentAdType.RateRateTypes.Count > 1)
                            {
                                theText += @"<table border='0'><tbody><tr><td><font size='4'><br><u>Page Rates Retail</u></font></td></tr>" +
                                            "<tr><td>***Please note, all dollar references are for Net US dollars.***</td></tr></tbody></table><br />" +
                                            "<table>" +
                                            "<tr>" +
                                                "<td>Net P4C Open Retail Rate:</td>" +
                                                "<td>" + parentAdType.RateRateTypes[1].RateAdTypes[1].RateTiers[0].RateEditionTypes[0].Rate + "</td>" +
                                            "</tr>" +
                                            "<tr>" +
                                                "<td>Net P4CB Open Retail Rate:</td>" +
                                                "<td>" + parentAdType.RateRateTypes[1].RateAdTypes[2].RateTiers[0].RateEditionTypes[0].Rate + "</td>" +
                                            "</tr>" +
                                            "<tr>" +
                                                "<td>Open Bleed Premium (in %):</td>" +
                                                "<td>" + parentAdType.BleedOpenPercentPremium + "</td>" +
                                            "</tr>" +
                                            "<tr>" +
                                                "<td>L'Oréal USA Bleed Premium (in %):</td>" +
                                                "<td>" + parentAdType.BleedEarnedPercentPremium + "</td>" +
                                            "</tr>" +
                                        "</table>";

                                // (Retail) L'Oréal USA Discount Structure By Tier
                                //theText += (
                                //    "<h4><u>(Retail) L'Oréal USA Discount Structure By Tier</u></h4>" +
                                theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>(Retail) L'Oréal USA Discount Structure By Tier</u></font></td></tr></tbody></table>" +
                                            "<table>" +
                                            "<tr>" +
                                                "<th> Tier</th>" +
                                                "<th> Tier Range</th>" +
                                                "<th> Earned Net P4C Rate</th>" +
                                                "<th> Earned Net P4CB Rate</th>" +
                                                "<th> Earned Net P4C CPM</th>" +
                                                "<th> Earned Net P4CB CPM</th>" +
                                            "</tr>";

                                foreach (var RateTier in parentAdType.RateRateTypes[1].RateAdTypes[1].RateTiers)
                                {
                                    if (RateTier.TierID != 0)
                                    {
                                        theText += (
                                                "<tr>" +
                                                    "<td>" + RateTier.Tier + "</td>" + // Tier
                                                    "<td>" + RateTier.TierRange + "</td>" + // Tier Range
                                                    "<td>" + RateTier.RateEditionTypes[0].Rate + "</td>" + // Earned Net P4C Rate
                                                    "<td>" + parentAdType.RateRateTypes[1].RateAdTypes[2].RateTiers[RateTier.TierID].RateEditionTypes[0].Rate + "</td>" + // Earned Net P4CB Rate 
                                                    "<td>" + RateTier.RateEditionTypes[0].CPM + "</td>" + // Earned Net P4C CPM
                                                    "<td>" + parentAdType.RateRateTypes[1].RateAdTypes[2].RateTiers[RateTier.TierID].RateEditionTypes[0].CPM + "</td>" + // Earned Net P4CB CPM
                                                "</tr>"
                                            );
                                    }
                                }

                                theText += (
                                    "</table>"
                                //"</div>"
                                );
                            }
                        }

                        if (parentAdType.ParentAdTypeID == 2)
                        {
                            // Scent Strips
                            //theText += (
                            //"<h3><u>Scent Strips / Inserts / BRC Cards</u></h3>" +
                            //"<p> ***Please note, all dollar references are for Net US dollars.*** </p>" +
                            theText += @"<table border='0'><tbody><tr><td><font size='4'><br><u>Scent Strips / Inserts / BRC Cards</u></font></td></tr>" +
                                            "<tr><td>***Please note, all dollar references are for Net US dollars.***</td></tr></tbody></table><br />" +
                            "<table>" +
                                    "<tr>" +
                                        "<td>Scent Strips / Inserts / BRC Cards Full Run <br /> Rate Base Circulation Guarantee (Not to Include Digital Replica) - (000):</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[0].RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>Scent Strips / Inserts / BRC Cards Subscription <br /> Only Rate Base Circulation Guarantee (Not to Include Digital Replica) - (000):</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[0].RateTiers[0].RateEditionTypes[1].RateBaseCirculationGuarantee + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>Scent Strips / Inserts / BRC Cards Full Run <br /> Average Print Run - (000):</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[0].RateTiers[0].RateEditionTypes[0].AveragePrintRun + "</td>" +
                                    "</tr>" +
                                    "<tr>" +
                                        "<td>Scent Strips / Inserts / BRC Cards Subscription Only <br /> Average Print Run  - (000):</td>" +
                                        "<td>" + parentAdType.RateRateTypes[0].RateAdTypes[0].RateTiers[0].RateEditionTypes[1].AveragePrintRun + "</td>" +
                                    "</tr>" +
                                "</table>";
                        }

                        if (parentAdType.ParentAdTypeID == 2 || parentAdType.ParentAdTypeID == 3 || parentAdType.ParentAdTypeID == 4)
                        {
                            // Beginning of Open Rates Charts - Scent Strip / Inserts / BRC Cards 
                            //theText += (
                            //    "<h4><u>US Only " + parentAdType.ParentAdType + " - Open Rates</u></h4>" +
                            theText +=@"<table border='0'><tbody><tr><td><br><font size='3'><u>US Only " + parentAdType.ParentAdType + " - Open Rates</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th>" + parentAdType.ParentAdType + " Ad Type</td>" +
                                            "<th> <b>Full Run</b> <br /> NET Open Rate</td>" +
                                            "<th> <b>Full Run</b> <br /> NET Open Production Cost</td>" +
                                            "<th> <b>Subscription Only</b> <br /> NET Open Rate</td>" +
                                            "<th> <b>Subscription Only/b> <br /> NET Open Production Cost</td>" +
                                        "</tr>";

                            foreach (var RateAdType in parentAdType.RateRateTypes[0].RateAdTypes)
                            {
                                theText += (
                                        "<tr>" +
                                            "<td>" + RateAdType.AdType + "</td>" + // Ad Type
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[0].Rate + "</td>" + // Full Run Open Prod Rate
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[0].OpenProductionCost + "</td>" + // Full Run Open Prod Cost
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[1].Rate + "</td>" + // Sub Only Open Rate
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[1].OpenProductionCost + "</td>" + // Sub Only Open Cost
                                        "</tr>"
                                );
                            }

                            theText += (
                                "</table>"
                            //"</div>"
                            );
                            // End of Open Rates Charts - Scent Strip / Inserts / BRC Cards 

                            // Beginning of Rate Structure - Scent Strip / Inserts / BRC Cards 
                            //theText += (
                            //    "<h4><u>L'Oréal USA " + parentAdType.ParentAdType + " Rate Structure</u></h4>" +
                            theText += @"<table border='0'><tbody><tr><td><br><font size='3'><u>L'Oréal USA " + parentAdType.ParentAdType + " Rate Structure</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th>" + parentAdType.ParentAdType + " Ad Type</th>" +
                                            "<th> Tier</th>" +
                                            "<th> Tier Range</th>" +
                                            "<th> US Only Full Run NET <br/ > Earned Rate</th>" +
                                            "<th> US Only Subscription Only <br/ > NET Earned Rate</th>" +
                                            "<th> US Only Full Run NET <br/ > Earned CPM <br/ > (without Production)</th>" +
                                            "<th> US Only Subscription <br/ > Only NET Earned CPM <br/ > (without Production)</th>" +
                                        "</tr>";

                            foreach (var RateAdType in parentAdType.RateRateTypes[0].RateAdTypes)
                            {
                                foreach (var RateTier in RateAdType.RateTiers)
                                    if (RateTier.TierID != 0)
                                    {
                                        theText += (
                                                "<tr>" +
                                                    "<td>" + RateAdType.AdType + "</td>" + // Ad Type
                                                    "<td>" + RateTier.Tier + "</td>" + // Tier
                                                    "<td>" + RateTier.TierRange + "</td>" + // Tier Range
                                                    "<td>" + RateTier.RateEditionTypes[0].Rate + "</td>" + // Full Run NET Earned Rate
                                                    "<td>" + RateTier.RateEditionTypes[1].Rate + "</td>" + // Sub Only NET Earned Rate
                                                    "<td>" + RateTier.RateEditionTypes[0].CPM + "</td>" + // Full Run NET Earned CPM
                                                    "<td>" + RateTier.RateEditionTypes[1].CPM + "</td>" + // Sub Only NET Earned CPM
                                                "</tr>"
                                        );
                                    }
                            }

                            theText += (
                                "</table>"
                            //"</div>"
                            );
                            // End of Rate Structure - Scent Strip / Inserts / BRC Cards 

                            // Beginning of Prod Costs and NEP Values for - Scent Strip / Inserts / BRC Cards 
                            //theText += (
                            //    "<h4><u>US Only L'Oréal USA Production Costs and NEP Values for " + parentAdType.ParentAdType + "</u></h4>" +
                            theText +=@"<table border='0'><tbody><tr><td><br><font size='3'><u>US Only L'Oréal USA Production Costs and NEP Values for " + parentAdType.ParentAdType + "</u></font></td></tr></tbody></table>" +
                                        "<table>" +
                                        "<tr>" +
                                            "<th>" + parentAdType.ParentAdType + " Ad Type</th>" +
                                            "<th> <b>Full Run</b> <br /> NET L'Oréal USA Earned <br/ > Production Cost</th>" +
                                            "<th> <b>Full Run</b> <br /> L'Oréal USA NEP Value</th>" +
                                            "<th> <b>Subscription Only</b> <br /> NET L'Oréal USA Earned <br/ > Production Cost</th>" +
                                            "<th> <b>Subscription Only</b> <br /> L'Oréal USA NEP Value</th>" +
                                        "</tr>";

                            foreach (var RateAdType in parentAdType.RateRateTypes[0].RateAdTypes)
                            {
                                theText += (
                                        "<tr>" +
                                            "<td>" + RateAdType.AdType + "</td>" + // Ad Type
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[0].EarnedProductionCost + "</td>" + // Full Run NET L'Oréal USA Earned Production Cost 
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[0].EarnedNEP + "</td>" + // Full Run L'Oréal USA NEP Valu
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[1].EarnedProductionCost + "</td>" + // Sub Only NET L'Oréal USA Earned Production Cost
                                            "<td>" + RateAdType.RateTiers[0].RateEditionTypes[1].EarnedNEP + "</td>" + // Sub Only L'Oréal USA NEP Valu
                                        "</tr>"
                                );
                            }

                            theText += (
                                "</table>"
                            //"</div>"
                            );
                            // End of Prod Costs and NEP Values for - Scent Strip / Inserts / BRC Cards
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService RenderRates method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return theText;
        }


        private string RenderRatesReport(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            int lID = 0;
            var lLogMsg = string.Empty;
            var lRatesReportOutput = new RatesReportModel();
            var lPubAgreement = string.Empty;
            //var theText = "<h1><u>Rates Report</u></h1>";

            var theText = "<table border='0'>" +
                                "<tbody>" +
                                    "<tr>" +
                                        "<td><br><font size='5'><u>Rates Report</u></font><br></td>" +
                                    "</tr>" +
                                "</tbody>" +
                            "</table>";

            try
            {
                var ReposRatesReport = new Repository.RatesReport();
                lRatesReportOutput = ReposRatesReport.GetRatesReport();

                if (lRatesReportOutput.RateTypes.Any())
                {
                    foreach (var record in lRatesReportOutput.RateTypes)
                    {
                        //theText = theText + "<h4>" + record.RateType + "</h4>";
                        theText +="<table border='0'><tbody><tr><td><br><font size='3'><u>" + record.RateType + "</u></font></td></tr></tbody></table>";

                        theText += "<table><thead><tr><th>Publisher Agreement</th><th>Parent Ad Type</th><th>Edition Type</th><th>Unit Type NET</th>";

                        foreach (var tier in record.AdTypes[0].Tiers)
                        {
                            theText += "<th>" + tier.TierRange + "</th>";
                        }
                        theText += "</tr></thead><tbody>";

                        foreach (var adType in record.AdTypes)
                        {
                            if (String.IsNullOrEmpty(adType.PublisherAgreement))
                            {
                                lPubAgreement = "--";
                            }
                            else
                            {
                                lPubAgreement = adType.PublisherAgreement;
                            }

                            theText += @"<tr><td>" + lPubAgreement + "</td>" +
                                "<td>" + adType.ParentAdType + "</td>" +
                                "<td>" + adType.EditionType + "</td>" +
                                "<td>" + adType.AdType + "</td>";

                            foreach (var tier in adType.Tiers)
                            {
                                theText += "<td>" + tier.Rate + "</td>";
                            }
                            theText += "</tr>";
                            //}
                        }
                        theText += "</tbody></table>";
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService RenderRatesReport method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return theText;
        }

        private string RenderTablet(Doc printPDF, PDFDocumentModel pPDFModel)
        {
            int lID = 0;
            var lLogMsg = string.Empty;
            var lTableRatesOutput = new List<TabletModel>();
            //var theText = "<h1><u>Tablet</u></h1>";
            var theText = "<table border='0'>" +
                                "<tbody>" +
                                    "<tr>" +
                                        "<td><br><font size='5'><u>Tablet</u></font><br></td>" +
                                    "</tr>" +
                                "</tbody>" +
                            "</table>";

            try
            {
                var ReposTablet = new Repository.Tablet();
                lTableRatesOutput = ReposTablet.GetTabletRates();

                if (lTableRatesOutput.Any())
                {
                    var lRow = 0;
                    foreach (var record in lTableRatesOutput)
                    {
                        if (lRow == 0)
                        {
                            //theText += @"<h2>" + record.TabletParentFunctionality + "</h2>" +
                            //    "<label>If your magazine charges URL link fees for SFP ads, please indicate the link fee in NET $ in the following fields.  If your magazine does not charge URL link Net fees for SFP ads, please reflect $0 in these fields.</label>" +
                            //    "<label>We ask that, if you typically charge URL link fees, that you agree to waive these fees for L'Oréal USA.  If you agree, please make sure that the Net L'Oréal USA Link Fee field is reflective of this.</label>";

                            theText += "<table border='0'><tbody><tr><td><br><font size='3'><u>" + record.TabletParentFunctionality + "</u></font></td></tr></tbody></table>";
                            theText += @"<table>
                                            <tbody>
                                                <tr><td>If your magazine charges URL link fees for SFP ads, please indicate the link fee in NET $ in the following fields.  If your magazine does not charge URL link Net fees for SFP ads, please reflect $0 in these fields.</td></tr>
                                                <tr><td>We ask that, if you typically charge URL link fees, that you agree to waive these fees for L'Oréal USA.  If you agree, please make sure that the Net L'Oréal USA Link Fee field is reflective of this.</td></tr>
                                            </tbody>
                                        </table>";

                            theText += @"<table>
                                            <thead>
                                                <tr>
                                                    <th>Functionality Type </th>
                                                    <th>Net Open Link Fee</th>
                                                    <th>NET L'Oréal USA Link Fee</th>
                                                </tr>
                                            </thead>";

                            theText += @"<tbody>
                                            <tr>
                                                <td>" + record.TabletFunctionality + "</td>" + /// TODO: ng-change="vm.getTabletAnswers(vm.records[0])"
                                                "<td>" + record.OpenRate + "</td>" +
                                                "<td>" + record.EarnedRate + "</td>" +
                                            "</tr>" +
                                        "</tbody>" + 
                                    "</table>";
                        }
                        else if (lRow == 1)
                        {
                            //theText = theText + @"<h2>" + record.TabletParentFunctionality + "</h2>" +
                            //    "<label>If your magazine charges creative fees for DFT ads, please indicate the creative fee in NET $ in the following fields.  If your magazine does not charge creative fees for DFT ads, please reflect $0 in these fields.</label>" +
                            //    "<label>We ask that, if you typically charge DFT creative fees, that you agree to waive these fees for L'Oréal USA.  If you agree, please make sure that the Net L'Oréal USA Creative Fee field is reflective of this.</label>";
                            theText += "<table border='0'><tbody><tr><td><br><font size='3'><u>" + record.TabletParentFunctionality + "</u></font></td></tr></tbody></table>";
                            theText += @"<table>
                                            <tbody>
                                               <tr><td>If your magazine charges creative fees for DFT ads, please indicate the creative fee in NET $ in the following fields.  If your magazine does not charge creative fees for DFT ads, please reflect $0 in these fields.</td></tr>
                                               <tr><td>We ask that, if you typically charge DFT creative fees, that you agree to waive these fees for L'Oréal USA.  If you agree, please make sure that the Net L'Oréal USA Creative Fee field is reflective of this.</td></tr>
                                            </tbody>
                                        </table>";

                            theText += @"<table>
                                            <thead>
                                                <tr>
                                                    <th>Functionality Type</th>
                                                    <th>Net Open Creative Fee ($)</th>
                                                    <th>NET L'Oréal USA Creative Fee ($)</th>
                                                </tr>
                                            </thead>";

                            theText += @"<tbody>
                                            <tr>
                                                <td>" + record.TabletFunctionality + "</td>" + /// TODO: ng-change="vm.getTabletAnswers(vm.records[0])"
                                                "<td>" + record.OpenRate + "</td>" + 
                                                "<td>" + record.EarnedRate + "</td>" +
                                            "</tr>" +
                                        "</tbody>" +
                                    "</table>";
                        }
                        else if (lRow == 2)
                        {
                            //theText = theText + @"<h2>" + record.TabletParentFunctionality + "</h2>" +
                            //    "<label>If your magazine charges creative fees for EFT elements, please indicate the creative fee in NET $ in the following fields by element.  If your magazine does not charge EFT creative fees, please reflect $0 in these fields.</label>" +
                            //    "<label>We ask that, if you typically charge EFT creative fees, that you agree to waive these fees for L'Oréal USA.  If you agree, please make sure that the Net L'Oréal USA Fee fields are reflective of this.</label>";
                            theText += "<table border='0'><tbody><tr><td><br><font size='3'><u>" + record.TabletParentFunctionality + "</u></font></td></tr></tbody></table>";
                            theText += @"<table>
                                            <tbody>
                                               <tr><td>If your magazine charges creative fees for EFT elements, please indicate the creative fee in NET $ in the following fields by element.  If your magazine does not charge EFT creative fees, please reflect $0 in these fields.</td></tr>
                                               <tr><td>We ask that, if you typically charge EFT creative fees, that you agree to waive these fees for L'Oréal USA.  If you agree, please make sure that the Net L'Oréal USA Fee fields are reflective of this.</td></tr>
                                            </tbody>
                                        </table>";

                            theText += @"<table>
                                            <thead>
                                                <tr>
                                                    <th>Functionality Type</th>
                                                    <th>Net Open Fee ($)</th>
                                                    <th>NET L'Oréal USA Fee</th>
                                                </tr>
                                            </thead>";

                            theText += @"<tbody>
                                            <tr>
                                                <td>" + record.TabletFunctionality + "</td>" + /// TODO: ng-change="vm.getTabletAnswers(vm.records[0])"
                                                "<td>" + record.OpenRate + "</td>" +
                                                "<td>" + record.EarnedRate + "</td>" +
                                            "</tr>";
                        }
                        else if (lRow > 2 && record.TabletParentFunctionality == "Enhanced for Tablet")
                        {
                            theText += @"<tr><td>" + record.TabletFunctionality + "</td>" + /// TODO: ng-change="vm.getTabletAnswers(vm.records[0])"
                            "<td>" + record.OpenRate + "</td>" + "<td>" + record.EarnedRate + "</td></tr>";
                        }
                        lRow++;
                    }
                    theText += "</tbody></table>";
                }
                //var lResult = SaveStringToHTMLFile(theText, pPDFModel, "Tablet");
                //printPDF.AddPage();
                //HtmlToPdf(printPDF, lResult);
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService Tablet RenderTablet method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return theText;
        }



        #region Common Methods

        private void RemoveHtmlFile(string pFilePath)
        {
            var lLength = pFilePath.LastIndexOf("\\") + 1;
            var lFolderPath = pFilePath.Substring(0, lLength);
            var lFileName = pFilePath.Substring(lLength);
            var lLogMsg = string.Empty;

            try
            {                
                if (File.Exists(lFolderPath + lFileName))
                {
                    File.Delete(lFolderPath + lFileName);
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService RemoveHtmlFile method. File path: {0}. File name: {1}", lFolderPath, lFileName);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
        }
        private string SaveStringToHTMLFile(string pHTMLText, PDFDocumentModel pPDFModel, string Type)
        {
            var lLogMsg = string.Empty;
            var lResult = string.Empty;
            var lFileUnique = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            var FileName = pPDFModel.Publisher + "-" + pPDFModel.Book + "-" + Type + "- " + lFileUnique + ".html";
            var lFileLocation = "~/App_Data/" + pPDFModel.PublisherID + "/";

            var folderPath = HttpContext.Current.Server.MapPath(lFileLocation);
            FilePath = folderPath + FileName;

            try
            {
                if (pHTMLText.Length > 0)
                {
                    //var buffer = GetBufferFromString(pHTMLText);
                    var file = File.CreateText(FilePath);
                    file.Write(pHTMLText);
                    file.Flush();
                    file.Dispose();
                    lResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService Tablet RenderTablet method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            return lResult;
        }

        //private void HtmlToPdf(string inPath, string outPath)
        private void HtmlToPdf(Doc printPDF, string inPath)
        {
            var lLogMsg = string.Empty;
            int maxImageWidth = 400;
            int maxImageHeight = 300;
            bool addBackground = false;
            bool outline = false;
            bool addFrame = false;
            bool deleteContent = false;
            bool deleteShift = false;


            if (!File.Exists(inPath))
                return;

            //Doc doc = new Doc();
            //printPDF.Rect.Inset(100, 100);
            printPDF.Rect.Inset(10, 10);

            int page = 1;
            HtmlDoc h = new HtmlDoc(printPDF);
            h.MaxImageSize = new Size(maxImageWidth, maxImageHeight);
            h.SetFile(inPath);

            try
            {
                while (!h.Drawn)
                {
                    while (page > printPDF.PageNumber)
                        printPDF.Page = printPDF.AddPage();
                    if (addBackground)
                    {
                        string save = printPDF.Rect.String;
                        printPDF.Rect.String = printPDF.MediaBox.String;
                        printPDF.Color.String = "255 128 128";
                        printPDF.FillRect();
                        printPDF.Color.String = "0 0 0";
                        printPDF.Rect.String = save;
                    }
                    XRect xr = h.Draw();
                    if (outline)
                        DrawContentOutlines(printPDF, h, xr);
                    if (addFrame)
                    {
                        string save = printPDF.Color.String;
                        printPDF.Width = 0.25;
                        printPDF.Color.String = "0 255 255"; // cyan
                        printPDF.FrameRect();
                        printPDF.Color.String = save;
                    }
                    if (deleteContent)
                    {
                        h.Delete();
                        break;
                    }
                    if (deleteShift)
                    {
                        h.Delete();
                        printPDF.Rect.Move(0, -200);
                        h.Draw();
                    }
                    if (printPDF.PageCount > 200)
                        break;
                    page++;
                }

                for (int i = 1; i <= printPDF.PageCount; i++)
                {
                    printPDF.PageNumber = i;
                    printPDF.Flatten();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in PDFService HtmlToPdf method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            //doc.Save(outPath);
            //System.Diagnostics.Process.Start(outPath);
        }

        void DrawContentOutlines(Doc doc, HtmlDoc html, XRect totalArea)
        {
            string saveRect = doc.Rect.String;
            doc.Color.String = "0 0 255"; // blue
            doc.Width = 1.0;
            // frame individual items of content
            List<XRect> rects = html.GetContentArea(true);
            foreach (XRect xr in rects)
            {
                doc.Rect.String = xr.String;
                doc.FrameRect();
            }
            // frame overall content area
            rects = html.GetContentArea(false);
            doc.Width = 0.75;
            doc.Color.String = "0 255 0"; // green
            doc.Rect.String = DeriveTotalContentArea(rects).String;
            doc.FrameRect();
            // frame overall table area
            doc.Width = 0.50;
            doc.Color.String = "255 0 0"; // red
            doc.Rect.String = totalArea.String;
            doc.FrameRect();
            doc.Rect.String = saveRect;
        }

        public XRect DeriveTotalContentArea(List<XRect> rects)
        {
            if ((rects == null) || (rects.Count == 0))
                return null;
            double l = Double.MaxValue, r = Double.MinValue;
            double t = Double.MinValue, b = Double.MaxValue;
            foreach (XRect xr in rects)
            {
                l = Math.Min(l, xr.Left);
                r = Math.Max(r, xr.Right);
                t = Math.Max(t, xr.Top);
                b = Math.Min(b, xr.Bottom);
            }
            XRect xr2 = new XRect();
            xr2.SetRect(l, b, r - l, t - b);
            return xr2;
        }

        #endregion
    }
}