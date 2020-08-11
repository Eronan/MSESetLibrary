using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace MSESetLibrary
{
    public class MSESet
    {
        /*
         * title: Outcast Provocation
	     * description:
		 *     A clan of outcasts from the United Sanctuary and Dragon Empire has finished forming together. Will this become dangerous?
		 * 
		 *     This is just the beginning.
	     * artist: 
	     * copyright: 
	     * symbol: symbol3.mse-symbol
	     * border color: rgb(255,255,255)
	     * automatic reminder text: old, core, expert, custom, fanmade
	     * automatic card numbers: no
	     * sort special rarity: after other cards
        */
        //Variables
        private string game;
        private string stylesheet;
        private string mseversion;
        private List<MSEKeyValue> setInfo; //SetInfo
        private Dictionary<string, byte[]> imageList; //Images converted to byte arrays
        private MSECard[] cardsList; //cards in the Set
        private MSEKeyword[] keywordsList; //Keywords unique to the Set

        public MSESet(string path)
        {
            //Read Zip Archive
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            {
                using (ZipArchive mseSet = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry zipEntry in mseSet.Entries)
                    {
                        if (zipEntry.Name.EndsWith(".mse-symbol"))
                        {
                            //Create XML
                        }
                        else if (zipEntry.Name.StartsWith("image"))
                        {
                            //Load Image
                        }
                        else if (zipEntry.Name == "set")
                        {
                            //Set Details
                            GetSetDetails(zipEntry);
                        }
                    }
                }
            }
        }

        private void GetSetDetails(ZipArchiveEntry setFile)
        {
            //Read Text File

        }

        private List<MSEKeyValue> ConvertToArray(string msedata)
        {
            //Remove Indent
            //string[] lines = msedata.Split('\n');
            string[] sections = Regex.Split(msedata, @"(\n[A-Za-z])+");
            List<MSEKeyValue> mseList = new List<MSEKeyValue>();
            for (int i = 0; i < sections.Length; i++)
            {
                string section = sections[i];
                //Create KeyTypeValue
                if (!section.Contains("\n")) mseList.Add(CreateMSEKeyValue(section));
                else
                {
                    //Get Index to Separate Title from NewData
                    int separateAt = section.IndexOf(':');
                    //Separate Data
                    string title = section.Substring(0, separateAt).Trim();
                    string newdata = section.Substring(separateAt).Replace("\n\t", "\n");
                    //Recursion
                    List<MSEKeyValue> sectionData = ConvertToArray(newdata);
                    //Add to List
                    mseList.Add(new MSEKeyValue(title, typeof(List<MSEKeyValue>), sectionData));
                }
            }

            return mseList;
        }

        //Initializing MSEKeyValue
        private static MSEKeyValue CreateMSEKeyValue(string msedataline)
        {
            //Separate into Key and Data
            string[] keyvalue = msedataline.Split(':');
            //Not Valid Line
            if (keyvalue.Length < 2) throw new Exception("Line is not a data line, and not suitable for a KeyValue Pair.");
            //Get Data Value
            string data = keyvalue[1].Trim();
            DateTime returnDate;
            //
            if (data.StartsWith("rgb("))
            {
                //Convert String to Colour
                string[] colours = data.Substring(4, data.LastIndexOf(')') - 4).Split(',');
                Color returnColour = Color.FromArgb(int.Parse(colours[0]), int.Parse(colours[1]), int.Parse(colours[2]));
                //Return MSEKeyValue
                return new MSEKeyValue(keyvalue[0].Trim(), typeof(Color), returnColour);
            }
            //Return integer: might not need
            //else if (int.TryParse(data, out returnParse)) return new MSEKeyValue(keyvalue[0].Trim(), typeof(int), returnParse);
            //Return DateTime
            else if (DateTime.TryParseExact(data, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out returnDate))
                return new MSEKeyValue(keyvalue[0].Trim(), typeof(DateTime), returnDate);
            //Return String
            else return new MSEKeyValue(keyvalue[0].Trim(), typeof(string), data.Trim());
        }
    }

    public class MSECard
    {
        public string ImageName { get; set; }
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public List<MSEKeyValue> CardInfo { get; set; }
        public string stylesheet { get; set; }
    }

    public class MSEKeyword
    {
        public string Keyword { get; set; }
        public string Match { get; set; }
        public string Reminder { get; set; }
        public string Mode { get; set; }
    }

    /*
    public class MSEStyle
    {
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public 
    }
    */
    public class MSEKeyValue
    {
        public string Key { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }

        public MSEKeyValue(string key, Type type, object value)
        {
            this.Key = key;
            this.Type = type;
            this.Value = value;
        }

        public bool IsType(Type compareType)
        {
            return compareType == this.Type;
        }

        public T GetValue<T>()
        {
            return (T)this.Value;
        }
    }
}
