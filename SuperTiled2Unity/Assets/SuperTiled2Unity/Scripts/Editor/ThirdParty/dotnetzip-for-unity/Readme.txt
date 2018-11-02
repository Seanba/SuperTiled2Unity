Mon, 11 Jul 2011  20:10


Introducing the DotNetZip Library and Tools
-------------------------------------------

DotNetZip is the name of an open-source project that delivers a .NET
library for handling ZIP files, and some associated tools.

 - The library allows .NET or Silverlight programmers to build
   applications that read, create and modify ZIP files.

 - The tools are .NET programs that rely on the library, and can be used
   by anyone on any Windows machine to build or extract ZIP files.



Why DotNetZip?
---------------------------------

The Microsoft .NET Framework base class library lacks a good set of
built-in classes for creating and reading ZIP files, and Windows itself
lacks full-powered built-in ZIP tools.  DotNetZip fills those needs.

There are other ZIP libraries available, but some of them have licenses
that may be unfriendly, some of them are too hard to use or not powerful
enough, and some of them are too expensive (not free).  DotNetZip
provides one more alternative. The goals for this alternative:

 - easy to adopt - low cost (Free), and a friendly license
 - fully-featured
 - good performance - in speed and compression
 - easy to use.



DotNetZip background
---------------------------------

Many people seem to think, incorrectly, that the classes in the
System.IO.Compression namespace, like GZipStream or DeflateStream, can
create or read zip files. Not true.

The System.IO.Compression namespace, available starting with .NET v2.0
for the desktop Framework and v3.5 for the Compact Framework, includes
base class libraries supporting compression within streams - both the
Deflate and Gzip formats are supported. But these classes are not
directly useful for creating compressed ZIP archives.  GZIP is not
ZIP. Deflate is not ZIP.  The GZipStream in System.IO.Compression is
able to read and write GZIP streams, but that is not the same as reading
or writing a zip file.  Also, these classes deliver poor compression in
practice, especially with binary data, or previously-compressed data.


Yes, it is possible to read and write zip files, using the classes in
the .NET Framework.

  - You can do it with the System.IO.Packaging.ZipPackage class, added
    in .NET v3.0. Actually this class lets you create a package file,
    which is a zipfile with a particular internal structure. It includes
    a manifest and some other things.  But the interface is odd and
    confusing if all you want is a regular-old ZIP file.  Also, the
    classes in this namespace do not provide control for things like
    passwords, comments, AES encryption, ZIP64, Unicode, and so on.

  - You can also create and read zip files with the J# runtime. This
    also has ita drawbacks.  First, J# is going out of support, or may
    be out of support now.  Second, the J# runtime is huge, and you have
    to swallow the whole thing, even if all you want is zip file
    capability.  Also, the J# runtime is based on the java.util.zip
    classes from Java v1.4, dating from 1998.  The runtime hasn't been
    updated in years and still includes bugs in zip file handling. It
    lacks support for AES, for ZIP64, and Unicode.  It is not accessible
    from COM. Finally, the zip classes in the J# runtime are decidedly
    un-dotnet.  There's no support for events, or enumerators to let you
    do things like For Each in VB, or foreach in C#. The interface is
    clunky. It does not feel like a .NET class library, because it isn't
    a .NET class library.  So for all those reasons, J# isn't ideal.

  - You can also rely on P/Invoke to the shell32.dll, and the
    ShellClass. This works in a limited fashion. The drawbacks here: it
    isn't documented.  It isn't a very intuitive or powerful programming
    interface.  There are no events, so embedding it into a Winforms app
    with a progress bar would be difficult.  Again it lacks an easy way
    to use or access many ZIP features, like encryption or ZIP64 or
    self-extracting archives.  Also, the shell32.dll is designed for use
    within Windows Explorer, and presumes a user interface.  In fact in
    some cases, calling into this DLL to perform a ZIP extraction can
    display a dialog box, so it may not be suitable for use within
    server or "headless" applications.


There are other libraries out there than do zip files for .NET.  But
there are compromises with each one.  Some are commercial and expensive.
Some are slow.  Some are complicated to use.  Some of these options lack
features.  Some of them have more than one of these drawbacks.

DotNetZip provides another option.  It's a very simple-to-use class
library that provides good ZIP file support.  Using this library, you
can write .NET applications that read and write zip-format files,
including files with passwords, Unicode filenames, ZIP64, AES
encryption, and comments.  The library also supports self-extracting
archives.  It is well documented and provides good performance.

