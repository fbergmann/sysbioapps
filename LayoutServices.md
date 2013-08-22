#Layout Services 
To make it easier to use SBGN files, or files with the SBML Layout / Rendering Packages, the online SBML Viewer: 

[http://sysbioapps.dyndns.org/Layout](sysbioapps.dyndns.org/Layout)

Now also features a REST API. Currently there are two services. As for error handling, if an error occurs a 404 error will be returned. 

##GenerateImage
This action generates a PNG for a posted SBML / SBGN file. Should the file contain no SBML Layout, one will be generated. So how does this work. The URL to use is: 

	http://sysbioapps.dyndns.org/Layout/GenerateImage

You simply post to this URL the following HTTP Form elements:
	
 	file         - the SBML file
    scale        - an optional floating point scale for the image (defaults to 2)
    height       - scale image to fit the height
    width        - scale image to fit width 

If `height` is given, `width` needs to be given as well. 

##GetLayoutSBML
This action simply loads the given SBML file, and writes it out using the [SBML Layout library](http://sbmllayout.sf.net). Should no layout, exist, one will be generated. The URL is: 

	http://sysbioapps.dyndns.org/Layout/GetLayoutSBML

You simply post to this URL the following HTTP Form elements:
	
 	file         - the SBML file

## Example
So for example, if you wanted to post a local file to the service with [curl](http://curl.haxx.se/), you'd run:

	curl -X POST -F file=@"E:\Users\fbergmann\Documents\SBML Models\BorisEJB.xml" http://sysbioapps.dyndns.org/Layout/GenerateImage -o out.png

lets take the arguments apart: 

* `-X POST ` post FORM data
* `-F file=@filename` post `filename` to the URL
* `http://sysbioapps.dyndns.org/Layout/GenerateImage` the URL
* `-o out.png` write the resulting PNG to the current directory as `out.png`. 

---
8/22/2013 3:09:50 PM Frank T. Bergmann 