//
// Main website for TVRename is http://tvrename.com
//
// Source code available at http://code.google.com/p/tvrename/
//
// This code is released under GPLv3 http://www.gnu.org/licenses/gpl.html
//
#include "stdafx.h"
#pragma hdrstop

#include "RSS.h"
#include "TVDoc.h"

namespace TVRename
{

    bool RSSItemList::ReadItem(XmlReader ^r)
    {
        String ^title = "";
        String ^link = "";
        String ^description = "";

        r->Read();
        r->Read();
        while (!r->EOF)
        {
            if ((r->Name == "item") && (!r->IsStartElement()))
                break;
            if (r->Name == "title")
                title = r->ReadElementContentAsString();
            else if (r->Name == "description")
                description = r->ReadElementContentAsString();
			else if ((r->Name == "link") && (String::IsNullOrEmpty(link)))
                link = r->ReadElementContentAsString();
			else if ((r->Name == "enclosure") && (r->GetAttribute("type") == "application/x-bittorrent"))
			{
				link = r->GetAttribute("url");
				r->ReadOuterXml();
			}
            else 
                r->ReadOuterXml();
        }
        if ((String::IsNullOrEmpty(title)) || (String::IsNullOrEmpty(link)))
            return false;

        int season = -1, episode = -1;
        String ^showName = "";

        TVDoc::FindSeasEp("", title, &season, &episode, nullptr, Rexps);

        try 
        {
            Match ^m = Regex::Match(description, "Show Name: (.*?)[;|$]", RegexOptions::IgnoreCase);
            if (m->Success)
                showName = m->Groups[1]->ToString();
            m = Regex::Match(description, "Season: ([0-9]+)", RegexOptions::IgnoreCase);
            if (m->Success)
                season = int::Parse(m->Groups[1]->ToString());
            m = Regex::Match(description, "Episode: ([0-9]+)", RegexOptions::IgnoreCase);
            if (m->Success)
                episode = int::Parse(m->Groups[1]->ToString());
        }
        catch (...)
        {
        }

        if ((season != -1) && (episode != -1))
            Add(gcnew RSSItem(link, title, season, episode, showName));

        return true;   


    }

} // namespace