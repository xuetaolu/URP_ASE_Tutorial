function vector getColor(string str) 
{
	
	string tokens[] = split(str, ",");
	vector color = set( 
		atof(tokens[0]), 
		atof(tokens[1]), 
		atof(tokens[2])
		);

	color /= 255.0;
	
	return color;
}