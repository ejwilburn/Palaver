/*
Copyright 2012, Marcus McKinnon, E.J. Wilburn, Kevin Williams
This program is distributed under the terms of the GNU General Public License.

This file is part of Palaver.

Palaver is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

Palaver is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Palaver.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Palaver2.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using CodeFirstMembership.Models;
using System.IO;
using System.Web.UI;

namespace Palaver2.Helpers
{
    public class CustomHtmlHelpers
    {
        public static string Linkify(string input)
        {
            String output = input;
            // Convert URLs in the text to links if they're not already a link.
            // First linkify URLs with the protocol already there.
            Regex regex = new Regex(@"(?<!(?:href=[""']?|src=['""]?|<a[^>]*>)[^.'""]*[\s]*)\b(?:(?:https?|ftps?|ftpes|file)://)[-A-Z0-9+&;@#/%=~_|$?!:,.]*[A-Z0-9+&;@#/%=~_|$]", RegexOptions.IgnoreCase);
            int beginningSize = output.Length;
            MatchCollection matches = regex.Matches(output);
            foreach (Match match in matches)
            {
                int sizeDif = output.Length - beginningSize;
                String cleanedMatchValue = HttpUtility.HtmlDecode(match.Groups[0].Value).Trim();
                output = output.Substring(0, match.Groups[0].Index + sizeDif) +
                    "<a href=\"" + cleanedMatchValue + "\" target=\"_blank\">" + cleanedMatchValue + "</a>" +
                    output.Substring(match.Groups[0].Index + match.Groups[0].Length + sizeDif);
            }

            // Second, linkify URLs without the protocol specified, assume http.
            regex = new Regex(@"(?<!(?:http://|ftp://|href=[""']?|src=[""']?|<a[^>]*>)[^.'""]*[\s]*)\b(?:www\.|ftp\.)[-A-Z0-9+&;@#/%=~_|$?!:,.]*[A-Z0-9+&;@#/%=~_|$]", RegexOptions.IgnoreCase);
            beginningSize = output.Length;
            matches = regex.Matches(output);
            foreach (Match match in matches)
            {
                int sizeDif = output.Length - beginningSize;
                String cleanedMatchValue = HttpUtility.HtmlDecode(match.Groups[0].Value).Trim();
                output = output.Substring(0, match.Groups[0].Index + sizeDif) +
                    "<a href=\"http://" + cleanedMatchValue + "\" target=\"_blank\">" + cleanedMatchValue + "</a>" +
                    output.Substring(match.Groups[0].Index + match.Groups[0].Length + sizeDif);
            }
            return output;
        }

		public static string RenderPartialToString(string controlName, object viewData)
		{
			ViewPage viewPage = new ViewPage() { ViewContext = new ViewContext() };

			viewPage.ViewData = new ViewDataDictionary(viewData);
			viewPage.Controls.Add(viewPage.LoadControl(controlName));

			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				using (HtmlTextWriter tw = new HtmlTextWriter(sw))
				{
					viewPage.RenderControl(tw);
				}
			}

			return sb.ToString();
		}

        public static string GetDisplayTime(DateTime time)
		{
			if (DateTime.Today == time.Date)
				return time.ToShortTimeString();
			else
				return time.ToShortDateString();
		}
    }
}