NFountain
=========

A Fountain support library & console app with PDF output for .NET

About Fountain
==============

Fountain is a simple, plaintext markup language for screenplays & screenwriters.  It's a simple way to write a script in a plain text editor, which can then be converted to PDF or other formats using any of a number of Fountain coversion tools.

For more information, visit: [Fountain.IO](http://fountain.io/).

About NFountain
===============

NFountain is a fountain support library and console application written in pure .NET.  It supports most of the fountain synatx, but is not 100% compliant.  As time goes on, discrepencies will either be resolved or documented.  I suspect most fountain files will work with little to no modifications.

The NFountain console application supports converting between a .fountain file and HTML, plain text (with elements spaced properly), and PDF.  Support for all these formats should be considered experimental and incomplete.

NFountain Console App
=====================

Fountain.exe is a simple console app that actually acts as a pipe between plugins.  There are a handful of relevant commandline arguments necessary to make the program do something useful:

	-input filePath -> Path to the .fountain file to process.  If not supplied, input will be assumed to come from the STDIN.
	-output filePath -> Path of file to write to.  If not supplied, output will dump to SDTOUT.
	-writer name -> The name of the writer plugin to use.  Currently, there is pdf-writer, html-writer, and default-writer, which is the default and dumps a formatted, plain-text script.

	With each writer, different options become available.

	For pdf-writer, these options exist:

	-page-width -> Page width in inches
	-page-height -> Page height in inches
	-(left|top|right|bottom)-margin -> Margins in inches
	-boneyard -> Outputs boneyard sections in the PDF (they are hidden by default)
	-notes -> Outputs note sections in the PDF (also hidden by default)
	-sections -> Outputs heading sections (hidden by default, only supports 2 levels of heading sections)

Recent Updates
==============

Added a rough PDF library with just enough PDF format support to produce scripts.  Scripts outputted to PDF try to follow the screenplay format, but have some limitations (see below).

Fixed page-break rules to allow action & dialog to (CONTINUE) on the next page.  Needs testing.

Roadmap
=======

Items I hope to improve upon going forward:

- PDF Support
	- More testing of page-break rules, general clean up of page break/indention calculations and formatted text writing.
	- Support for FLATE compression on streams.  Currently PDFs are 100% pure plain text, which makes them easy to write, but much larger than they need be.

- Fountain compliance
	- More time spent making sure rules (in particular rules about overriding default parser behavior) are being followed and nothing is being missed.
	- More testing of formatting elements.  Add formatting support to more elements (currently only supported in dialog, action, and centered text blocks).


