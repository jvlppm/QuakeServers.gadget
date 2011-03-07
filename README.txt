Windows Vista Sidebar Gadget Template
==============================================
Date: 10 NOV 2006
Update: 11 MAR 2007 (to include the locale folder structure)

This is meant to be a stub template for you to ensure some minimum and helpful information
is included in your Vista Sidebar Gadget.  Sidebar Gadgets cannot directly interpret server-side code, 
so be sure not to confuse this as a full web site that can be hosted within the Windows Vista Sidebar.

More Resources:
==============================================
For more information on the Gadget API reference and to get started, visit the MSDN site for Sidebar 
Gadget development:
    
    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sidebar/sidebar/overviews/gdo.asp
    

Some notes about the template files:
===============================================
Localized folders:
				it is probably a good practice to develop using locale informaiton in mind.  this template uses the en-US
				folder by default and you can create locale-specific implementations of the gadget in other folders.
				
				if items are shared across the locale's, consider moving them to a more global location and maintaining your 
				locale-specific items only in the locale folders
				
gadget.html --  no specific name needed, 'gadget' just chosen as a default here, feel free to rename.  You will
                have to pick what file is the default and put that in the gadget.xml manifest file.

gadget.xml --   this is the manifest file that describes the gadget to the sidebar.  There are a few image files 
                in the template that reference logo.png, icon.png, and drag.png.  These files are not 
                included here...but meant as guides to include your logo for the details tile (48x48 pixels), 
                your default icon for the Gadget Tile display (48x48 pixels) and an optional drag file for 
                when the user selects the Gadget from the tile display and drags it onto the Sidebar (recommend 
                sticking with 130 pixels wide.

utils.js   --   this file includes some helper utilities that you can utilize in your gadget HTML.

settings.html - a default stub for any custom settings your Gadget would need/want.

flyout.html --  a default stub for any flyout for your Gadget.