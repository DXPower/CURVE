Your events all go in the square brackets from "events"
THEY NEED TO BE FOLLOW BY COMMAS (except for the last one)
All timing is in seconds


"timing":0.0 < This sets the time (doesn't have to be a decimal, accuracy is only to 1 decimal point
"type":"TYPEHERE" < This can be either SetAlienPos, MoveAlien, PlaySound, ShowText, CameraShake
	SetAlienPos will set the position instantly. It's data is "pos" followed by two numbers (decimal). 
		The numbers are the coords in percentage (.5 .5 is the center of the screen. 1 1 is the top right corner)
	MoveAlien has "pos" is the same as above
		duration is how long it takes for the alien to move there
	PlaySound will play a sound (you need the names you can do that later)
	ShowText will show a caption on the screen
		"text" is what to show
		"duration" is how long to show it
	CameraShake will shake the camera
		amount and decay are kinda arbitrary.
	

All the different properties NEED to be separated by SEMI COLONS. Any semicolon will be parsed and separates the data elements.

All the cutscenes need to be named 0.json, 1.json, 2.json, etc. 
They all are played in order. 0 comes after the tutorial, 1 comes after level 1, etc.
