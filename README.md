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

Recent Updates
==============

Added a rough PDF library with just enough PDF format support to produce scripts.  Scripts outputted to PDF try to follow the screenplay format, but have some limitations (see below).

Roadmap
=======

Items I hope to improve upon going forward:

- PDF Support
	- Support proper page break rules (currently, a dialog block either fits on a page or doesn't, should be allowed to break multiline dialog at an appropriate place, optionally including (CONT) headers).
	- Support for FLATE compression on streams.  Currently PDFs are 100% pure plain text, which makes them easy to write, but much larger than they need be.

- Fountain compliance
	- More time spent making sure rules (in particular rules about overriding default parser behavior) are being followed and nothing is being missed.
	- More testing of formatting elements.  Add formatting support to more elements (currently only supported in dialog, action, and centered text blocks).