Though DotNetZip is implemented in .NET and was originally intended to
provide a managed-code library for ZIP files, you can now use it library
from any COM environment, including Javascript, VBScript, VB6, VBA, PHP,
Perl, and others.  Using DotNetZip, you could generate an AES-encrypted
zip file from within the code of a macro running in MS-Excel, for example.

DotNetZip works with applications running on PCs with Windows.  There is a
version of this library available for the .NET Compact Framework, too.

I have not tested DotNetZip for use with Mono, but I've heard reports
that people use the binary releases with Mono successfully, without
change.


License
--------

This software is open source. It is released under the Microsoft Public
License of October 2006.  The use of the "Microsoft Public License" does
not mean it is licensed by Microsoft.  See the License.txt file for
details.

DotNetZip is derived in part from ZLIB, the C-language library by Mark
Adler and Jean-loup Gailly .  See the License.ZLIB.txt file included in
the DotNetZip download for details.



What is DotNetZip?  and How is it packaged?
---------------------------------------------

DotNetZip is primarily a managed library for dealing with ZIP files.

It is packaged as a DLL that your application must reference:
Ionic.Zip.dll.  In the "developer's kit" package, there is
documentation, code examples, and debug versions of the DLL.

The ZIP library depends on a set of supporting classes for doing
compression and decompression; these are exposed in other namespaces.

The classes in the ZIP library reside in these namespaces:

   namespace     interesting classes
   ------------------------------------------------------------
   Ionic.Zip     ZipFile, ZipEntry, ZipOutputStream, and
                 ZipInputStream.

   Ionic.Zlib    DeflateStream, GZipStream, ZlibStream

   Ionic.BZip2   BZip2InputStream, BZip2OutputStream

   Ionic.Crc     CRC32


If you want only ZLIB (raw compression and decompression, RFC 1950,
1951, and 1952), the ZLIB classes are packaged independently, in
Ionic.Zlib.dll.  Likewise, if you want to do BZIP2 compression, outside
the scope of a zip file, you can use the Ionic.BZip2.dll assembly.

If you want ZIP, or both ZIP and ZLIB, then your application should
depend soly on Ionic.Zip.dll; this assembly includes a superset of the
classes in Ionic.Zlib.dll and Ionic.BZip2.dll.

For each DLL, there is a version for the regular .NET Framework and
another for the Compact Framework.

DotNetZip also includes command-line and GUI tools for manipulating zip
files; these can be helpful to developers when building applications
that create or manipulate zip files. They also can be helpful as
end-user tools.

There are other downloads for DotNetZip - the source package, the
runtime-only package (DLLs and no helpfile or tools), the
documentation-only package, etc.




Using the Zip Class Library: The Basics
----------------------------------------

The examples here provide just the basics.

There are many other examples available: some are included in the source
package, some in the class reference documentation in the help file, and
others on the web.  Those examples provide many illustrate how to read
and write zip files, taking advantage of all the various features of zip
files exposed by the library.  For a full set of examples, your best bet
is to see the documentation. Here's a basic primer:

The main type you will use to fiddle with zip files is the ZipFile
class. Full name: Ionic.Zip.ZipFile.  You use this to create, read, or
update zip files.  There is also a ZipOutputStream class, which offers a
Stream metaphor, for those who want it. You should choose one or the
other for your application.

The simplest way to create a ZIP file in C# looks like this:

      using(ZipFile zip= new ZipFile())
      {
        zip.AddFile(filename);
        zip.Save(NameOfZipFileTocreate);
      }


Or in VB.NET, like this:

     Using zip As ZipFile = New ZipFile
         zip.AddFile(filename)
         zip.Save("MyZipFile.zip")
     End Using


The using clause is important; don't leave it out.


The simplest way to Extract all the entries from a zipfile looks
like this:

      using (ZipFile zip = ZipFile.Read(NameOfExistingZipFile))
      {
        zip.ExtractAll(args[1]);
      }

But you could also do something like this:

      using (ZipFile zip = ZipFile.Read(NameOfExistingZipFile))
      {
        foreach (ZipEntry e in zip)
        {
          e.Extract();
        }
      }


Or in VB, extraction would be like this:
     Using zip As ZipFile = ZipFile.Read(NameOfExistingZipFile)
         zip.ExtractAll
     End Using

Or this:
     Using zip As ZipFile = ZipFile.Read(NameOfExistingZipFile)
        Dim e As ZipEntry
        For Each e In zip
            e.Extract
        Next
     End Using


