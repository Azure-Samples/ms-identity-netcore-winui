using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUIMSALAppB2C.MSAL
{
    public class DownStreamApiConfig
    {
        /// <summary>
        /// Gets or sets the API base URL.
        /// </summary>
        /// <value>
        /// The API base URL.
        /// </value>
        public string ApiBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the scopes for API call.
        /// </summary>
        /// <value>
        /// The scopes as space separated string.
        /// </value>
        public string Scopes { get; set; }

        /// <summary>
        /// Gets the scopes in a format as expected by the various MSAL SDK methods.
        /// </summary>
        /// <value>
        /// The scopes as array.
        /// </value>
        public string[] ScopesArray
        {
            get
            {
                return Scopes.Split(' ');
            }
        }
    }
}
