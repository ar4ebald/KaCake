#include "bit_stream.h"

bit_stream::bit_stream(std::basic_istream<char> *stream)
{
	this->stream = stream; 
}

bit_stream::~bit_stream()
{
	delete this->stream;
}
