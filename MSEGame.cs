using System;
using System.Collections.Generic;
using System.Text;

namespace MSESetLibrary
{
    class MSEGame
    {
        MSEVersion gameVersion;

        public MSEGame()
        {
            
        }
    }

    public class MSEStyle
    {
        List<MSEVersion> dependsOn = new List<MSEVersion>();
    }

    public struct MSEVersion
    {
        public string Name { get; set; }
        public string VersionNumber { get; set; }
    }

    public static class Extensions
    {
        /// <summary>
        /// Checks if the first Game Version is compatible with the second. (Second gVersion is a higher version number)
        /// </summary>
        /// <param name="gVersion1">The Game Version to compare.</param>
        /// <param name="gVersion2">The Game Version to compare with.</param>
        /// <returns></returns>
        public static bool CompatibleWith(this MSEVersion gVersion1, MSEVersion gVersion2)
        {
            if (gVersion1.Name != gVersion2.Name) return false;
            return gVersion1.VersionNumber.CompareTo(gVersion2.VersionNumber) <= 0;
        }
    }

    
}