That covers the basics.

Notice that a using clause is always employed. DOn't forget this.  Don't
leave it off.  If you don't understand what it is, don't just skip it.
It's important.

There are a number of other options for using the class library.  For
example, you can read zip archives from streams, or you can create
(write) zip archives to streams, or you can extract into streams.  You
can apply passwords for weak encryption.  You can specify a code page
for the filenames and metadata of entries in an archive.  You can rename
entries in archives, and you can add or remove entries from archives.
You can set up save and read progress events. You can do LINQ queries on
the Entries collection.  Check the documentation for complete
information, or use Visual Studio's intellisense to explore some of the
properties and methods on the ZipFile class.

Another type you will use is ZipEntry. This represents a single entry -
either a file or a directory - within a ZipFile.  To add an entry to a
zip file, you call one of the AddEntry (or AddFile) methods on the
ZipFile class.  You never directly instantiate a ZipEntry type.  The
AddEntry/AddFile returns a ZipEntry type; you can then modify the
properties of the entry within the zip file, using that object.

For example, the following code adds a file as an entry into a ZipFile,
then renames the entry within the zip file:

      using(ZipFile zip= new ZipFile())
      {
        ZipEntry e = zip.AddFile(filename);
        e.FileName = "RenamedFile.txt";
        zip.Save(NameOfZipFileTocreate);
      }

Extracting a zip file that was created in this way will produce a file
called "RenamedFile.txt", regardless of the name of the file originally
added to the ZipFile.


As an alternative to using ZipFile type to create a zip file, you can
use the ZipOutputStream type to create zip files .  To do so, wrap it
around a stream, and write to it.

      using (var fs = File.Create(filename))
      {
        using(var s = new ZipOutputStream(fs))
        {
          s.PutNextEntry("entry1.txt");
          byte[] buffer = Encoding.ASCII.GetBytes("This is the content for entry #1.");
          s.Write(buffer, 0, buffer.Length);
        }
      }

Unlike the ZipFile class, the ZipOutputStream class can only create zip
files. It cannot read or update zip files.

If you want to read zip files using a streaming metaphor, you can use
ZipInputStream.  Think of ZipInputStream and ZipOutputStream as
alternatives to using ZipFile to manipulate zip files. The former is for
reading zip files; the latter is for writing them.



About Directory Paths
---------------------------------

One important note: the ZipFile.AddXxx methods add the file or
directory you specify, including the directory.  In other words,
logic like this:
    ZipFile zip = new ZipFile();
    zip.AddFile("c:\\a\\b\\c\\Hello.doc");
    zip.Save();

...will produce a zip archive that contains a single entry, or file, and
that file is stored with the relative directory information.  When you
extract that file from the zip, either using this Zip library or winzip
or the built-in zip support in Windows, or some other package, all those
directories will be created, and the file will be written into that
directory hierarchy.  At extraction time, if you were to extract that
file into a directory like c:\documents, then resulting file would be
named c:\documents\a\b\c\Hello.doc .

This is by design.

If you don't want that directory information in your archive,
then you need to use the overload of the AddFile() method that
allows you to explicitly specify the directory used for the entry
within the archive:

    zip.AddFile("c:\\a\\b\\c\\Hello.doc", "files");
    zip.Save();

This will create an archive with an entry called "files\Hello.doc",
which contains the contents of the on-disk file located at
c:\a\b\c\Hello.doc .

If you extract that file into a directory e:\documents, then the
resulting file will be called e:\documents\files\Hello.doc .

If you want no directory at all, specify "" (the empty string).
Specifying null (Nothing in VB) will include all the directory hierarchy
in the filename, as in the orginal case.




Pre-requisites to run Applications that use DotNetZip
-----------------------------------------------------

To run desktop applications that depend on DotNetZip:
 .NET Framework 2.0 or later


To run smart device applications that depend on DotNetZip:
  .NET Compact Framework 2.0 or later





In more detail: The Zip Class Library
----------------------------------------------

The Zip class library is packaged as Ionic.Zip.DLL for the regular .NET
Framework and Ionic.Zip.CF.dll for the Compact Framework.  The Zip
library allows applications to create, read, and update zip files.

This library uses the DeflateStream class to compress file data,
and extends it to support reading and writing of the metadata -
the header, CRC, and other optional data - defined or required
by the zip format spec.

