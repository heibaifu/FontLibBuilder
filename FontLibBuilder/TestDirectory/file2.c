#include "stdio.h"		// Preprocessor header should not be included

int main()
{
    #define mystr "asdfgh"	// Preprocessor definition should be included
    string str1 = "123456";	// Should be included
    string str2 = "abcdef";	// Should be included
    // string str3 = "uvwxyz";	// Should not be included
    // The following 4 strings should be included with escape character recoginsed.
    string str4 = "Text with \"Escape Character\"";
    string str5 = "Text with // comment mark";
    string str6 = "Text with # precompiler mark";
    string str7 = "Text with \"Escape Character and // comment mark\"";
	string str8 = "Multi text in single line"; string str9 = "Second Text in line";
	string str10 = "Text with // comment mark in single line"; string str11 = "Text with # precompiler mark";
	string str10 = "Text with // comment mark in single line"; string str11 = "Text with # precompiler mark";
}
}
