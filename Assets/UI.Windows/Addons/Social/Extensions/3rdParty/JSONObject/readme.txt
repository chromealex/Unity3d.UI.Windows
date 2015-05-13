==Author==
[mailto:schoen@defectivestudios.com Matt Schoen] of [http://www.defectivestudios.com Defective Studios]

==Download==
[[Media:JSONObject.zip|Download JSONObject.zip]]

= Intro =
I came across the need to send structured data to and from a server on one of my projects, and figured it would be worth my while to use JSON.  When I looked into the issue, I tried a few of the C# implementations listed on [http://json.org json.org], but found them to be too complicated.  So, I've written a very simple JSONObject class, which can be generically used to encode/decode data into a simple container.  This page assumes that you know what JSON is, and how it works.  It's rather simple, just go to json.org for a visual description of the encoding format.

As an aside, this class is pretty central to the [[AssetCloud]] content management system, from Defective Studios.

Update: The code has been updated to version 1.3 to incorporate user-submitted patches and bug reports.  This fixes issues dealing with whitespace in the format, as well as empty arrays and objects.

= Usage =
Users should not have to modify the JSONObject class themselves, and must follow the very simple proceedures outlined below:

Sample data (in JSON format): {"field1": 0.5,"field2": "sampletext","field3": [1,2,3]}

= Features =

*Decode JSON-formatted strings into a usable data structure
*Encode structured data into a JSON-formatted string
*Interoperable with System.Collections.Generic.Dictionary
*Copy to new JSONObject
*Merge with another JSONObject (experimental)
*Random access (with [int] or [string])
*ToString() returns JSON data with optional "pretty" flag to include newlines and tabs
*Switch between double and float for numeric storage depending on level of precision needed (and to ensure that numbers are parsed/stringified correctly)
*Supports Infinity and NaN values

It should be pretty obvious what this parser can and cannot do.  If anyone reading this is a JSON buff (is there such a thing?) please feel free to expand and modify the parser to be more compliant.  Currently I am using the .NET System.Convert namespace functions for parsing the data itself.  It parses strings and numbers, which was all that I needed of it, but unless the formatting is supported by System.Convert, it may not incorporate all proper JSON strings.  Also, having never written a JSON parser before, I don't doubt that I could improve the efficiency or correctness of the parser.  It serves my purpose, and hopefully will help you with your project!  Let me know if you make any improvements :)

== Encoding ==

Encoding is something of a hard-coded process.  This is because I have no idea what your data is!  It would be great if this were some sort of interface for taking an entire class and encoding it's number/string fields, but it's not.  I've come up with a few clever ways of using loops and/or recursive methods to cut down of the amount of code I have to write when I use this tool, but they're pretty project-specific.

Note: This section used to be WRONG!

<syntaxhighlight lang="csharp">
//Note: your data can only be numbers and strings.  This is not a solution for object serialization or anything like that.
JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
//number
j.AddField("field1", 0.5);
//string
j.AddField("field2", "sampletext");
//array
JSONObject arr = new JSONObject(JSONObject.Type.ARRAY);
j.AddField("field3", arr);

arr.Add(1);
arr.Add(2);
arr.Add(3);

string encodedString = j.print();
</syntaxhighlight>

NEW! The constructor, Add, and AddField functions now support a nested delegate structure.  This is useful if you need to create a nested JSONObject in a single line.  For example:
<syntaxhighlight lang="csharp">
DoRequest(URL, new JSONObject(delegate(JSONObject request) {
	request.AddField("sort", delegate(JSONObject sort) {
		sort.AddField("_timestamp", "desc");
	});
	request.AddField("query", new JSONObject(delegate(JSONObject query) {
		query.AddField("match_all", JSONObject.obj);
	}));
	request.AddField("fields", delegate(JSONObject fields) {
		fields.Add("_timestamp");
	});
}).ToString());
</syntaxhighlight>

== Decoding ==
Decoding is much simpler on the input end, and again, what you do with the JSONObject will vary on a per-project basis.  One of the more complicated way to extract the data is with a recursive function, as drafted below.  Calling the constructor with a properly formatted JSON string will return the root object (or array) containing all of its children, in one neat reference!  The data is in a public ArrayList called list, with a matching key list (called keys!) if the root is an Object.  If that's confusing, take a glance over the following code and the print() method in the JSONOBject class.  If there is an error in the JSON formatting (or if there's an error with my code!) the debug console will read "improper JSON formatting".


<syntaxhighlight lang="csharp">
string encodedString = "{\"field1\": 0.5,\"field2\": \"sampletext\",\"field3\": [1,2,3]}";
JSONObject j = new JSONObject(encodedString);
accessData(j);
//access data (and print it)
void accessData(JSONObject obj){
	switch(obj.type){
		case JSONObject.Type.OBJECT:
			for(int i = 0; i < obj.list.Count; i++){
				string key = (string)obj.keys[i];
				JSONObject j = (JSONObject)obj.list[i];
				Debug.Log(key);
				accessData(j);
			}
			break;
		case JSONObject.Type.ARRAY:
			foreach(JSONObject j in obj.list){
				accessData(j);
			}
			break;
		case JSONObject.Type.STRING:
			Debug.Log(obj.str);
			break;
		case JSONObject.Type.NUMBER:
			Debug.Log(obj.n);
			break;
		case JSONObject.Type.BOOL:
			Debug.Log(obj.b);
			break;
		case JSONObject.Type.NULL:
			Debug.Log("NULL");
			break;
		
	}
}
</syntaxhighlight>

NEW! Decoding now also supports a delegate format which will automatically check if a field exists before processing the data, providing an optional parameter for an OnFieldNotFound response.  For example:
<syntaxhighlight lang="csharp">
new JSONObject(data);
list.GetField("hits", delegate(JSONObject hits) {
	hits.GetField("hits", delegate(JSONObject hits2) {
		foreach (JSONObject gameSession in hits2.list) {
			Debug.Log(gameSession);
		}
	});
}, delegate(string name) {	//"name" will be equal to the name of the missing field.  In this case, "hits"
	Debug.LogWarning("no game sessions");
});
</syntaxhighlight>

===New! (O(n)) Random access!===
I've added a string and int [] index to the class, so you can now retrieve data as such (from above):
<syntaxhighlight lang="csharp">
JSONObject arr = obj["field3"];
Debug.log(arr[2].n);		//Should ouptut "3"
</syntaxhighlight>

--Code omitted from readme--

[[Category:C Sharp]]
[[Category:Scripts]]
[[Category:Utility]]
[[Category:JSON]]