The key object in the class library is the ZipFile class.  Some of the
important methods on it:

      - AddItem - adds a file or a directory to a zip archive
      - AddDirectory - adds a directory to a zip archive
      - AddFile - adds a file to a zip archive
      - AddFiles - adds a set of files to a zip archive
      - Extract - extract a single element from a zip file
      - Read - static methods to read in an existing zipfile, for
               later extraction
      - Save - save a zipfile to disk

There is also a supporting class, called ZipEntry.  Applications can
enumerate the entries in a ZipFile, via ZipEntry.  There are other
supporting classes as well.  Typically, 80% of apps will use just the
ZipFile class, and will not need to directly interact with these other
classes. But they are there if you need them.

If you want to create or read zip files, the Ionic.Zip.DLL assembly is
the one you want.

When building applications that do zip stuff, you need to add a reference to
the Ionic.Zip.dll in Visual Studio, or specify Ionic.Zip.dll with the
/R flag on the CSC.exe or VB.exe compiler line.




In more detail: The Zlib Class Library
-----------------------------------------

The Zlib class library is packaged as Ionic.Zlib.DLL for the regular .NET
Framework and Ionic.Zlib.CF.dll for the Compact Framework.  The ZLIB
library does compression and decompression according to IETF RFC's 1950 (ZLIB),
1951 (Deflate), and 1952 (GZIP).

See http://www.ietf.org/rfc/rfc1950.txt
    http://www.ietf.org/rfc/rfc1951.txt
 and  http://www.ietf.org/rfc/rfc1952.txt


The key classes are:

  ZlibCodec - a class for Zlib (RFC1950/1951/1952) encoding and decoding.
        This low-level class does deflation and inflation on buffers.

  DeflateStream - patterned after the DeflateStream in
        System.IO.Compression, this class supports compression
        levels and other options.

  GZipStream - patterned after the GZipStream in
        System.IO.Compression, this class supports compression
        levels and other options.

  ZlibStream - similar to the GZipStream in
        System.IO.Compression, this class generates or consumes raw ZLIB
        streams.


If you want to simply compress (deflate) raw block or stream data, this
library is the thing you want.

When building applications that do zlib things, you need to add a reference to
the Ionic.Zlib.dll in Visual Studio, or specify Ionic.Zlib.dll with the
/R flag on the CSC.exe or VB.exe compiler line.

NB: If your application does both Zlib and Zip stuff, you need only add
a reference to Ionic.Zip.dll.  Ionic.Zip.dll includes all the capability
in Ionic.Zlib.dll.  Ionic.Zip.dll is a superset.



In more detail: The BZip2 Class Library
-----------------------------------------

The BZip2 class library is packaged as Ionic.BZip2.DLL for the regular .NET
Framework and Ionic.BZip2.CF.dll for the Compact Framework.  The BZip2
library does compression according to the bzip2 format created by
Julian Seward.
See http://en.wikipedia.org/wiki/Bzip2

NB: If your application does a combination of BZip2, Zlib and Zip stuff,
you need only add a reference to Ionic.Zip.dll.  Ionic.Zip.dll includes
all the capability in Ionic.Zlib.dll and Ionic.BZip2.dll.  Ionic.Zip.dll
is a superset.

If you try to link to more than one of these, you will get compiler
warnings about "duplicate types".



Namespace changes for DotNetZip
-----------------------------------------

The namespace for the DotNetZip classes is Ionic.Zip.
Classes are like:
  Ionic.Zip.ZipFile
  Ionic.Zip.ZipEntry
  Ionic.Zip.ZipException
  etc

(check the .chm file for the full list)

For the versions prior to v1.7, the namespace DotNetZip was Ionic.Utils.Zip.
The classes were like so:
  Ionic.Utils.Zip.ZipFile
  Ionic.Utils.Zip.ZipEntry
  etc

If you have code that depends on an older version of the library, with
classes in the Ionic.Utils.Zip namespace), a simple namespace
replacement will allow your code to compile against the new version of
the library.


In addition to the Zip capability, DotNetZip includes capability (new
for v1.7).  For Zlib, the classes are like this:
  Ionic.Zlib.DeflateStream
  Ionic.Zlib.ZlibStream
  Ionic.Zlib.ZlibCodec
  ...

(again, check the .chm file for the full list)

For v1.9.1.6, the CRC class moved from the Ionic.Zlib namespace to the
Ionic.Crc namespace.




Dependencies
---------------------------------

