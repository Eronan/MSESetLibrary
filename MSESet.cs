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
        string game;
        string stylesheet;
        string mseversion;
        MSEDictionary setInfo; //SetInfo
        Dictionary<string, byte[]> imageList; //Images converted to byte arrays
        List<MSECard> cardsList; //cards in the Set
        List<MSEKeyword> keywordsList; //Keywords unique to the Set

        /// <summary>
        /// Initializes a new instance of a <see cref="MSESet"/> from a file path.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="MSESet"/> with read from.</param>
        public MSESet(string path)
        {
            cardsList = new List<MSECard>();
            keywordsList = new List<MSEKeyword>();
            //Read Zip Archive
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            using (ZipArchive mseSet = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                //Read all Entries in Zip File
                foreach (ZipArchiveEntry zipEntry in mseSet.Entries)
                {
                    //Create XML
                    if (zipEntry.Name.EndsWith(".mse-symbol"))
                    {
                        
                    }
                    //Load Image
                    else if (zipEntry.Name.StartsWith("image"))
                    {
                        
                    }
                    //Set Details
                    else if (zipEntry.Name == "set")
                    {
                        GetSetDetails(zipEntry);
                    }
                }
            }
        }

        private void GetSetDetails(ZipArchiveEntry setFile)
        {
            //Read Set File
            using (var zipEntryStream = setFile.Open())
            using (StreamReader streamReader = new StreamReader(zipEntryStream))
            {
                //Read Text File
                string msedata = streamReader.ReadToEnd();
                setInfo = MSEDictionary.ConvertToDictionary(msedata);
            }

            //Retrieve data from MSEDictionary
            game = setInfo.RemoveAndGet("game").Value.ToString();
            stylesheet = setInfo.RemoveAndGet("stylesheet").Value.ToString();
            mseversion = setInfo.RemoveAndGet("mse version").Value.ToString();

            //Convert MSEDictionary of Cards to List of Cards
            IEnumerable<MSEKeyValue> cards = setInfo.RemoveAndGetAll("card");
            foreach (MSEKeyValue keyValue in cards)
            {
                if (keyValue.Type == typeof(MSECard)) cardsList.Add((MSECard)keyValue.Value);
            }

            //Convert MSEDictionary of Cards to List of Cards
            IEnumerable<MSEKeyValue> keywords = setInfo.RemoveAndGetAll("keyword");
            foreach (MSEKeyValue keyValue in keywords)
            {
                if (keyValue.Type == typeof(MSEKeyword)) keywordsList.Add((MSEKeyword)keyValue.Value);
            }
        }
    }

    public class MSECard
    {
        public string ImageName { get; set; }
        public string Notes { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public MSEDictionary StylingData { get; set; }
        public MSEDictionary CardInfo { get; set; }
        public string Stylesheet { get; set; }

        /// <summary>
        /// Initializes a <see cref="MSECard"/> from Section Data.
        /// </summary>
        /// <param name="data"></param>
        public MSECard(string data)
        {
            CardInfo = new MSEDictionary();
            string[] sections = Regex.Split(data, @"[\r\n]+(?![\t\n])");
            for (int i = 0; i < sections.Length; i++)
            {
                string section = sections[i];
                //Get Index to Separate Title from NewData
                int separateAt = section.IndexOf(':');

                //Continue if it is not a valid parameter
                if (separateAt == -1 || section.Length == 0) continue;
                else
                {
                    //Separate into Title and Value
                    string title = section.Substring(0, separateAt).Trim();
                    string value = section.Substring(separateAt + 1).Trim();
                    //Separate by Parameter Name
                    switch (title)
                    {
                        //Styling Data
                        case "styling data":
                            string newdata = Regex.Replace(value, @"(?<!\t)\t", "");
                            StylingData = MSEDictionary.ConvertToDictionary(newdata);
                            break;
                        //Image Name
                        case "image":
                            ImageName = value;
                            break;
                        //Notes
                        case "notes":
                            Notes = value;
                            break;
                        //Time Created & Modified
                        case "time created":
                            DateCreated = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                            break;
                        case "time modified":
                            DateModified = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                            break;
                        //Game Fields
                        default:
                            CardInfo.Add(new MSEKeyValue(title, typeof(string), value));
                            break;
                    }
                    
                }
            }
        }
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
        string _key;
        Type _type;
        object _value;

        /// <summary>
        /// Initializes a <see cref="MSEKeyValue"./>
        /// </summary>
        /// <param name="key">The key/parameter name.</param>
        /// <param name="type">The <see cref="System.Type"/> of <see cref="Type"/>.</param>
        /// <param name="value">The parameter being parsed.</param>
        public MSEKeyValue(string key, Type type, object value)
        {
            CommonConstrutor(key, type, value);
        }

        /// <summary>
        /// Initialize <see cref="MSEKeyValue"/> by reading a dataline.
        /// </summary>
        /// <param name="dataline">The line being read.</param>
        public MSEKeyValue(string dataline)
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
                CommonConstrutor(keyvalue[0].Trim(), typeof(Color), returnColour);
            }
            //Return DateTime
            else if (DateTime.TryParseExact(data, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out returnDate))
                CommonConstrutor(keyvalue[0].Trim(), typeof(DateTime), returnDate);
            //Return String
            else CommonConstrutor(keyvalue[0].Trim(), typeof(string), data.Trim());

            //Return integer: might not need
            //else if (int.TryParse(data, out returnParse)) return new MSEKeyValue(keyvalue[0].Trim(), typeof(int), returnParse);
        }

        private void CommonConstrutor(string key, Type type, object value)
        {
            this._key = key;
            this._type = type;
            this._value = value;
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

        /// <summary>
        /// The Name for the Key/Parameter of a component of the <see cref="MSESet"/>.
        /// </summary>
        public string Key
        { 
            get { return this._key; }
        }
        /// <summary>
        /// The type that <see cref="Value"/> should read as.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }
        /// <summary>
        /// The <see cref="Value"/> stored when initialised.
        /// </summary>
        public object Value
        {
            get { return _value; }
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

        /// <summary>
        /// Converts Section Text to an <see cref="MSEDictionary"/>.
        /// </summary>
        /// <param name="sectiontext">The text belonging to a section.</param>
        /// <returns>Converts Section Text to an <see cref="MSEDictionary"/>.</returns>
        public static MSEDictionary ConvertToDictionary(string sectiontext)
        {
            //Remove Indent
            //string[] lines = msedata.Split('\n');
            string[] sections = Regex.Split(sectiontext, @"[\r\n]+(?![\t\n])");
            MSEDictionary mseList = new MSEDictionary();
            for (int i = 0; i < sections.Length; i++)
            {
                string section = sections[i];
                //Get Index to Separate Title from NewData
                int separateAt = section.IndexOf(':');

                //Separate Data
                string title;
                string value;
                //1. Check if sectiontext can be separated
                //2. Get the Key of the sectiontext
                //3. Get the value of the sectiontext
                //4. Checks if value can be Split again into sections after removing Indents
                if (separateAt != -1 && (title = section.Substring(0, separateAt).Trim()).Length != 0 &&
                    Regex.IsMatch(value = Regex.Replace(section.Substring(separateAt + 1).Trim(), @"(?<!\t)\t", ""), @"[\r\n]+(?![\t\n])"))
                {
                    //Convert to MSECard instead
                    if (title == "card") mseList.Add(new MSEKeyValue(title, typeof(MSECard), new MSECard(value)));
                    else
                    {
                        //Recursion
                        MSEDictionary sectionData = ConvertToDictionary(value);
                        //Add to List
                        mseList.Add(new MSEKeyValue(title, typeof(List<MSEKeyValue>), sectionData));
                    }
                }
                else if (section.Length != 0) mseList.Add(new MSEKeyValue(section));

                //Create KeyTypeValue
                //if (!section.Contains("\n")) mseList.Add(CreateMSEKeyValue(section));
            }

            return mseList;
        }
    }
}
