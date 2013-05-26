tvNZB
=========
tvNZB is a tivo for usenet. The program fetches NZB files from newzbin.com whenever a new episode for a show is aired according to tvrage.com. The nzb files are then sent to SABnzbd. The list of shows can be modified using the web interface.

 - Maintains an XML database of user inputted shows via ASP.NET interface
 - Current episode information is gathered and compared by using API calls to www.tvrage.com
 - Matched files are sent SABnzbd via their API
 - Shows are sorted by latest airtime

Requirements
---------------

 * .NET framework 2.0 or greater
 * SABnzbd (www.sabnzbd.org)
 * Newzbin account (www.newzbin.com)

Usage
---------------

 * Navigate to the tvNZB folder in the start menu and open the tvNZB url.
 
 * Upon first execution, you will be directed to the config page.

 * Home - Add/edit/delete shows

    - Add Show: To add a show, click the Add Show link at the bottom of the page.

	- SearchType:
		-- ShowName Season x Episode - "Lost 3x04".
		-- Showname AbsoluteEpisode  - "Naruto 127".
		-- Showname YYYY-MM-DD       - "Conan 2008-02-18".
		-- Showname EpisodeTitle     - "Survivor Knights of the Round Table"
		
 * Config - Shows current configuration
 
 * Log - Shows what searches for tvshows were last done.

Changelog
---------------
 * 2.0
    - Created an installer that has Cassini web server built in and configured for the application.  Also downloads/installs .Net frameworks.
	- Completely redone the web interface with configuration, log and home pages.
	- Made tvnzb.exe a windows service that logs it's searches to the eventlog under "tvNZB"
	- Removed the need to input season/episode when adding a show.  Program just grabs latest information from tvrage.	

 * 1.1
	- Merged columns episode and season in the web interface and adjusted corresponding edit modes.
	- Included next episode information in the Next Show column.
	- Fixed bug where tvrage.com was gathered every instance of the program.
	- Fixed bug where when a search found the nzb, the xml file wasn't updated properly.
	- Renamed autoNZB.exe to tvNZB.exe (it was the original name of the program).

 * 1.0 
	- First release