Originally, this library was designed to depend upon the built-in
System.IO.Compression.DeflateStream class for the compression.  This
proved to be less than satisfactory because the built-in compression
library did not support compression levels and also was not available on
.NET CF 2.0.

As of v1.7, the library includes a managed code version of zlib, the
library that produces RFC1950 and RFC1951 compressed streams.  Within
that version of zlib, there is also a DeflateStream class which is
similar to the built-in System.IO.Compression.DeflateStream, but more
flexible, and often more effective as well.

As a result, this library depends only on the .NET Framework v2.0, or the
.NET Compact Framework v2.0.




The Documentation
--------------------------------------------

There is a single set of developer reference documentation for all of
the DotNetZip library features, including Zip and Zlib stuff.  It is
packaged in two ways: As a .chm file, and as a Help Viewer 1.0 resource.
The latter is the new format suitable for viewing within VS2010.

If you only use the Zlib stuff, then you should focus on the doc in the
Ionic.Zlib namespace.  Likewise BZip2.  If you are building apps for
mobile devices running the Compact Framework, then ignore the pieces
that deal with SaveSelfExtractor() and AES.

Consult the help file for more specifics here.

In some cases, upon opening the .chm file for DotNetZip, the help
items tree loads, but the contents are empty. You may see an error:
"This program cannot display the webpage."  or, "Address is invalid."
If this happens, it's likely that you've encountered a problem with Windows
protection of files downloaded from less trusted locations. To work around
this, within Windows Explorer, right-click on the CHM file, select properties,
and Unblock it, using the button in lower part of properties window.

The help is also packaged in a format that you can integrate into Visual
Studio 2008, or Visual Studio 2010.  VS2008 requires MS Help 2.0, while
VS2010 requires a different, newer format, sometimes called MS Help 3,
and sometimes (confusingly) called "MS Help Viewer 1.0 format".

The DotNetZip "devkit" download includes help in all these formats.



The Zip Format
---------------------------------
The zip format is described by PKWare, at
  http://www.pkware.com/business_and_developers/developer/popups/appnote.txt

Every valid zipfile conforms to this specification.  For example, the
spec says that for each compressed file contained in the zip archive,
the zipfile contains a byte array of compressed data.  (The byte array
is something the DeflateStream class can produce directly.)  But the
zipfile also contains header and "directory" information - you might
call this "metadata".  In other words, the zipfile must contain a list
of all the compressed files in the archive. The zipfile also contains
CRC checksums, and can also contain comments, and other optional
attributes for each file.  These are things the DeflateStream class -
either the one included in the .NET Framework Class Library, or the one
embedded in this library - does not read or write.

Managing the metadata in a zip file is most of what DotNetZip does.


Which DLL to use?
--------------------------------
The binary releases of DotNetZip include multiple distinct DLLs or
assemblies.  Which one should you use?

The likely answer is:  use Ionic.Zip.dll.

That's the mainstream library, the full library, and it includes all the
capability.  If you have particular requirements, like you want a
smaller library, or you want to exclude the Self-Extracting stuff, or
you only want the ZLIB capability, then you may want to choose a
different assembly.

Here's a summary of the options.


Usage scenario                                 Reference this DLL
------------------------------------------------------------------
reading or writing Zip files                   Ionic.Zip.dll

raw block or stream compression, ZLIB, GZIP,   Ionic.Zlib.dll
   or DEFLATE

raw block or stream compression, BZIP2         Ionic.BZip2.dll

both raw compression as well as reading        Ionic.Zip.dll
   or writing Zip files

reading or writing Zip files on Compact        Ionic.Zip.CF.dll
     Framework

raw compression on Compact Framework           Ionic.Zlib.CF.dll
                                                   -and/or-
                                               Ionic.BZip2.CF.dll

both raw compression as well as reading        Ionic.Zip.CF.dll
   or writing Zip files on CF

reading or writing Zip files, using desktop    Ionic.Zip.Reduced.dll
  .NET framework but never creating a
  self-extracting archive


Never reference both Ionic.Zlib.dll and Ionic.Zip.dll, or both
Ionic.BZip2.dll and Ionic.Zip.dll in the same application.  If your
application does both Zlib and Zip stuff, you need only add a reference
to Ionic.Zip.dll.  Ionic.Zip.dll includes all the capability in
Ionic.Zlib.dll and Ionic.BZip2.dll You always need to reference only a
single Ionic DLL, regardless whether you use Zlib or BZip2 or Zip or
some combination.




Self-Extracting Archive support
--------------------------------

