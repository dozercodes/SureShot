Before you can run this tutorial, you will need to download some of the dlls mentioned in the 
GoblinXNAInstallationGuide.docx.

We strongly recommend to read the artag_rev2.doc which comes with the ARTag SDK, and run their demos to
have better understanding of how ARTag works. 

The ARTag.cf file contains bunch of marker (fiducial) configurations, and you are welcome to
change the configurations, but for now, please leave those as it is, otherwise, you may have some
problems rendering the tracked objects. (For detailed descrption of the .cf file format, please see
below)

In order to make the tutorial work, please print out "ground.pdf" and "toolbar.pdf" (NOTE: When you print out the PDF file using
Acrobat Reader, make sure you change "Page Scaling" under "Page Handling" to be "None. Otherwise, the inche measures
won't be exact as noted on the sheet) and bring it to a place where the camera can see the array of ground markers (fiducials). 
You should see a green 3D sphere rendered on top of the center of array of markers as well as a red 3D box at one of
the corner. Once you bring the toolbar1 marker to a place where the camera can see, you will see that the red 3D box
is now rendered on top of the toolbar1 marker. For this simple demo, it uses 24 (6x4) markers plus bunch of small markers
between them to perform the tracking. The more markers you use and the larger the size of the marker is, the more stable the 
tracking will be. The smaller markers are there to provide tracking when the camera is really close to the array.
Also, if you put the print out (sheet of paper with markers) on a clip board or any type of flat rigid 
surface, the tracking will be more stable than holding it by your hands.  


ARTag Configuration (.cf) File Format:
-- An array of markers starts with the a tag ‘coordframe’ and ends with a tag ‘/coordframe’
-- Each array of markers is assigned a name with the following syntax: name=”array_name”
-- Each marker definition starts with a tag ‘marker’ and ends with a tag ‘/marker’
-- Each marker is assigned a predefined ID with the following syntax: id=number
-- Each marker defines the 4 corners of the marker. Each point has 3 coordinates,
   but the 3rd coordinate (z coordinate) has to be always 0. (If you change it to a nonzero
   value, it won’t work!!). The number can be a floating point.
-- Note that you can’t have duplicated marker (marker with same ID) defined in
   one .cf file. If you duplicate a marker, the newer definition (whichever comes
   after is the newer definition) will overwrite the old one, so it will mess up the
   array which has the old definition. 