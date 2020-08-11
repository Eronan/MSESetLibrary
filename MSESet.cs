using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace MSESetLibrary
{
    /// <summary>
    /// Represents an ".mse-set" file.
    /// </summary>
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
        private MSEDictionary setInfo; //SetInfo
        private Dictionary<string, byte[]> imageList; //Images converted to byte arrays
        private MSECard[] cardsList; //cards in the Set
        private MSEKeyword[] keywordsList; //Keywords unique to the Set

        /// <summary>
        /// Initializes a new instance of a <see cref="MSESet"/> from a file path.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="MSESet"/> with read from.</param>
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
            using (var zipEntryStream = setFile.Open())
            using (StreamReader streamReader = new StreamReader(zipEntryStream))
            {
                //Read Text File
                string msedata = streamReader.ReadToEnd();
                setInfo = ConvertToDictionary(msedata);
            }

            game = setInfo.RemoveAndGet("game").Value.ToString();
            stylesheet = setInfo.RemoveAndGet("stylesheet").Value.ToString();
            mseversion = setInfo.RemoveAndGet("mse version").Value.ToString();

            IEnumerable<MSEKeyValue> cardList = setInfo.RemoveAndGetAll("card");
            IEnumerable<MSEKeyValue> keywordList = setInfo.RemoveAndGetAll("keyword");
        }

        //Converts a Section to an MSEDictionary
        private MSEDictionary ConvertToDictionary(string sectiontext)
        {
            //Remove Indent
            //string[] lines = msedata.Split('\n');
            string[] sections = Regex.Split(sectiontext, @"(\n[A-Za-z])+");
            MSEDictionary mseList = new MSEDictionary();
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
                    MSEDictionary sectionData = ConvertToDictionary(newdata);
                    //Add to List
                    mseList.Add(new MSEKeyValue(title, typeof(List<MSEKeyValue>), sectionData));
                }
            }

            return mseList;
        }

        //Initializing MSEKeyValue
        private static MSEKeyValue CreateMSEKeyValue(string dataline)
        {
            //Separate into Key and Data
            string[] keyvalue = dataline.Split(':');
            //Not Valid Line
            if (keyvalue.Length < 2) throw new Exception("Line is not a data line, and not suitable for a MSEKeyValue.");
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
        public MSEDictionary CardInfo { get; set; }
        public string Stylesheet { get; set; }
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
    /// <summary>
    /// Represents a KeyValue Pair where the value is of a varying type
    /// </summary>
    public class MSEKeyValue
    {
        /// <summary>
        /// The Name for the Key/Parameter of a component of the <see cref="MSESet"/>.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The type that <see cref="Value"/> should read as.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// The <see cref="Value"/> stored when initialised.
        /// </summary>
        public object Value { get; }

        //Constructor
        public MSEKeyValue(string key, Type type, object value)
        {
            this.Key = key;
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Checks if the <see cref="MSEKeyValue"/> of the specific type
        /// </summary>
        /// <param name="compareType">The type to compare it to</param>
        /// <returns>Checks if the <see cref="MSEKeyValue"/> of the specific type</returns>
        public bool IsType(Type compareType)
        {
            return compareType == this.Type;
        }

        /// <summary>
        /// Returns the Value as Type T.
        /// </summary>
        /// <typeparam name="T">The object Type the value should be returned as.</typeparam>
        /// <returns>Returns the Value as Type T.</returns>
        public T GetValue<T>()
        {
            return (T)this.Value;
        }
    }

    /// <summary>
    /// Represents a <see cref="List{MSEKeyValue}"/> of <see cref="MSEKeyValue"/> that can be accessed by index.
    /// </summary>
    public class MSEDictionary : List<MSEKeyValue>
    {
        /// <summary>
        /// Removes the first occurence of a key in <see cref="MSEDictionary"/>
        /// </summary>
        /// <param name="key">The Key of a value to be removed</param>
        /// <returns>Removes the first occurence of a key in <see cref="MSEDictionary"/></returns>
        public bool Remove(string key)
        {
            for (int i = 0; i < this.Count; i++)
            {
                //Key is the same
                if (this[i].Key == key)
                {
                    this.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        //Get Value of Type
        /// <summary>
        /// Gets an <see cref="MSEKeyValue"/> from the <see cref="MSEDictionary"/> with a matching key and type.
        /// </summary>
        /// <typeparam name="T">The Type of the value to be returned</typeparam>
        /// <param name="key">The Key of a <see cref="MSEKeyValue"/></param>
        /// <returns>Returns the first value of a <see cref="MSEKeyValue"/> with a matching key and type.</returns>
        public T GetValue<T>(string key)
        {
            //Search List
            foreach (MSEKeyValue keyValue in this)
            {
                //Key is the same
                if (keyValue.Key == key)
                {
                    //Return Value
                    if (keyValue.Type == typeof(T)) return (T)keyValue.Value;
                    else return default(T);
                }
            }
            //Return default if not found
            return default(T);
        }

        /// <summary>
        /// Gets the first <see cref="MSEKeyValue"/> from the <see cref="MSEDictionary"/> with a matching key.
        /// </summary>
        /// <param name="key">The Key of a <see cref="MSEKeyValue"/></param>
        /// <returns>Returns the first <see cref="MSEKeyValue"/> with a matching key.</returns>
        public MSEKeyValue GetValue(string key)
        {
            foreach (MSEKeyValue keyValue in this)
            {
                //Key is the same
                if (keyValue.Key == key) return keyValue;
            }
            //Return Null if not found
            return null;
        }

        /// <summary>
        /// Returns and removes a <see cref="MSEKeyValue"/> from <see cref="MSEDictionary"/> with a matching key.
        /// </summary>
        /// <param name="key">The Key of a <see cref="MSEKeyValue"/></param>
        /// <returns>Returns and removes the first <see cref="MSEKeyValue"/> with a matching key.</returns>
        public MSEKeyValue RemoveAndGet(string key)
        {
            for (int i = 0; i < this.Count; i++)
            {
                //Key is the same
                if (this[i].Key == key)
                {
                    MSEKeyValue keyValue = this[i];
                    this.RemoveAt(i);
                    return keyValue;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a list of all <see cref="MSEKeyValue"/> with matching keys.
        /// </summary>
        /// <param name="key">The Key of a <see cref="MSEKeyValue"/></param>
        /// <returns>Returns a list of all <see cref="MSEKeyValue"/> with matching keys.</returns>
        public IEnumerable<MSEKeyValue> FindAll(string key)
        {
            List<MSEKeyValue> keyValues = new List<MSEKeyValue>();
            foreach (MSEKeyValue keyValue in this)
            {
                //Key is the same
                if (keyValue.Key == key) keyValues.Add(keyValue);
            }
            return keyValues;
        }

        /// <summary>
        /// Returns a list of all <see cref="MSEKeyValue"/> with matching keys.
        /// </summary>
        /// <param name="key">The Key of a <see cref="MSEKeyValue"/></param>
        /// <returns>Returns a list of all <see cref="MSEKeyValue"/> with matching keys.</returns>
        public bool RemoveAll(string key)
        {
            bool hasDeleted = false;
            //Loop Backwards to prevent bad deletions
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].Key == key)
                {
                    this.RemoveAt(i);
                    hasDeleted = true;
                }
            }
            //Return result of deletion
            return hasDeleted;
        }

        /// <summary>
        /// Returns and deletes a list of all <see cref="MSEKeyValue"/> with matching keys.
        /// </summary>
        /// <param name="key">The Key of a <see cref="MSEKeyValue"/></param>
        /// <returns>Returns and deletes a list of all <see cref="MSEKeyValue"/> with matching keys.</returns>
        public IEnumerable<MSEKeyValue> RemoveAndGetAll(string key)
        {
            List<MSEKeyValue> keyValues = new List<MSEKeyValue>();
            //Loop Backwards to prevent bad deletions
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (this[i].Key == key)
                {
                    keyValues.Add(this[i]);
                    this.RemoveAt(i);
                }
            }
            return keyValues;
        }
    }
}
