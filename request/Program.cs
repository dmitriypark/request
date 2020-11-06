using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace request
{
    class Program
    {

        static void Main(string[] args)
        {
            
            Program.SheduleStart();
            var currentDirectiry = AppDomain.CurrentDomain.BaseDirectory;
            var strArray = File.ReadAllLines(@"" + currentDirectiry + "request_sheduler.txt");
            var intArray = GetArrayFromString(strArray);
            Array.Sort(intArray);
            var median = median_of_an_even_number_series(intArray);


            Console.WriteLine("Медиана:" + median);
            Console.ReadKey();

        }




        public static void SheduleStart()
        {
            
            AsyncScheduler.Sheduler sheduler = new AsyncScheduler.Sheduler(2018, 500);
            sheduler.MainFrame();
        }


        public static int[] GetArrayFromString(string [] strArray)
        {
            List<int> intArray=new List<int>();
            
            foreach (var str in strArray)
            {
                var Index = str.IndexOf(")");
                var originalString = str.Remove(0, Index+1);
                var integer = Int32.Parse(originalString);
                intArray.Add(integer);
                
            }
            
            return intArray.ToArray();

            
        }


        public static double median_of_an_even_number_series(int [] array)
        {
            var middle = array.Length / 2;
            return (Convert.ToDouble(array[middle - 1]) + Convert.ToDouble(array[middle])) / 2;
        }


        


        

    }
    



}
