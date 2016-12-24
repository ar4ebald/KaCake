// КДЗ-1 по дисциплине Алгоритмы и структуры данных
// Булатов Артур Ринатович, БПИ154(1), 05.12.2016
// MS Visual Studio 2015
// Вроде как все сделал

#include <iostream>
#include <stdio.h>
#include <functional>
#include <map>
#include <queue>
#include <fstream>
#include <cmath>
#include <cstring>

typedef unsigned char BYTE;

static const int BITS_IN_BYTE = 8;
static const char32_t EOF_CHAR = 0x10FFFF;

// Operations count
static uint64_t ops = 0;

// Represents an input stream of bits
class bit_istream
{
	// Inner byte stream
	std::basic_istream<BYTE>* stream;
	// Last read byte
	BYTE buffer = 0;
	// Index of bit in buffer
	int bit_index = -1;

public:
	// Creates new bit_istream using specified path to file
	bit_istream(const char* file)
	{
		this->stream = new std::basic_ifstream<BYTE>(file, std::ios_base::binary);
	}
	~bit_istream()
	{
		// Frees allocated stream
		delete this->stream;
	}

	bool next_byte_avaliable() const
	{
		// return if next byte isn't available
		return this->stream->peek() != std::char_traits<BYTE>::eof();
	}

	// Restarts current bit stream to beginning of inner stream
	void seek_to_start() const
	{
		this->stream->seekg(0);
	}

	// Reads next bit from stream
	int read_bit()
	{
		// If bit index is less than minimal bit index (0)
		if (this->bit_index == -1)
		{
			// Then set it to index of first bit
			this->bit_index = BITS_IN_BYTE - 1;
			// And read buffer byte from stream
			this->buffer = stream->get();
		}
		// Returns bit of buffer at bit_index position
		return this->buffer >> this->bit_index-- & 1;
	}

	// Reads next 8 bits as 1 byte
	int read_byte()
	{
		// Composes a resulting byte from current and next bytes of a stream
		// Left part of the result is left bits of buffer
		int result = this->buffer << (BITS_IN_BYTE - 1 - this->bit_index);
		// And right part of the result is first bits of next byte
		this->buffer = stream->get();
		result |= this->buffer >> (this->bit_index + 1);
		// Apply byte mask to result to truncate bits other than last 8
		return result & 0xFF;
	}
};

// Represents an output stream of bits
class bit_ostream
{
	// Inner byte stream
	std::basic_ostream<BYTE>* stream;
	// Last unwritten byte
	BYTE buffer = 0;
	// Buffer bit index
	int bit_index = BITS_IN_BYTE - 1;
public:
	// Creates a new stream for writing for specified file
	bit_ostream(const char* file)
	{
		this->stream = new std::basic_ofstream<BYTE>(file, std::ios_base::binary);
	}
	~bit_ostream()
	{
		// Writes last byte buffer if there is any bits written
		if (this->bit_index != BITS_IN_BYTE - 1)
		{
			this->stream->put(this->buffer);
		}
		// Frees (and closes) allocated stream
		delete this->stream;
	}
	// Writes a single bit to stream
	void write_bit(int bit)
	{
		if (bit) // Adds specified bit to buffer
		{
			this->buffer |= 1 << this->bit_index;
		}
		// If buffer is filled, then write it to file
		if (this->bit_index == 0)
		{
			this->stream->put(this->buffer);
			this->buffer = 0;
			this->bit_index = BITS_IN_BYTE - 1;
		}
		else
		{
			// Or else decrement bit index
			this->bit_index--;
		}
	}
	// Writes 8 bits from specified byte
	void write_byte(int byte)
	{
		// Fill buffer rightmost bits of buffer with leftmost bits of specified byte
		this->buffer |= byte >> (BITS_IN_BYTE - 1 - this->bit_index);
		// And write it to file
		this->stream->put(this->buffer);
		// And set buffers leftmost bits as rightmost bits of specified byte
		this->buffer = (byte << (this->bit_index + 1)) & 0xFF;
	}
};

