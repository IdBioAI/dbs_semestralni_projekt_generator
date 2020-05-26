using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Generator
{
    class Program
    {
        static String connectionString;

        static int NumMachine = 20;

        static Machine[] machines;
        static Timer[] myTimer;

        static Timer autoOff;
        static int autoOfftime = 10; // sec
        static bool stop = true;

        static int sum = 0;

        static String schema = "";

        static MySqlConnection connection;
        static MySqlCommand mySqlCommand;

        private static Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        static void Main(string[] args)
        {
            byte c = 1;

            if (InitDatabase())
            {
                while (c != 0)
                {
                    Console.WriteLine("      Generator dat");
                    Console.WriteLine("[1] - spustit generator");
                    Console.WriteLine("[2] - nastaveni generatoru");
                    Console.WriteLine("[3] - zastavit generator");
                    Console.WriteLine("[0] - konec");
                    try
                    {
                        c = System.Convert.ToByte(Console.ReadLine());
                    }
                    catch
                    {
                        continue;
                    }

                    if (c == 1)
                    {
                        //DeleteAll();
                        InitMachines();
                        Start();
                    }

                    else if (c == 2)
                    {
                        Settings();
                    }

                    else if (c == 3)
                    {
                        Stop(null, null);

                    }
                    else if (c == 0)
                    {
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine("Pripojeni k databazi se nezdarilo.");
                Console.ReadLine();
            }
        }

        private static void DeleteAll()
        {
            connection = Program.GetConnection();

            connection.Open();

            mySqlCommand = new MySqlCommand("DELETE FROM udajetenzometr", connection);
            mySqlCommand.ExecuteNonQuery();

            mySqlCommand = new MySqlCommand("DELETE FROM udajestroj", connection);
            mySqlCommand.ExecuteNonQuery();

            mySqlCommand = new MySqlCommand("DELETE FROM iotprvek", connection);
            mySqlCommand.ExecuteNonQuery();

            mySqlCommand = new MySqlCommand("DELETE FROM stroj", connection);
            mySqlCommand.ExecuteNonQuery();

            connection.Close();

        }

        static private bool InitDatabase()
        {
            try
            {
                String[] con = File.ReadAllLines("conf.txt");
                connectionString = "SERVER=" + con[0] + ";" + "DATABASE=" +
                    con[1] + ";" + "UID=" + con[2] + ";" + "PASSWORD=" + con[3] + ";";

                schema = con[1];

                MySqlConnection connection = new MySqlConnection(connectionString);

                connection.Open();
                connection.Close();
                return true;

            }
            catch (Exception ex)
            {

                Console.WriteLine("ERROR: " + ex.Message);

                return false;
            }

        }

        static private void Settings()
        {
            byte b = 1;

            Console.WriteLine("      Generator dat - nastaveni");
            Console.WriteLine("[1] - pocet stroju (" + NumMachine + ")");
            Console.WriteLine("[2] - auto vypnuti generace (" + autoOfftime + ")");
            Console.WriteLine("[0] - zpet");

            try
            {
                b = System.Convert.ToByte(Console.ReadLine());
            }
            catch
            {
                return;
            }

            if (b == 1)
            {
                Console.Write("zadejte pocet stroju: ");
                NumMachine = System.Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("");
            }

            if (b == 2)
            {
                Console.Write("zadejte pocet sekund: ");
                autoOfftime = System.Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("");
            }


        }

        static private void InitMachines()
        {
            machines = new Machine[NumMachine];
            for (int i = 0; i < NumMachine; i++)
            {
                machines[i] = new Machine();
            }

            AddMachinesToDatabase(machines);
        }

        private static void AddMachinesToDatabase(Machine[] machines)
        {
            connection = Program.GetConnection();

            for (int i = 0; i < machines.Length; i++)
            {
                connection.Open();
                mySqlCommand = new MySqlCommand("INSERT INTO " + Program.GetSchema() + ".stroj " +
                                                            "(VIN, Popis, Nazev, DatumPridani)" +
                                                            " VALUES (N'" + machines[i].GetVIN() +
                                                            "', N'" + "stroj" +
                                                            "', N'" + "stroj-" + RandomString(4) +
                                                            "', '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") +
                                                            "');", connection);

                mySqlCommand.ExecuteNonQuery();

                mySqlCommand = new MySqlCommand("INSERT INTO " + Program.GetSchema() + ".IoTPrvek " +
                                                           "(SerioveCislo, Nazev, Popis, DatumPridani)" +
                                                           " VALUES (N'" + machines[i].GetSN() +
                                                           "', N'" + "strojSN-" + RandomString(4) +
                                                           "', N'" + "strojSN" +
                                                           "', '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") +
                                                           "');", connection);

                mySqlCommand.ExecuteNonQuery();

                connection.Close();

            }
        }

        public static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static private void Start()
        {

            sum = 0;
            stop = false;

            myTimer = new Timer[NumMachine];
            for (int i = 0; i < NumMachine; i++)
            {
                myTimer[i] = new Timer(machines[i].GetRandomTimeSend() * 100);
                myTimer[i].Elapsed += machines[i].SendData;
                myTimer[i].AutoReset = true;
                myTimer[i].Enabled = true;
            }

            autoOff = new Timer(autoOfftime * 1000);
            autoOff.Elapsed += Stop;
            autoOff.AutoReset = false;
            autoOff.Enabled = true;

            Console.WriteLine("!!generace spustena!!");
        }

        static private void Stop(Object source, ElapsedEventArgs e)
        {
            stop = true;
            for (int i = 0; i < NumMachine; i++)
            {
                myTimer[i].Dispose();
            }

            Console.WriteLine("!!generace ukoncena!!");
            Console.WriteLine("celkem zapsano mereni: " + sum);
        }

        static public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        static public bool GetStop()
        {
            return stop;
        }

        static public void AddSum()
        {
            sum++;
        }

        static public String GetSchema()
        {
            return schema;
        }

    }
}
