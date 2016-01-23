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
using System.Web;
using System.Web.Util;

namespace Palaver2
{
    public class CustomRequestValidator : RequestValidator
    {
        public CustomRequestValidator() { }

        protected override bool IsValidRequestString(
            HttpContext context, string value,
            RequestValidationSource requestValidationSource, string collectionKey,
            out int validationFailureIndex)
        {
            validationFailureIndex = -1;  //Set a default value for the out parameter.

            //Allow the query-string key data to have a value that is formatted like XML.
            if ((requestValidationSource == RequestValidationSource.Form) &&
                (collectionKey == "data"))
            {
                //The querystring value wrapped in {} is automatically allowed as a JSON request for the data key.
                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    validationFailureIndex = -1;
                    return true;
                }
                else
                    //Leave any further checks to ASP.NET.
                    return base.IsValidRequestString(context, value,
                    requestValidationSource,
                    collectionKey, out validationFailureIndex);
            }
            //All other HTTP input checks are left to the base ASP.NET implementation.
            else
            {
                return base.IsValidRequestString(context, value, requestValidationSource,
                                                 collectionKey, out validationFailureIndex);
            }
        }
    }
}
