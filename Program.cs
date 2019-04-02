using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCD
{
	class Program
	{
		static void Main(string[] args)
		{
			FCDocument fcd = new FCDocument();
			fcd.Load("example.txt");
		}
	}
}