// Reads next UTF-8 character from specified stream
// And convert it to unicode index (like UTF-32)
// (see https://en.wikipedia.org/wiki/UTF-8#Description)
char32_t read_utf8_character(bit_istream& stream)
{
	char32_t character;
	BYTE code_point;

	code_point = stream.read_byte(); // first byte

	if ((code_point & 0b10000000) == 0b00000000) // 0xxxxxxx
	{
		return code_point;
	}
	else if ((code_point & 0b11100000) == 0b11000000) // 110xxxxx
	{
		character = code_point & 0b00011111;

		code_point = stream.read_byte(); // second byte
		character <<= 6;
		character |= code_point & 0b00111111;

		return character;
	}
	else if ((code_point & 0b11110000) == 0b11100000) // 1110xxxx
	{
		character = code_point & 0b00001111;

		code_point = stream.read_byte(); // second byte
		character <<= 6;
		character |= code_point & 0b00111111;

		code_point = stream.read_byte(); // third byte
		character <<= 6;
		character |= code_point & 0b00111111;

		return character;
	}
	else if ((code_point & 0b11111000) == 0b11110000) // 11110xxx
	{
		character = code_point & 0b00000111;

		code_point = stream.read_byte(); // second byte
		character <<= 6;
		character |= code_point & 0b00111111;

		code_point = stream.read_byte(); // third byte
		character <<= 6;
		character |= code_point & 0b00111111;

		code_point = stream.read_byte(); // fourth byte
		character <<= 6;
		character |= code_point & 0b00111111;

		return character;
	}
	else
	{
		throw std::ios_base::failure("Unknown UTF-8 sequence");
	}
}

// Converts UTF-32 character to UTF-8 and write it to stream
// (see https://en.wikipedia.org/wiki/UTF-8#Description)
void write_utf8_character(const char32_t& character, bit_ostream& stream)
{
	if (character < 0x080) // U+0000 - U+007F
	{
		stream.write_byte(character);
	}
	else if (character < 0x0800) //	U+0080 - U+07FF
	{
		stream.write_byte(character >> 6 & 0b00011111 | 0b11000000);
		stream.write_byte(character >> 0 & 0b00111111 | 0b10000000);
	}
	else if (character < 0x10000) // U+0800 - U+FFFF
	{
		stream.write_byte(character >> 12 & 0b00001111 | 0b11100000);
		stream.write_byte(character >> 6 & 0b00111111 | 0b10000000);
		stream.write_byte(character >> 0 & 0b00111111 | 0b10000000);
	}
	else if (character < 0x110000) // U+10000 - U+10FFFF
	{
		stream.write_byte(character >> 18 & 0b00000111 | 0b11110000);
		stream.write_byte(character >> 12 & 0b00111111 | 0b10000000);
		stream.write_byte(character >> 6 & 0b00111111 | 0b10000000);
		stream.write_byte(character >> 0 & 0b00111111 | 0b10000000);
	}
	else
	{
		throw std::ios_base::failure("Invalid Unicode character");
	}
}

// Represents prefix code tree node
class TreeNode
{
public:
	// Represents lack of value of node == node is a leaf
	static const char32_t NO_VALUE = 0xEFFFFFFF;

	// Left and right subrees
	TreeNode* left;
	TreeNode* right;

	// Frequency of current character/node
	uint64_t frequency;
	// Unicode code-point of leaf or NO_VALUE
	char32_t value;

	// Creates a new node with specified subtrees
	TreeNode(TreeNode* left, TreeNode* right) : left(left), right(right)
	{
		this->frequency = left->frequency + right->frequency;
		this->value = NO_VALUE;
		ops += 3;
	}
	// Creates a new leaf
	TreeNode(char32_t value, uint64_t frequency) : left(nullptr), right(nullptr)
	{
		this->frequency = frequency;
		this->value = value;
		ops += 2;
	}
	~TreeNode()
	{
		ops += 1;
		if (!is_leaf())
		{
			// Free node's subtrees
			delete left;
			delete right;
		}
	}
	// Gets if node is a leaf
	bool is_leaf() const { ops++; return value != NO_VALUE; }

