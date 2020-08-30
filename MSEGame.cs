using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MSESetLibrary
{
    /// <summary>
    /// Denotes the Horizontal Alignment
    /// </summary>
    public enum MSEJustifiedAlignment
    {
        Justify = 1,
        JustifyAll = 2,
        Stretch = 4,
        Shrink = 8,
        Force = 16,
        OverflowJustify = 32,
        OverflowJustifyAll = 64,
        OverflowStretch = 128,
        OverflowShrink = 256,
    }

    /// <summary>
    /// Duplicate of the System.Drawing.ContentAlignment in specific .NET Core and Frameworks
    /// </summary>
    public enum ContentAlignment
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 4,
        MiddleLeft = 16,
        MiddleCenter = 32,
        MiddleRight = 64,
        BottomLeft = 256,
        BottomCenter = 512,
        BottomRight = 1024,
    }


    /// <summary>
    /// Game Package
    /// </summary>
    public class MSEGame
    {
        MSEVersion gameVersion;
        MSEDictionary extraInfo;
        string gameName;
        string shortName;
        string fullName;
        byte[] icon;
        int position;
        string installerGroup;
        bool hasKeywords;
        List<MSEKeyword> keywordsList;


        public MSEGame(string path)
        {
            string fileText = ReadAllFromFile(path + "\\game");
            //Find all include file functions
            MatchCollection includes = Regex.Matches(fileText, "((?<!#)[^#\n\r]*)include file: (.+)");
            foreach (Match match in includes)
            {
                fileText = fileText.Replace(match.Value, match.Groups[1].Value.Trim() + "\n" + ReadAllFromFile(path + "\\" + match.Groups[2].Value.Trim()));
            }

            fileText = Regex.Replace(fileText, "#(.*)", "");
            //Compile File Text into MSEGame
            extraInfo = MSEDictionary.ConvertToDictionary(fileText);

            //Gather Information from Dictionary
            gameVersion.Name = path.Substring(path.LastIndexOf('\\') + 1);
            gameVersion.VersionNumber = extraInfo.RemoveAndGet("version").Value.ToString();
            installerGroup = extraInfo.RemoveAndGet("installer group").Value.ToString();
            position = Convert.ToInt32(extraInfo.RemoveAndGet("position hint").Value);
            icon = File.ReadAllBytes(path + "\\" + extraInfo.RemoveAndGet("icon").Value.ToString());

            hasKeywords = bool.Parse(extraInfo.RemoveAndGet("has keywords").Value.ToString());
            if (hasKeywords)
            {
                //Convert MSEDictionary of Keywords to List of Keywords
                IEnumerable<MSEKeyValue> keywords = extraInfo.RemoveAndGetAll("keyword");
                foreach (MSEKeyValue keyValue in keywords)
                {
                    if (keyValue.Type == typeof(MSEKeyword)) keywordsList.Add((MSEKeyword)keyValue.Value);
                }
            }

            //Get all the Card Fields
        }
        
        private string ReadAllFromFile(string filepath)
        {
            string returnText = "";
            using (StreamReader reader = new StreamReader(filepath)) returnText = reader.ReadToEnd();
            return returnText;
        }

        public static explicit operator MSEVersion(MSEGame obj)
        {
            return obj.gameVersion;
        }
    }

    /// <summary>
    /// Represents a Field in Magic Set Editor
    /// </summary>
    public class MSEField
    {
        Type _type;
        string _name;
        string _description;
        byte[] icon;
        bool _editable;
        bool _saveValue;
        bool _showStatistics;
        bool _identifying;
        int _cardListColumn;
        int _cardListWidth;
        bool _cardListVisible;
        bool _cardListAllow;
        string _cardListName;
        MSEJustifiedAlignment _justifiedAlignment;
        ContentAlignment _contentAlignment;
        MSEScript _sortScript;
        int _tabIndex;

        public MSEField(string sectionText)
        {

        }

        public MSEField(MSEDictionary keyValues)
        {

        }

        private void GetFromDictionary(MSEDictionary keyValues)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MSEText : MSEField
    {
        MSEScript _script;
        MSEScript _default;
        string _defaultName;
        bool _multiLine;

        public MSEText(MSEDictionary keyValues) : base(keyValues)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MSEChoice : MSEField
    {
        MSEScript _script;
        MSEScript _default;
        string _initial;
        string _defaultName;
        List<string> _choices;
        List<Color> _choiceColours;

        public MSEChoice(MSEDictionary keyValues) : base(keyValues)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MSEPackage : MSEField
    {
        MSEScript _script;
        MSEScript _default;
        string _initial;
        string _defaultName;
        List<string> _choices;
        List<Color> _choiceColours;

        public MSEChoice(MSEDictionary keyValues) : base(keyValues)
        {

        }
    }

    /// <summary>
    /// Stylesheet for MSE
    /// </summary>
    public class MSEStyle
    {
        List<MSEVersion> dependsOn = new List<MSEVersion>();
    }

    /// <summary>
    /// Game Package Name and Version Number
    /// </summary>
    public struct MSEVersion
    {
        public string Name { get; set; }
        public string VersionNumber { get; set; }
    }

    /// <summary>
    /// Extensions for Structures
    /// </summary>
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
            if (gVersion1.Name.Equals(gVersion2.Name, StringComparison.OrdinalIgnoreCase)) return false;
            return gVersion1.VersionNumber.CompareTo(gVersion2.VersionNumber) <= 0;
        }
    }

    public class MSEScript
    {
        /// <summary>
        /// The Text for the Script
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// Builds a Script Class of MSE
        /// </summary>
        /// <param name="scriptText">Text Value for the JSON</param>
        public MSEScript(string scriptText)
        {
            this.Script = scriptText;
        }
    }
}
