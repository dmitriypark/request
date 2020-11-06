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
    class AsyncScheduler
    {
        public class Sheduler
        {
           

            Aim[] aims;
            Agent[] agents;


            public Sheduler(int aims, int agents)
            {
                this.aims = new Aim[aims];
                this.agents = new Agent[agents];
                
                for (int i=0;i<this.aims.Length;i++)
                {
                    this.aims[i] = new Aim();
                }

                for (int i = 0; i < this.agents.Length; i++)
                {
                    this.agents[i] = new Agent();
                }
            }

            

            public void SetAims(Aim[] aims)
            {
                var currentDirectiry = AppDomain.CurrentDomain.BaseDirectory;
                if (File.Exists(@"" + currentDirectiry + "request_sheduler.txt"))
                {
                    
                    var strArray = File.ReadAllLines(@"" + currentDirectiry + "request_sheduler.txt");
                    List<int> intArray = new List<int>();
                    foreach (var str in strArray)
                    {
                        var startIndex = str.IndexOf(")");

                        var originalString = str.Remove(startIndex);
                        var integer = Int32.Parse(originalString);

                        intArray.Add(integer);

                    }
                    int[] allAimNumbers = Enumerable.Range(1, 2018).ToArray();
                    var allListAimNumbers= allAimNumbers.ToList();
                    foreach(var number in intArray)
                    {
                        allListAimNumbers.Remove(number);
                    }


                    for (int i = 0; i < aims.Length; i++)
                    {
                        aims[i].number = i + 1;

                    }

                    foreach(var aim in aims)
                    {
                        aim.done = true;
                        aim.free = false;
                    }

                    foreach (var numb in allListAimNumbers)
                    {
                        aims.Where(a => a.number == numb).FirstOrDefault().free = true;
                        aims.Where(a => a.number == numb).FirstOrDefault().done = false;
                    }

                }
                else
                {
                    for (int i = 0; i < aims.Length; i++)
                    {
                        aims[i].number = i + 1;

                    }
                }
                

                
            }


            public IEnumerable<Aim> SelectFreeAims(Aim[] aims,int buffer)
            {
                
               return aims.Where(a => a.free == true & a.done==false).Take(buffer);
                
            }

            public bool CheckAims(Aim[] aims)
            {
                
                foreach (var aim in aims)
                {
                    if (!aim.done)
                        return false;
                }
                return true;

            }


            public IEnumerable<Agent> TakeFreeAgents(Agent [] agents)
            {

               return agents.Where(a => a.free == true & a.result==null);
               
            }


            public IEnumerable<Agent> TakeResultDoneAgents(Agent [] agents)
            {
                return agents.Where(a => a.result != null & a.free==false & a.number!=-1 & a.exept !=true);
            }


            public void WriteResults__CheckAimsAndAgents(Agent [] agents)
            {
                

                

                var currentDirectiry = AppDomain.CurrentDomain.BaseDirectory;
                
                

                foreach (var agent in TakeResultDoneAgents(agents))
                {
                    
                    File.AppendAllText(@"" + currentDirectiry + "request_sheduler.txt", agent.number + ") " + agent.result+"\n");
                    var aim = aims.Where(a => a.number == agent.number).FirstOrDefault();
                    aim.done = true;
                    aim.free = false;
                    agent.result = null;
                    
                    agent.number = -1;
                    agent.free = true;
                }


                var AgentsWithoutResult = agents.Where(a => a.number != -1 & a.exept==true);

                foreach (var agent in AgentsWithoutResult)
                {
                    
                    var aim = aims.Where(a => a.number == agent.number).FirstOrDefault();
                    aim.free = true;
                    agent.result = null;
                    agent.number = -1;
                    agent.exept = false;
                    agent.free = true;
                }



            }




            public async void AgentAimStart(Aim aim, Agent agent)
            {
                await Task.Run(() => agent.NumberRequest(aim));
            }


            public void MainFrame()
            {
                SetAims(aims);
                while (!CheckAims(aims))
                {
                    
                    var aim_buffer = SelectFreeAims(aims, agents.Length).GetEnumerator();
                    var free_agents = TakeFreeAgents(agents);




                    foreach (var agent in free_agents)
                    {
                        var end = aim_buffer.MoveNext();
                        if (end)
                        {
                            aim_buffer.Current.free = false;
                            agent.free = false;

                            AgentAimStart(aim_buffer.Current, agent);
                        }
                        else
                        {
                            aim_buffer = SelectFreeAims(aims, agents.Length).GetEnumerator();
                            if (SelectFreeAims(aims, agents.Length).Count() == 0)
                                break;
                        }
                            
                                
                    }

                    Thread.Sleep(1000);


                    WriteResults__CheckAimsAndAgents(agents);
                }


               

            }

            
        }

        public  class Aim
        {
            public  int number { get; set; }
            public  bool free { get; set; } = true;
            public  bool done { get; set; } = false;
            
        }


        


        public class Agent
        {
            private const string port = "2012";
            private const string server = "88.212.241.115";

            public  bool free { get; set; } = true;
            public  string result { get; set; } = null;
            public bool exept { get; set; } = false;
            public int number { get; set; } = -1;

            
            


            public void NumberRequest(Aim aim)
            {

                number = aim.number;
                

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ip = IPAddress.Parse(server);
                IPEndPoint ipe = new IPEndPoint(ip, int.Parse(port));
                var end = (EndPoint)ipe;




                try
                {
                    socket.Connect(ipe);
                    string str = aim.number.ToString() + "\n";

                    byte[] buffer = Encoding.Default.GetBytes(str);


                    byte[] answer = new byte[100];


                    socket.Send(buffer, buffer.Length, 0);


                    Thread.Sleep(120000);

                    socket.ReceiveFrom(answer, ref end);



                    bool check = false;


                    for (int k = 0; k < answer.Length; k++)
                    {
                        if (answer[k] == 10)
                        {

                            check = true;

                        }
                    }

                    if (check)
                    {
                        var strAns = Encoding.Default.GetString(answer.ToArray());
                        var originalString = strAns.Replace(" ", "");
                        originalString = originalString.Replace("\t", "");
                        originalString = originalString.Replace("\r", "");
                        originalString = originalString.Replace("\n", "");
                        var strNumber = originalString.Replace(".", "");

                        result = strNumber;
                        

                    }
                    else
                    {
                        exept = true;
                        result = null;
                        
                        
                    }
                }
                catch (Exception ex)
                {
                    exept = true;
                    result = null;
                    
                    
                    
                    

                    //Console.WriteLine($"Исключение: {ex.Message}");

                }

            }
        }
    }
}