	// Fills code table with characters codes
	void fill_code_table(std::map<char32_t, std::vector<int>>* codeTable, std::vector<int>& stack)
	{
		// Add current stack to codeTable at leaf's value if we're in leaf
		if (this->is_leaf())
		{
			ops += 2;
			codeTable->insert(std::make_pair(this->value, stack));
		}
		else
		{
			// Or push 0 to the stack and go to the left subtree
			ops += 2;
			stack.push_back(0);
			this->left->fill_code_table(codeTable, stack);
			// Then reset it with 1 and go to the right subtree
			ops += 2;
			*(stack.end() - 1) = 1;
			this->right->fill_code_table(codeTable, stack);
			// And remove 1 from the stack
			ops += 2;
			stack.pop_back();
		}
	}

	// Writes the tree to output stream
	void write_tree(bit_ostream& stream) const
	{
		// If we're in a leaf
		if (this->is_leaf())
		{
			// then write 1 and leaf's value
			ops += 2;
			stream.write_bit(1);
			write_utf8_character(this->value, stream);
		}
		else
		{
			// Or write 0 and write node's subtrees
			ops += 3;
			stream.write_bit(0);
			this->left->write_tree(stream);
			this->right->write_tree(stream);
		}
	}

	// Reads a character by it's code bits from input stream
	char32_t& read_code(bit_istream& stream)
	{
		// If we're got to the leaf, then return it's value
		if (this->is_leaf())
		{
			ops += 1;
			return this->value;
		}
		else if (stream.read_bit()) // or 1 is read go to the right subtree
		{
			ops += 2;
			return right->read_code(stream);
		}
		else // otherwise got to the left subtree
		{
			ops += 2;
			return left->read_code(stream);
		}
	}

	// Reads a tree from input stream
	static TreeNode* read_tree(bit_istream& stream)
	{
		// If 1 has been read
		if (stream.read_bit())
		{
			ops += 2;
			// Then create new leaf and read it's character
			return new TreeNode(read_utf8_character(stream), 0);
		}
		else
		{
			ops += 3;
			// Otherwise 0 has been read, so recursevly read left and right subtrees
			TreeNode* left = read_tree(stream);
			TreeNode* right = read_tree(stream);
			// Then create and return their parrent
			return new TreeNode(left, right);
		}
	}
};

// Reads characters frequencies from input stream
std::map<char32_t, uint64_t>* read_frequencies(bit_istream& in)
{
	// Allocate new frequencies table 
	// Where key is character and value is character's occurrences count
	std::map<char32_t, uint64_t>* frequencies = new std::map<char32_t, uint64_t>();
	// Insert finishing character to table
	frequencies->insert(std::make_pair(EOF_CHAR, 1));
	ops += 2;

	// Read all characters from input stream
	// and increment corresponding occurrences count
	while (in.next_byte_avaliable())
	{
		char32_t symbol = read_utf8_character(in);
		std::map<char32_t, uint64_t>::iterator it = frequencies->insert(std::make_pair(symbol, 0)).first;
		it->second++;
		ops += 4;
	}

	return frequencies;
}

// Creates a Huffman code tree from frequencies dictionary
TreeNode* create_huffman_code_tree(std::map<char32_t, uint64_t>* frequencies)
{
	// Create a nodes table where key is node's frequency
	// And is pointer to the node's 
	std::multimap<uint64_t, TreeNode*> nodes{};

	// Seed nodes from specified frequencies table
	for (std::map<char32_t, uint64_t>::iterator it = frequencies->begin(); it != frequencies->end(); ++it)
	{
		nodes.insert(std::make_pair(it->second, new TreeNode(it->first, it->second)));
		ops += 4;
	}

	// While there is more than 1 element is nodes table
	while (nodes.size() > 1)
	{
		// Take to nodes with least frequencies
		// and remove then from table
		std::multimap<uint64_t, TreeNode*>::iterator it = nodes.begin();
		TreeNode* left = it->second;
		nodes.erase(it);
		it = nodes.begin();
		TreeNode* right = it->second;
		nodes.erase(it);
		// Then create and add to table a parrent of these nodes
		// with frequency equal to sum of their frequencies
		TreeNode* parrent = new TreeNode(left, right);
		nodes.insert(std::make_pair(parrent->frequency, parrent));
		ops += 11;
	}
	ops += 4;

	return nodes.begin()->second;
}

