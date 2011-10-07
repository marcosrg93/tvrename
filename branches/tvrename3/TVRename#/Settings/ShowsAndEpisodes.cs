// 
// Main website for TVRename is http://tvrename.com
// 
// Source code available at http://code.google.com/p/tvrename/
// 
// This code is released under GPLv3 http://www.gnu.org/licenses/gpl.html
// 
using System;
using System.IO;
using System.Xml;
using TVRename.Settings;
using TVRename.Shows;
using System.Collections.Generic;
using TVRename.db_access.repository;
using TVRename.db_access;

// These are what is used when processing folders for missing episodes, renaming, etc. of files.

// A "ProcessedEpisode" is generated by processing an Episode from thetvdb, and merging/renaming/etc.
//
// A "ShowItem" is a show the user has added on the "My Shows" tab

// TODO: C++ to C# conversion stopped it using some of the typedefs, such as "IgnoreSeasonList".  (a) probably should
// rename that to something more generic like IntegerList, and (b) then put it back into the classes & functions
// that use it (e.g. ShowItem.IgnoreSeasons)

namespace TVRename
{
    public class ProcessedEpisode : Episode
    {
        public int EpNum2; // if we are a concatenation of episodes, this is the last one in the series. Otherwise, same as EpNum
        public bool Ignore;
        public bool NextToAir;
        public int OverallNumber;
        public string ShowItemID;
        public string ShowEffectiveName;

        private ShowItemRepository repo = null;
        private ShowItem parentShowItem = null;

        public ProcessedEpisode(SeriesInfo ser, Season seas, ShowItem si)
            : base(ser, seas)
        {
            this.NextToAir = false;
            this.OverallNumber = -1;
            this.Ignore = false;
            this.EpNum2 = this.EpNum;
            this.ShowItemID = si.innerDocument.Id;
            this.ShowEffectiveName = si.ShowName;
        }

        public ProcessedEpisode(ProcessedEpisode O)
            : base(O)
        {
            this.NextToAir = O.NextToAir;
            this.EpNum2 = O.EpNum2;
            this.Ignore = O.Ignore;
            this.ShowItemID = O.ShowItemID;
            this.OverallNumber = O.OverallNumber;
        }

        public ProcessedEpisode(Episode e, ShowItem si)
            : base(e)
        {
            this.OverallNumber = -1;
            this.NextToAir = false;
            this.EpNum2 = this.EpNum;
            this.Ignore = false;
            this.ShowEffectiveName = si.ShowName;
            this.ShowItemID = si.innerDocument.Id;
        }

        public ShowItem getParentShowItem()
        {
            if (repo == null)
                repo = new ShowItemRepository(RavenSession.SessionInstance);

            if (parentShowItem == null)
                parentShowItem = repo.Load(ShowItemID);

            if (parentShowItem == null)
            {
                parentShowItem = new ShowItem();
            }
            return parentShowItem;
        }

        public string NumsAsString()
        {
            if (this.EpNum == this.EpNum2)
                return this.EpNum.ToString();
            else
                return this.EpNum + "-" + this.EpNum2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static int EPNumberSorter(ProcessedEpisode e1, ProcessedEpisode e2)
        {
            int ep1 = e1.EpNum;
            int ep2 = e2.EpNum;

            return ep1 - ep2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static int DVDOrderSorter(ProcessedEpisode e1, ProcessedEpisode e2)
        {
            int ep1 = e1.EpNum;
            int ep2 = e2.EpNum;

            string key = "DVD_episodenumber";
            if (e1.Items.ContainsKey(key) && e2.Items.ContainsKey(key))
            {
                string n1 = e1.Items[key];
                string n2 = e2.Items[key];
                if ((!string.IsNullOrEmpty(n1)) && (!string.IsNullOrEmpty(n2)))
                {
                    try
                    {
                        int t1 = (int) (1000.0 * double.Parse(n1));
                        int t2 = (int) (1000.0 * double.Parse(n2));
                        ep1 = t1;
                        ep2 = t2;
                    }
                    catch (FormatException)
                    {
                    }
                }
            }

            return ep1 - ep2;
        }
    }

    // ShowItem

    public class EpisodeDict : System.Collections.Generic.Dictionary<int, List<ProcessedEpisode>>
    {
    }

    // dictionary by season #

    public class IgnoreSeasonList : System.Collections.Generic.List<int>
    {
    }

    public class FolderLocationDict : System.Collections.Generic.Dictionary<int, StringList>
    {
    }
}