The Self-Extracting Archive (SFX) support in the library allows you to
create a self-extracting zip archive.  An SFX is both a standard EXE
file *and* a ZIP file.  The exe contains boilerplate program logic to
unzip the embedded zip file.  When the user executes the SFX runs, the
boilerplate application logic just reads the zip content and
then unzips itself. You can open an SFX in WinZip and other zip tools,
as well, if you want to view it.

Running the SFX (unpacking from the SFX) requires the .NET Framework
installed on the machine, but does not require the DotNetZip library.

There are two versions of the SFX - one that presents a GUI form, and
another that runs as a console (command line) application.

NB: Creation of SFX is not supported in the Compact Framework version of
the library.

Also, there is no way, currently, to produce an SFX file that can run on
the .NET Compact Framework.




The Reduced ZIP library
--------------------------------

The full DotNetZip library is currently about 400k in size.  The SFX
(Self-Extracting Archive) support is responsible for more than half the
total size of the library.  Some deployments may wish to omit the SFX
support in order to get a smaller DLL. For that you can rely on the
Ionic.Zip.Reduced.dll.  It provides everything the normal library does,
except the SaveSelfExtractor() method on the ZipFile class.

For size comparisons...these approximate figures are for v1.9.1.6 of the
library:


Desktop Framework:

  assembly              ~size   comment
  -------------------------------------------------------
  Ionic.Zlib.dll         100k   {Deflate,GZip,Zlib}Stream and ZlibCodec

  Ionic.BZip2.dll         57k   BZip2{Input,Output}Stream

  Ionic.Zip.dll          460k   includes ZLIB and BZIP2 compression,
                                SFX, selector logic, WinZIP AES encryption,
                                and the ComHelper class

  Ionic.Zip.Reduced.dll  250k   includes everything in the main ZIP
                                library except SFX. (ability to save
                                Self-extracting archives)



Compact Framework:

  assembly              ~size   comment
  -------------------------------------------------------
  Ionic.Zlib.CF.dll       74k   {Deflate,GZip,Zlib}Stream and ZlibCodec

  Ionic.BZip2.CF.dll      36k   BZip2{Input,Output}Stream

  Ionic.Zip.CF.dll       204k   includes ZLIB and BZIP2 compression, but
                                no SFX.


Silverlight:

  assembly              ~size   comment
  -------------------------------------------------------
  Ionic.Zlib.dll          80k   {Deflate,GZip,Zlib}Stream and ZlibCodec

  Ionic.BZip2.dll         41k   BZip2{Input,Output}Stream

  Ionic.Zip.dll          226k   includes ZLIB and BZIP2 compression, and
                                the selector logic.  No SFX, no WinZIP AES.








Support
--------------------------------------------

There is no official support for this library.  I try to make a good
effort to answer questions and monitor the work items raised on the
project portal at:

    http://DotNetZip.codeplex.com.





About Intellectual Property
---------------------------------

I am no lawyer, but before using this library in your app, it
may be worth reviewing the various licenses.

The specification for the zip format, which PKWARE owns, includes a
paragraph that reads:

  PKWARE is committed to the interoperability and advancement of the
  .ZIP format.  PKWARE offers a free license for certain technological
  aspects described above under certain restrictions and conditions.
  However, the use or implementation in a product of certain technological
  aspects set forth in the current APPNOTE, including those with regard to
  strong encryption or patching, requires a license from PKWARE.  Please
  contact PKWARE with regard to acquiring a license.

Contact pkware at:  zipformat@pkware.com

This library does not do strong encryption as described by PKWare, nor
does it do patching.  But again... I am no lawyer.


This library uses a ZLIB implementation that is based on a conversion of
the jzlib project http://www.jcraft.com/jzlib/.  The license and
disclaimer required by the jzlib source license is referenced in the
relevant source files of DotNetZip, specifically in the sources for the
Zlib module.

This library uses a BZip2 implementation that is based on a conversion
of the bzip2 implementation in the Apache Commons compression library.
The Apache license is referenced in the relevant source files of
DotNetZip, specifically in the sources for the BZip2 module.




Limitations
---------------------------------

There are a few limitations to this library:

 It does not do strong encryption.

 The GUI tool for creating zips is functional but basic. This isn't a limitation
 of the library per se.

 ...and, I'm sure, many others

But it is a good basic library for reading and writing zipfiles
in .NET applications.




Building the Library
============================================