// Splits linked list of nodes in two almost equal by frequency parts
TreeNode* split_list(TreeNode* start, int64_t total)
{
	// If linked list contains only one element, then return it
	ops += 1;
	if (start->right == nullptr)
	{
		return start;
	}

	int64_t left_sum = 0; // sum of left part of partition
	TreeNode* right_start_prev = nullptr; // element before right_start
	TreeNode* right_start = start; // fist element of second partition
	ops += 3;
	// While taking next element is bringing us closer to best partition
	while (
		std::abs(left_sum * 2 - total) >
		std::abs((left_sum + static_cast<int64_t>(right_start->frequency)) * 2 - total))
	{
		// Take it and add it's frequency to left_sum
		left_sum += right_start->frequency;
		right_start_prev = right_start;
		right_start = right_start->right;
		ops += 8 + 3;
	}
	ops += 8;

	right_start_prev->right = nullptr;
	ops += 5;

	return new TreeNode(split_list(start, left_sum), split_list(right_start, total - left_sum));
}

// Creates a Shannon-Fano code tree from specified frequencies dictionary
TreeNode* create_shannon_fano_code_tree(std::map<char32_t, uint64_t>* frequencies)
{
	// Beginning of a linked list of nodes
	TreeNode* start = nullptr;
	TreeNode** next_ptr = &start; // pointer to the next element of a last node
	int64_t total_frequency = 0; // total frequency of all characters
	ops += 3;
	for (std::pair<char32_t, uint64_t> p : *frequencies)
	{
		*next_ptr = new TreeNode(p.first, p.second);
		total_frequency += (*next_ptr)->frequency;
		next_ptr = &(*next_ptr)->right;
		ops += 3 + 6;
	}

	ops += 3 + 2;
	return split_list(start, total_frequency);
}

// Creates code table using specified tree root
std::map<char32_t, std::vector<int>>* create_code_table(TreeNode* root)
{
	std::map<char32_t, std::vector<int>>* table = new std::map<char32_t, std::vector<int>>();
	std::vector<int> stack{};
	root->fill_code_table(table, stack);
	ops += 3;
	return table;
}

// Compresses specified file using specified compression method
void compress(const char* source, const char* target, bool use_huffman)
{
	bit_istream in = bit_istream(source);
	bit_ostream out = bit_ostream(target);
	ops += 2;

	// Get characters frequencies from input file
	std::map<char32_t, uint64_t>* frequencies = read_frequencies(in);

	ops += 2;
	// Create code tree using frequencies and specified method
	TreeNode* treeRoot = use_huffman
		? create_huffman_code_tree(frequencies)
		: create_shannon_fano_code_tree(frequencies);

	ops += 1;
	// Write code tree at the beginning of a output file
	treeRoot->write_tree(out);
	
	ops += 1;
	// Create bit vector lookup for each presented in file character
	std::map<char32_t, std::vector<int>>* codeTable = create_code_table(treeRoot);

	ops += 1;
	// Restar input reader to beginning after read_frequencies method
	in.seek_to_start();
	// Write bit codes to output file for each character in input file
	while (in.next_byte_avaliable())
	{
		char32_t symbol = read_utf8_character(in);
		for (int& bit : codeTable->find(symbol)->second)
		{
			out.write_bit(bit);
			ops += 3;
		}
		ops += 3;
	}

	ops += 4;
	// Write finishing character to the end of an output file
	for (int& bit : codeTable->find(EOF_CHAR)->second)
	{
		out.write_bit(bit);
		ops += 3;
	}
	ops += 3;

	// Frees allocated resources
	delete frequencies;
	delete treeRoot;
	delete codeTable;
}

