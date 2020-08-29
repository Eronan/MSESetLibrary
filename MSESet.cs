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
        //Variables
        string stylesheet;
        MSEVersion gameVersion;
        string mseVersion;
        MSEDictionary setInfo; //SetInfo
        MSEDictionary styling; //Styling unique to this set
        Dictionary<string, byte[]> imageList; //Images converted to byte arrays
        List<MSECard> cardsList; //cards in the Set
        List<MSEKeyword> keywordsList; //Keywords unique to the Set
        MSEDictionary symbolData;

        /// <summary>
        /// Initializes a new instance of a <see cref="MSESet"/> from a file path.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="MSESet"/> with read from.</param>
        public MSESet(string path)
        {
            cardsList = new List<MSECard>();
            keywordsList = new List<MSEKeyword>();
            imageList = new Dictionary<string, byte[]>();
            //Read Zip Archive
            using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
            using (ZipArchive mseSet = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            {
                //Read all Entries in Zip File
                foreach (ZipArchiveEntry zipEntry in mseSet.Entries)
                {
                    using (var zipEntryStream = zipEntry.Open())
                    {
                        //Create XML
                        if (zipEntry.Name.EndsWith(".mse-symbol"))
                        {
                            //Read ZipEntry
                            using (StreamReader streamReader = new StreamReader(zipEntryStream))
                            {
                                //Read Text File
                                string symboldata = streamReader.ReadToEnd();
                                symbolData = MSEDictionary.ConvertToDictionary(symboldata);
                            }
                        }
                        //Load Image
                        else if (zipEntry.Name.StartsWith("image"))
                        {
                            //Add to Image List
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                zipEntryStream.CopyTo(memoryStream);
                                imageList.Add(zipEntry.Name, memoryStream.ToArray());
                            }
                        }
                        //Set Details
                        else if (zipEntry.Name == "set") GetSetDetails(zipEntryStream);
                    }
                }
            }
        }

        //Reads "set" File
        private void GetSetDetails(Stream zipEntryStream)
        {
            using (StreamReader streamReader = new StreamReader(zipEntryStream))
            {
                //Read Text File
                string msedata = streamReader.ReadToEnd();
                setInfo = MSEDictionary.ConvertToDictionary(msedata);
            }

            //Retrieve data from MSEDictionary
            gameVersion.Name = setInfo.RemoveAndGet("game").Value.ToString();
            gameVersion.VersionNumber = "0";
            stylesheet = setInfo.RemoveAndGet("stylesheet").Value.ToString();
            mseVersion = setInfo.RemoveAndGet("mse version").Value.ToString();
            styling = (MSEDictionary)setInfo.RemoveAndGet("styling").Value;

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

        /// <summary>
        /// Name of the default Stylesheet assigned to the Set.
        /// </summary>
        public string Stylesheet
        {
            get { return stylesheet; }
        }

        /// <summary>
        /// Game and Version Number of the Game that the set was made with.
        /// </summary>
        public MSEVersion GameVersion
        {
            get { return gameVersion; }
        }

        /// <summary>
        /// Version Number of MSE that the set was made with
        /// </summary>
        public string ProgramVersion
        {
            get { return mseVersion; }
        }

        /// <summary>
        /// Additional Set Info.
        /// </summary>
        public MSEDictionary SetInfo
        {
            get { return setInfo; }
            set { setInfo = value; }
        }

        /// <summary>
        /// Styling Parameters for the Set.
        /// </summary>
        public MSEDictionary StylingData
        {
            get { return styling; }
            set { styling = value; }
        }

        /// <summary>
        /// List of Cards in the Set.
        /// </summary>
        public List<MSECard> Cards
        {
            get { return cardsList; }
            set { cardsList = value; }
        }

        /// <summary>
        /// Keywords unique to the set.
        /// </summary>
        public List<MSEKeyword> Keywords
        {
            get { return keywordsList; }
            set { keywordsList = value; }
        }

        /// <summary>
        /// Images stored in the Set
        /// </summary>
        public Dictionary<string, byte[]> Images
        {
            get { return imageList; }
            set { imageList = value; }
        }
    }

    /// <summary>
    /// Represents a Card Entry in the Set File.
    /// </summary>
    public class MSECard
    {
        /// <summary>
        /// Extra Notes on the card.
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Date and Time the card was created.
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// Date and Time the card was last modified.
        /// </summary>
        public DateTime DateModified { get; set; }
        /// <summary>
        /// Styling Data unique to this card only.
        /// </summary>
        public MSEDictionary StylingData { get; set; }
        /// <summary>
        /// Card Info for the Game.
        /// </summary>
        public MSEDictionary CardInfo { get; set; }
        /// <summary>
        /// The unique Stylesheet the card uses.
        /// </summary>
        public string Stylesheet { get; set; }

        /// <summary>
        /// Initializes a <see cref="MSECard"/> from Section Data.
        /// </summary>
        /// <param name="data">Section Data for a Card.</param>
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
}
