Some of the Unity unit tests use features of the Moq mock object library.
We cannot include these libraries as part of the Unity download. To run
the unit tests, please download the binaries for Moq 4.0 from the
project site at:

http://code.google.com/p/moq/

(current version as of this writing is 4.0.10827)

and unpack the binaries and place them in this directory. Both the desktop (.NET 3.5)
and Silverlight binaries can be placed here. You will then be able to compile
and run the unit tests.