// Decompresses specified file
void decompress(const char* source, const char* target)
{
	bit_istream in = bit_istream(source);
	bit_ostream out = bit_ostream(target);
	ops += 2;

	// Read code tree from input file
	// Huffman and Shannon-Fano trees are stored in the same way
	ops += 2;
	TreeNode* root = TreeNode::read_tree(in);

	// Read bits from input files as corresponding unicode character
	// until finishing character is read
	// and write unicode character to output file
	char32_t symbol;
	while ((symbol = root->read_code(in)) != EOF_CHAR)
	{
		ops += 3;
		write_utf8_character(symbol, out);
	}
	ops += 3;

	// Frees allocated resources
	delete root;
}

// Prints the usage of the program to standard output
void print_usage()
{
	std::cout <<
		"USAGE:\n"
		"Compression %method% %action% %input_file% [%output_file%]\n"
		"\t%method% Specifies a compression method\n"
		"\t\tUse -h for Huffman coding\n"
		"\t\tUse -sf for Shannon-Fano coding\n"
		"\t%action% Specifies if compress or decompress input file\n"
		"\t\tUse -c to compress file\n"
		"\t\tUse -d to decompress file\n"
		"\tIf %output_file% is omitted, then program will use %input_file%.txt\n"
		"\tand compressed file name will be %input_file%.haff or %input_file%.shan"
		<< std::endl;
}

// Concatenates to c-style strings
char* concat(char* left, char* right)
{
	int left_length = strlen(left);
	int right_length = strlen(right);

	char* str = new char[left_length + right_length + 1];
	memcpy(str, left, left_length);
	memcpy(str + left_length, right, right_length);
	str[left_length + right_length] = '\0';

	return str;
}

// Entry point of the program
int main(int argc, char* argv[])
{
	// Print usage if arguments count is invalid
	if (argc < 4 || argc > 5)
	{
		print_usage();
		return 0;
	}

	// Check if use Huffman or Shannon-Fano coding
	bool use_huffman;
	if (strcmp(argv[1], "-h") == 0)
		use_huffman = true;
	else if (strcmp(argv[1], "-sf") == 0)
		use_huffman = false;
	else
	{
		print_usage();
		return 0;
	}

	// Check if program need to compress or decompress a specified file
	bool do_compress;
	if (strcmp(argv[2], "-c") == 0 || strcmp(argv[2], "-c_debug") == 0)
		do_compress = true;
	else if (strcmp(argv[2], "-d") == 0 || strcmp(argv[2], "-d_debug") == 0)
		do_compress = false;
	else
	{
		print_usage();
		return 0;
	}

	char* source;
	char* target;

	// Create source and target file names using specified command line arguments
	if (argc == 4)
	{
		if (use_huffman)
		{
			if (do_compress)
			{
				source = concat(argv[3], ".txt");
				target = concat(argv[3], ".haff");
			}
			else
			{
				source = concat(argv[3], ".haff");
				target = concat(argv[3], "-unz-h.txt");
			}
		}
		else
		{
			if (do_compress)
			{
				source = concat(argv[3], ".txt");
				target = concat(argv[3], ".shan");
			}
			else
			{
				source = concat(argv[3], ".shan");
				target = concat(argv[3], "-unz-s.txt");
			}
		}
	}
	else
	{
		source = argv[3];
		target = argv[4];
	}

	if (do_compress)
		compress(source, target, use_huffman);
	else
		decompress(source, target);

	// Frees allocated resources if they were allocated dynamically
	if (argc == 4)
	{
		delete[] source;
		delete[] target;
	}

	if (strcmp(argv[2], "-c_debug") == 0 || strcmp(argv[2], "-d_debug") == 0)
		std::cout << ops;
	std::cout.flush();
}