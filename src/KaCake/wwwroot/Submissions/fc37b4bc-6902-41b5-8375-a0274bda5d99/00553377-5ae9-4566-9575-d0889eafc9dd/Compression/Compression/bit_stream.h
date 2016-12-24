#pragma once
#include <iostream>

class bit_stream
{
private:
	std::basic_iostream<char>* stream;

public:
	bit_stream(std::basic_istream<char> *stream);
	~bit_stream();
};