This section is mostly interesting to developers who will work on or
view the source code of DotNetZip, to extend or re-purpose it.  If you
only plan to use DotNetZip in applications of your own, you probably
don't need to concern yourself with the information that follows.





Pre-requisites to build DotNetZip
---------------------------------

.NET Framework 4.0 SDK or later
  -or-
Visual Studio 2010 or later

  -and-

ILMerge - a tool from Microsoft that combines
multiple managed assemblies into a single DLL or image.  It is in
similar in some respects to the lib tool in C toolkits.

You can get it here:
  http://www.microsoft.com/downloads/details.aspx?familyid=22914587-b4ad-4eae-87cf-b14ae6a939b0&displaylang=en





Building DotNetZip with the .NET SDK
-------------------------------------

To build the library using the .NET Framework SDK v3.5,

1. extract the contents of the source zip into a new directory.

2. be sure the .NET 2.0 SDK, .NET 3.5 runtime, and .NET 2.0 runtime
   directories are on your path.  These are typically

     C:\Program Files\Microsoft.NET\SDK\v2.0\bin
     c:\windows\Microsoft.NET\Framework\v3.5
       and
     c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727

   The .NET 3.5 runtime is necessary because building DotNetZip requires
   the csc.exe compiler from NET 3.5. (Using DotNetZip from within C#
   requires the v2.0 csc compiler.)


3. Modify the .csproj files in Zip and ZLIB and BZip2 to eliminate
   mention of the Ionic.pfx and Ionic.snk files.

   The various DLLs (Zip Partial, ZLIB, etc.) are signed with my private
   key.  You will want to remove the mention of the private key in the
   project files. I cannot distribute my private key, so don't ask me!
   That would be silly.  So you have to modify the project in order to
   build without the key.


4. open a CMD prompt and CD to the DotNetZip directory.


5. msbuild

   Be sure you are using the .NET 3.5 version of MSBuild.
   This builds the "Debug" version of the library.  To build the
   "Release" version, do this:

   msbuild /t:Release


6. to clean and rebuild, do
   msbuild /t:clean
   msbuild


7. There are two setup directories, which contain the projects necessary
   to build the MSI file.  Unfortunately msbuild does not include
   support for building setup projects (vdproj).  You need Visual Studio
   to build the setup directories.

   I am in the process of converting these from .vdproj to .wixproj, so
   they can be built from the command line using msbuild. .




Building DotNetZip with Visual Studio
-------------------------------------

To build DotNetZip using Visual Studio 2010,

1. Open the DotNetZip.sln file in VS2010.

