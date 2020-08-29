using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace MSESetLibrary
{

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
            else if (DateTime.TryParseExact(data, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out returnDate))
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
            set { _value = value; }
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
            string[] sections = Regex.Split(sectiontext, @"[\r\n]+(?![\t\r\n])");
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
                    Regex.IsMatch(value = Regex.Replace(section.Substring(separateAt + 1).Trim(), @"(?<!\t)\t", ""), @"[\r\n]+\t"))
                {
                    //Convert to MSECard instead
                    if (title == "card") mseList.Add(new MSEKeyValue(title, typeof(MSECard), new MSECard(value)));
                    else if (title == "keyword") mseList.Add(new MSEKeyValue(title, typeof(MSEKeyword), new MSEKeyword(value)));
                    //else if (title == "extra card field" || title == "card field") mseList.Add(new MSEKeyValue(title, typeof(MSEField), new MSEField(value)));
                    else if (title == "init script" || title == "script") mseList.Add(new MSEKeyValue(title, typeof(string), value));
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

    /// <summary>
    /// Represents a Custom Keyword Entry in the Set File.
    /// </summary>
    public class MSEKeyword
    {
        /// <summary>
        /// Keyword Name
        /// </summary>
        public string Keyword { get; set; }
        /// <summary>
        /// What needs to match for the Keyword
        /// </summary>
        public string Match { get; set; }
        /// <summary>
        /// Reminder Text for Keyword
        /// </summary>
        public string Reminder { get; set; }
        /// <summary>
        /// The Mode the Keyword exists in
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Initializes a <see cref="MSEKeyword"/> from Section Data.
        /// </summary>
        /// <param name="data">Section Data for a Keyword.</param>
        public MSEKeyword(string data)
        {
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
                        case "keyword":
                            Keyword = value;
                            break;
                        case "match":
                            Match = value;
                            break;
                        case "mode":
                            Mode = value;
                            break;
                        case "reminder":
                            Reminder = value;
                            break;
                        //Game Fields
                        default:
                            Console.WriteLine(value);
                            break;
                    }

                }
            }
        }
    }
}