2. If necessary, Remove the dependencies on Ionic.pfx and Ionic.snk.

   (References to these will have already been removed from the zipped
   source distributions, but if you get your source from the TFS server,
   then you will have to remove references to the keyfiles manually)

   The various DLLs (Zip, ZLIB, etc.) are signed with my (Dino
   Chiesa's) private key.  I do not distribute that key for anyone
   else's use.  If you build the DotNetZip library from source, You will
   want to remove the mention of the private key in the project files. I
   will not distribute my private key, that would be silly.  So don't
   ask me!

3. Press F6 to build everything.




The Project Structure and Build approach
----------------------------------------------------

The function here is grouped into three basic sets: Zip,
ZLIB/Deflate/GZIP, and BZip2.  The Zip group is a superset of the ZLIB
and BZIP2 groups.

Each group of functionality is packaged into various assemblies, one
assembly per "platform".  The platforms supported are: .NET (Desktop),
Compact Framework 2.0, and Silverlight.

There is also a special "Zip Reduced" library, available only on the
Desktop platform; it is a reduced-function version of the regular
Desktop Framework zip library. It provides an option of using a smaller
library for those zip-handling applications that don't produce
Self-extracting archives.

In a previous guise, DotNetZip relied on the ILMerge tool to combine
distinct DLLs into a single package.  This is no longer the case.

Because the ZIP projects include the ZLIB and BZIP2 function, the
appropriate source modules for the ZLIB and Bzip2 are "linked" into each
of the ZIP projects (Desktop, CF, and Silverlight).




Regarding the missing Ionic.pfx and Ionic.snk files
-------------------------------------------------------------------------

The binary DLLs shipped in the codeplex project are signed by me, Dino
Chiesa.  This provides a "strong name" for the assembly, which itself
provides some assurance as to the integrity of the library, and also
allows it to be run within restricted sites, like apps running inside
web hosters.

For more on strong names, see this article:
http://msdn.microsoft.com/en-gb/magazine/cc163583.aspx

Signing is done automatically at build time in the Visual Studio project or in
the msbuild build.  There
is a .pfx file that holds the crypto stuff for signing the assembly, and
that pfx file is itself protected by a password. There is also an
Ionic.snk file which is referenced by the project, but which I do not
distribute.

People opening the project ask me: what's the password to this .pfx
file?  Where's the .snk file?

Here's the problem; those files contain my private key. if I give
everyone the password to the PFX file or the .snk file, then anyone can
go and build a modified Ionic.Zip.dll, and sign it with my key, and
apply any version number they like.  This means there could be multiple
distinct assemblies with the same signature.  This is obviously not
good.

Since I don't release the ability to sign DLLs with my key, the DLL
signed with my key is guaranteed to be produced by me only, which is in
fact the exact intent of code signing in .NET.

If anyone wants to modify the project and re-compile it, they have a
couple options:

  - sign the assembly themselves, using their own key.
  - produce a modified, unsigned assembly

In either case it is not the same as the assembly I am shipping,
therefore it should not be signed with the same key.

All clear?

As for those options above, here is some more detail:

  1. If you want a strong-named assembly, then create your own PFX file
     and .snk file and modify the appropriate projects to use those new
     files.

  2. If you don't need a strong-named assembly, then remove all the
     signing from the various projects.

In either case, you will need to modify the "Zip" and "Zip CF DLL"
projects, the BZip and BZip CF projects, and the "Zlib" and "Zlib CF"
projects.




Building the Documentation
--------------------------------------------

The documentation files are built using the Sandcastle Helpfile Builder
tool, also available on CodePlex at http://www.codeplex.com/SHFB .  It
is built from in-code xml documentation, augmented with some additional
out-of-band html documentation.

If you want to build the help files yourself, you will need to have
Sandcastle from May 2008 (or later, I guess), and SHFB, from February
2009.  Both are free tools available from http://codeplex.com .  I think
you can get a package download of both of these by installing v1.9.3.0
of SHFB .

The helpfile projects are:

  HtmlHelp1.shfbproj - to build the .chm file
  MSHelp2.shfbproj - to build the MS Help 2.0 content
  HelpViewer.shfbproj - to build the MS Help Viewer 1.0 content

  (The MSHelp2 project is broken at the moment.)

To build the documentation in any of these formats, first build the "zip
Full DLL" project in the source (Ionic.Zip.dll), then run:

  msbuild   HtmlHelp1.shfbproj

    -or-

  msbuild   HelpViewer.shfbproj


The Help Viewer 1.0 content can be viewed in the help viewer that is
integrated into VS 2010, or in an alternative viewer, such as
H3Viewer.exe.  See http://mshcmigrate.helpmvp.com/viewer .




Examples
--------------------------------------------

The source solution also includes a number of example applications
showing how to use the DotNetZip library and all its features - creating
ZIPs, using Unicode, passwords, comments, streams, and so on.  Most of
these will be built when you build the solution.  Some of them do not -
you will need to build them independently.



Tests
--------------------------------------------

There are two source projects in the VS Solution that contain Unit
Tests: one for the zlib library, one for the bzip2 library, and another
for the Zip library.  If you develop any new tests for DotNetZip, I'd be
glad to look at them.






Origins
============================================

This library is mostly original code.

There is a GPL-licensed library called SharpZipLib that writes zip
files, it can be found at
http://www.sharpdevelop.net/OpenSource/SharpZipLib/Default.aspx

This library is not based on SharpZipLib.

I think there may be a Zip library shipped as part of the Mono
project.  This library is also not based on that.

Now that the Java class library is open source, there is at least one
open-source Java implementation for zip.  This implementation is not
based on a port of Sun's JDK code.

There is a zlib.net project from ComponentAce.com.  This library is not
based on that code.

This library is all new code, written by me, with these exceptions:

 -  the CRC32 class - see above for credit.
 -  the zlib library - see above for credit.
 -  the bzip2 compressor - see above for credit.



You can Donate
--------------------------------

If you think this library is useful, consider donating to my chosen
cause: The Boys and Girls Club of Southwestern Pennsylvania, in the USA.
(In the past I accepted donations for the Boys and Girls Club of
Washington State, also in the USA.  I've moved, and so changed the
charity.)  I am accepting donations on my paypal account.

http://cheeso.members.winisp.net/DotNetZipDonate.aspx

Thanks.

